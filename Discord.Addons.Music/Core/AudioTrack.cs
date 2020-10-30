using Discord.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Core
{
    public class AudioTrack
    {
        public Stream SourceStream { get; set; }

        // Track Info
        // ..

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
            SourceStream.Dispose();
            SourceStream.Close();
        }
    }
}
