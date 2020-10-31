using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Provider
{
    public class YoutubeInfoProvider
    {
        public static async Task<string> GetVideoInfoByUrlAsync(string videoUrl)
        {
            Process ytdlProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = $" --dump-json " + videoUrl,
                RedirectStandardOutput = true
            });

            return await ytdlProcess.StandardOutput.ReadToEndAsync();
        }
    }
}
