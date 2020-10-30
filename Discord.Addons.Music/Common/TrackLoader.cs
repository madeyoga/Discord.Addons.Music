using Discord.Addons.Music.Provider;
using System;
using System.Threading.Tasks;
using Discord.Addons.Music.Core;

namespace Discord.Addons.Music.Common
{
    public class TrackLoader
    {
        public static async Task <AudioTrack> LoadYoutubeTrack(string url)
        {
            string songInfo = await YoutubeInfoProvider.GetVideoInfoByUrlAsync(url);
            Console.WriteLine(songInfo);
            return new AudioTrack()
            {
                Url = url
            };
        }
    }
}
