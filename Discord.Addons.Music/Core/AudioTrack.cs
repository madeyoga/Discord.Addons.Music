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

        public AudioTrack(Process ffmpegProcess)
        {
            FFmpegProcess = ffmpegProcess;
            SourceStream = ffmpegProcess.StandardOutput.BaseStream;
        }

        public AudioTrack MakeClone()
        {
            Stream streamClone = new MemoryStream();
            SourceStream.CopyTo(streamClone);

            return new AudioTrack()
            {
                Url = Url,
                SourceStream = streamClone,
                FFmpegProcess = null,
                TrackInfo =TrackInfo
            };
        }

        ~AudioTrack()
        {
            if (FFmpegProcess != null)
            {
                FFmpegProcess.Dispose();
                FFmpegProcess.Close();
            }
            if (SourceStream != null)
            {
                SourceStream.Dispose();
                SourceStream.Close();
            }
        }
    }
}
