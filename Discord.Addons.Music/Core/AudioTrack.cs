using Discord.Addons.Music.Objects;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Core
{
    public class AudioTrack
    {
        public Stream SourceStream { get; set; }

        public Process FFmpegProcess { get; set; }

        public SongInfo TrackInfo { get; set; }

        public string Url { get; set; }

        public AudioTrack()
        {

        }

        public AudioTrack(Stream sourceStream)
        {
            SourceStream = sourceStream;
        }

        ~AudioTrack()
        {
            if (SourceStream != null)
            {
                SourceStream.Dispose();
                SourceStream.Close();
            }
        }
    }
}
