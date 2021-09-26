using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Discord.Addons.Music.Source
{
    public class FFmpegAudio : IAudioSource, IDisposable
    {
        Process ffmpegProcess;

        public FFmpegAudio(string source, string executable = "ffmpeg", List<string> args = null)
        {
            args.Insert(0, executable);
        }

        public bool IsOpus()
        {
            return true;
        }

        public byte[] Provide20msAudio()
        {
            throw new NotImplementedException();
        }

        protected Process SpawnProcess(List<string> args)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = string.Join(' ', args),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        public void Dispose()
        {
            if (ffmpegProcess != null)
            {
                ((IDisposable)ffmpegProcess).Dispose();
                ffmpegProcess.Kill();
            }
        }
    }
}
