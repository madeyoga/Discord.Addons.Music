using Discord.Addons.Music.Object;
using System;
using System.Diagnostics;
using System.IO;

namespace Discord.Addons.Music.Core
{
    public class AudioTrack : IAudioTrack
    {
        public Stream SourceStream { get; set; }
        public Process FFmpegProcess { get; set; }
        public string Url { get; set; }
        public IAudioInfo Info { get; set; }

        public void LoadProcess()
        {
            string command = $"/C youtube-dl.exe --format bestaudio --audio-quality 0 -o - {Url} | " +
                "ffmpeg.exe -loglevel warning -re -vn -i pipe:0 -f s16le -b:a 128k -ar 48000 -ac 2 pipe:1";
            FFmpegProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            SourceStream = FFmpegProcess.StandardOutput.BaseStream;
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
                Info = Info
            };
        }

        public void Dispose()
        {
            ((IDisposable)SourceStream).Dispose();
            FFmpegProcess.Dispose();
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
