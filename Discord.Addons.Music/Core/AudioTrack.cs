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
            FFmpegProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --format --audio-quality 0 bestaudio -o - {Url} | ffmpeg.exe -re -reconnect 1 -reconnect_streamed 1 -reconnect_delay_max 5 -vn -nostats -loglevel 0 panic -i pipe:0 -c:a libopus -b:a bitrate 96K -ac 2 -f s16le -ar 48000 pipe:1",
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
