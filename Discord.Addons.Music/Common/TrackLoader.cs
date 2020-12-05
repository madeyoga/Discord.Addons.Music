using Discord.Addons.Music.Provider;
using System;
using System.Threading.Tasks;
using Discord.Addons.Music.Core;
using System.IO;
using System.Diagnostics;
using Discord.Addons.Music.Exception;
using Discord.Addons.Music.Objects;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Web;

namespace Discord.Addons.Music.Common
{
    public class TrackLoader
    {
        public static async Task<List<AudioTrack>> LoadAudioTrack(string query, bool fromUrl = true)
        {
            //query = HttpUtility.UrlEncode(query);

            JObject ytdlResponseJson = await YoutubeDLInfoProvider.ExtractInfo(query, fromUrl);

            List<AudioTrack> songs = new List<AudioTrack>();

            // Check if playlist
            if (ytdlResponseJson.ContainsKey("entries"))
            {
                if (fromUrl)
                {
                    foreach (JObject ytdlVideoJson in ytdlResponseJson["entries"].Value<JArray>())
                    {
                        SongInfo songInfo = SongInfo.ParseYtdlResponse(ytdlVideoJson);
                        songs.Add(new AudioTrack()
                        {
                            Url = songInfo.Url,
                            FFmpegProcess = LoadFFmpegProcess(songInfo.Url),
                            TrackInfo = songInfo
                        });
                    }
                }
                else
                {
                    JObject ytdlVideoJson = ytdlResponseJson["entries"].Value<JArray>()[0].Value<JObject>();
                    SongInfo firstEntrySong = SongInfo.ParseYtdlResponse(ytdlVideoJson);
                    songs.Add(new AudioTrack()
                    {
                        Url = firstEntrySong.Url,
                        FFmpegProcess = LoadFFmpegProcess(firstEntrySong.Url),
                        TrackInfo = firstEntrySong
                    });
                }
            }
            else
            {
                SongInfo songInfo = SongInfo.ParseYtdlResponse(ytdlResponseJson);
                songs.Add(new AudioTrack()
                {
                    Url = songInfo.Url,
                    FFmpegProcess = LoadFFmpegProcess(songInfo.Url),
                    TrackInfo = songInfo
                });
            }
            return songs;
        }

        public static async Task LoadAudioTrack(string query, AudioLoadResultHandler handler, bool fromUrl = true)
        {
            JObject ytdlResponseJson = await YoutubeDLInfoProvider.ExtractInfo(query, fromUrl);

            List<AudioTrack> songs = new List<AudioTrack>();

            // Check if playlist
            if (ytdlResponseJson.ContainsKey("entries"))
            {
                if (fromUrl)
                {
                    foreach (JObject ytdlVideoJson in ytdlResponseJson["entries"].Value<JArray>())
                    {
                        SongInfo songInfo = SongInfo.ParseYtdlResponse(ytdlVideoJson);
                        songs.Add(new AudioTrack()
                        {
                            Url = songInfo.Url,
                            FFmpegProcess = LoadFFmpegProcess(songInfo.Url),
                            TrackInfo = songInfo
                        });
                    }
                    handler.OnLoadPlaylist(songs);
                }
                else
                {
                    JObject ytdlVideoJson = ytdlResponseJson["entries"].Value<JArray>()[0].Value<JObject>();
                    SongInfo firstEntrySong = SongInfo.ParseYtdlResponse(ytdlVideoJson);
                    handler.OnLoadTrack(new AudioTrack()
                    {
                        Url = firstEntrySong.Url,
                        FFmpegProcess = LoadFFmpegProcess(firstEntrySong.Url),
                        TrackInfo = firstEntrySong
                    });
                }
            }
            else
            {
                SongInfo songInfo = SongInfo.ParseYtdlResponse(ytdlResponseJson);
                handler.OnLoadTrack(new AudioTrack()
                {
                    Url = songInfo.Url,
                    FFmpegProcess = LoadFFmpegProcess(songInfo.Url),
                    TrackInfo = songInfo
                });
            }
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
    }
}
