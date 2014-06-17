using Caliburn.Micro;
using MemBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroImageViewer.ViewModels
{
    public class MainWindowViewModel : Screen
    {
        private const string SearchPattern = "*.png";

        IBus _bus;

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set 
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
            }
        }

        private IObservableCollection<ImageViewModel> _images;
        public IObservableCollection<ImageViewModel> Images
        {
            get { return _images; }
            set 
            {
                _images = value;
                NotifyOfPropertyChange(() => Images);
            }
        }

        public MainWindowViewModel(IBus bus)
        {
            IsLoading = false;

            _bus = bus;
            bus.Subscribe(this);
            this.DisplayName = "MetroImageViewer Image Viewer";

            var images = new BindableCollection<ImageViewModel>();

            Images = new BindableCollection<ImageViewModel>();

            // Okay, if I had more time I would do the following:
            // Write a cache folder on disk that stores thumbnails of the images.
            // PNGs don't include thumbnails in them, while JPGs do. So a JPG cache would be awesome.
            // The other thing is, I'd write a loading engine that defers loading the image until it's scrolled into view.
            // In WPF, to adhere to string MVVM that's somewhat difficult.
            // I'm going to spend the last 35 minutes I've got hacking something together.

            Task.Run(() =>
            {
                var directoryInfo = new DirectoryInfo(Environment.CurrentDirectory + @"\..\..\images");

                foreach (var image in directoryInfo.GetFiles(SearchPattern))
                {
                    images.Add(new ImageViewModel(image.FullName));
                }

                Images = images;

                IsLoading = true;
            });
        }
    }

    public class MessageTest
    {
        public string MyText { get; set; }
    }

    public class ImageViewModel : Screen
    {

        private string _imagePath;
        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                _imagePath = value;
                NotifyOfPropertyChange(() => ImagePath);
            }
        }

        public ImageViewModel(string path)
        {
            ImagePath = path;
        }

    }
}
