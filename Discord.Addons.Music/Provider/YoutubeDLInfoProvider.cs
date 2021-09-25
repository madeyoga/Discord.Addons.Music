using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Provider
{
    public class YoutubeDLInfoProvider
    {
        public static async Task<JObject> ExtractInfo(string query, bool isUrl=true)
        {
            string arguments = $" --dump-single-json  ytsearch:" + query;

            if (isUrl)
            {
                arguments = $" --dump-single-json  " + query;
            }

            Process ytdlProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = arguments,
                RedirectStandardOutput = true
            });

            string jsonString = await ytdlProcess.StandardOutput.ReadToEndAsync();

            return JObject.Parse(jsonString);
        }
    }
}
