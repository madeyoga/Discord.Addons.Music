using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Source
{
    public class AudioTrack : FFmpegAudioSource
    {
        public byte[] BufferFrame = new byte[1024];

        public override void LoadProcess()
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
