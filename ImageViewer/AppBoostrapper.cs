using Caliburn.Micro;
using MetroImageViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bootstrap.Autofac;
using Bootstrap;
using Bootstrap.Extensions;
using Autofac;
using Autofac.Core;
using System.Windows.Threading;
using MemBus;
using MemBus.Configurators;
using MemBus.Subscribing;

namespace MetroImageViewer
{
    /// <summary>
    /// Yeah, I know it's a lot of extra code. But it bugs me not have convenience!
    /// </summary>
    public class AppBootstrapper : Bootstrapper<MainWindowViewModel>
    {
        bool isDesignTime = false;

        protected override void Configure()
        {
            base.Configure();

            if(!isDesignTime)
                SetupDesignMode();

            Application.DispatcherUnhandledException += Application_DispatcherUnhandledException;
        }

        void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.Source);
            Console.WriteLine(e.Exception.Message);
        }

        private void SetupDesignMode()
        {
            Bootstrap.Bootstrapper
                .With.Autofac()
                .And.Start();

            var container = (IContainer)Bootstrap.Bootstrapper.Container;
            var builder = new ContainerBuilder();

            var defaultLocator = ViewLocator.LocateTypeForModelType;

            ViewLocator.LocateTypeForModelType = (modelType, displayLocation, context) =>
            {
                var viewType = defaultLocator(modelType, displayLocation, context);

                while (viewType == null && modelType != typeof(object))
                {
                    modelType = modelType.BaseType;
                    viewType = defaultLocator(modelType, displayLocation, context);
                }

                return viewType;
            };

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(p => p.Name.EndsWith("ViewModel"))
                .Where(type => type.GetInterface(typeof(System.ComponentModel.INotifyPropertyChanged).Name, false) != null)
                .InstancePerDependency();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(p => p.Name.Contains("ViewModel"))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.Register<IWindowManager>(c => new WindowManager()).InstancePerLifetimeScope();

            builder.Register<IBus>(p => BusSetup.StartWith<Conservative>()
                .Apply<FlexibleSubscribeAdapter>(a => a.ByMethodName("Handle"))
                .Construct()).SingleInstance();

            builder.Update(container);
        }

        protected override void StartDesignTime()
        {
            isDesignTime = true;

            base.StartDesignTime();
        }

        protected override object GetInstance(Type service, string key)
        {
            var container = (IContainer)Bootstrap.Bootstrapper.Container;

            object instance;
            if (string.IsNullOrWhiteSpace(key))
            {
                if (container.TryResolve(service, out instance))
                {
                    return instance;
                }
            }
            else
            {
                if (container.TryResolveNamed(key, service, out instance))
                {
                    return instance;
                }
            }
            throw new Exception("Could not find instance of " + key + service);
        }

        protected override void BuildUp(object instance)
        {
            var container = (IContainer)Bootstrap.Bootstrapper.Container;
            container.InjectProperties(instance);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            var container = (IContainer)Bootstrap.Bootstrapper.Container;
            return container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        private Service GetServiceForKey(string key)
        {
            var container = (IContainer)Bootstrap.Bootstrapper.Container;
            var registrations = container.ComponentRegistry.Registrations;

            foreach (var registration in registrations)
            {
                var result = registration.Services.OfType<KeyedService>()
                    .FirstOrDefault(service => service.ServiceKey as string == key ||
                                               service.ServiceType.Name == key);

                if (result != null)
                    return (Service)result;
            }

            return null;
        }
    }
}