using System;
using System.Diagnostics;
using System.IO;

namespace Discord.Addons.Music.Object
{
    public interface IAudioTrack : IDisposable
    {
        IAudioInfo Info { get; set; }
        public Stream SourceStream { get; set; }
        public Process FFmpegProcess { get; set; }
        public string Url { get; set; }
        void LoadProcess();
    }
}
