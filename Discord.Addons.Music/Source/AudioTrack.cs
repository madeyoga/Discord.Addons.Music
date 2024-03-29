﻿using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Source
{
    public class AudioTrack : FFmpegAudioSource
    {
        public byte[] BufferFrame = new byte[1024];

        public override void LoadProcess()
        {
            string filename = $"/bin/bash";
            string command = $"-c \"youtube-dl --format bestaudio -o - {Url} | ffmpeg -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1\"";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                filename = "cmd.exe";
                command = $"/C youtube-dl.exe --format bestaudio --audio-quality 0 -o - {Url} | " +
                "ffmpeg.exe -loglevel warning -re -vn -i pipe:0 -f s16le -b:a 128k -ar 48000 -ac 2 pipe:1";
            }

            FFmpegProcess = Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            SourceStream = FFmpegProcess.StandardOutput.BaseStream;
        }

        public override async Task<int> Provide20msAudio(CancellationToken ct)
        {
            return await SourceStream.ReadAsync(BufferFrame, 0, BufferFrame.Length, ct).ConfigureAwait(false);
        }

        public override bool IsOpus()
        {
            return false;
        }

        public override byte[] GetBufferFrame()
        {
            return BufferFrame;
        }

        public AudioTrack MakeClone()
        {
            Stream streamClone = new MemoryStream();
            SourceStream.CopyTo(streamClone);

            return new AudioTrack()
            {
                Url = Url,
                SourceStream = null,
                FFmpegProcess = null,
                Info = Info
            };
        }
    }
}
