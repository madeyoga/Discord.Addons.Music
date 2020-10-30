using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Discord.Addons.Music.Objects
{
    public class SongInfo
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Duration { get; set; }
    }
}
