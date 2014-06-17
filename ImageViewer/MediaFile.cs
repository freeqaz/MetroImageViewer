using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroImageViewer
{
    public class MediaFile
    {
        public string Artist { get; set; }

        public string Album { get; set; }

        public string Title { get; set; }

        public string Path { get; set; }

        public string Genre { get; set; }

        public string Duration { get; set; }
    }

    public class StatusUpdate
    {
        public MediaFile current { get; set; }

        public bool playing { get; set; }

        public List<string> playlist { get; set; }

        public StatusUpdate()
        {
            playlist = new List<string>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(string.Format("Current: {0}, ", current));
            sb.Append(string.Format("Playing: {0}, ", playing ? "Yes" : "No"));
            sb.Append("Playlists: " + playlist.Count);

            return sb.ToString();
        }
    }
}
