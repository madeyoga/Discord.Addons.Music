﻿using Discord.Addons.Music.Objects;
using Discord.Addons.Music.Source;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;

namespace Discord.Addons.Music.Common
{
    public class TrackLoader
    {
        public static async Task<List<AudioTrack>> LoadAudioTrack(string query, bool fromUrl = true)
        {
            if (!fromUrl)
            {
                query = HttpUtility.UrlEncode(query);
            }

            JObject ytdlResponseJson = await YoutubeDLInfoProvider.ExtractInfo(query, fromUrl).ConfigureAwait(false);

            List<AudioTrack> songs = new List<AudioTrack>();

            // Check if playlist
            if (ytdlResponseJson.ContainsKey("entries"))
            {
                if (fromUrl)
                {
                    foreach (JObject ytdlVideoJson in ytdlResponseJson["entries"].Value<JArray>())
                    {
                        AudioInfo songInfo = AudioInfo.ParseYtdlResponse(ytdlVideoJson);
                        songs.Add(new AudioTrack()
                        {
                            Url = songInfo.Url,
                            Info = songInfo
                        });
                    }
                }
                else
                {
                    JObject ytdlVideoJson = ytdlResponseJson["entries"].Value<JArray>()[0].Value<JObject>();
                    AudioInfo firstEntrySong = AudioInfo.ParseYtdlResponse(ytdlVideoJson);
                    songs.Add(new AudioTrack()
                    {
                        Url = firstEntrySong.Url,
                        Info = firstEntrySong
                    });
                }
            }
            else
            {
                AudioInfo songInfo = AudioInfo.ParseYtdlResponse(ytdlResponseJson);
                songs.Add(new AudioTrack()
                {
                    Url = songInfo.Url,
                    Info = songInfo
                });
            }
            return songs;
        }

        public static Process LoadFFmpegProcess(string url)
        {
            string filename = $"/bin/bash";
            string command = $"-c \"youtube-dl --format bestaudio -o - {url} | ffmpeg -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1\"";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                filename = "cmd.exe";
                command = $"/C youtube-dl.exe --format bestaudio --audio-quality 0 -o - {url} | " +
                "ffmpeg.exe -loglevel warning -re -vn -i pipe:0 -f s16le -b:a 128k -ar 48000 -ac 2 pipe:1";
            }

            return Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }
}
