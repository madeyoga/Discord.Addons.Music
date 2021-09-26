using Discord.Addons.Music.Core;
using Discord.Addons.Music.Objects;
using Discord.Addons.Music.Provider;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
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
                            Info = songInfo
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
                        Info = firstEntrySong
                    });
                }
            }
            else
            {
                SongInfo songInfo = SongInfo.ParseYtdlResponse(ytdlResponseJson);
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
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --format --audio-quality 0 bestaudio -o - {url} | ffmpeg.exe -loglevel panic -i pipe:0 -c:a libopus -b:a 96K -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }
}
