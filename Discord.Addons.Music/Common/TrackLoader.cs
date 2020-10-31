using Discord.Addons.Music.Provider;
using System;
using System.Threading.Tasks;
using Discord.Addons.Music.Core;
using System.IO;
using System.Diagnostics;
using Discord.Addons.Music.Exception;
using Discord.Addons.Music.Objects;

namespace Discord.Addons.Music.Common
{
    public class TrackLoader
    {
        public static async Task <AudioTrack> LoadYoutubeTrack(string url)
        {
            string ytdlResponseJson = await YoutubeInfoProvider.GetVideoInfoByUrlAsync(url);
            SongInfo songInfo = SongInfo.ParseYtdlResponse(ytdlResponseJson);

            return new AudioTrack()
            {
                Url = url,
                FFmpegProcess = LoadFFmpegProcess(url),
                TrackInfo = songInfo
            };
        }

        public static Process LoadFFmpegProcess(string url)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --format bestaudio -o - {url} | ffmpeg.exe -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        public static async Task<Stream> LoadStreamAsync(AudioTrack track)
        {
            Process ffmpegProcess = await Task.Run(() =>
            {
                return Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C youtube-dl.exe --format bestaudio -o - {track.Url} | ffmpeg.exe -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            });

            return ffmpegProcess.StandardOutput.BaseStream;
        }

        public static async Task<Stream> LoadStreamAsync(string url)
        {
            Process ffmpegProcess = await Task.Run(() =>
            {
                return Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C youtube-dl.exe --format bestaudio -o - {url} | ffmpeg.exe -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            });

            return ffmpegProcess.StandardOutput.BaseStream;
        }

        public Stream LoadStream(AudioTrack track)
        {
            Process ffmpegProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --format bestaudio -o - {track.Url} | ffmpeg.exe -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            return ffmpegProcess.StandardOutput.BaseStream;
        }

        public static Stream LoadStream(string Url)
        {
            Process ffmpegProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --format bestaudio -o - {Url} | ffmpeg.exe -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            return ffmpegProcess.StandardOutput.BaseStream;
        }
    }
}
