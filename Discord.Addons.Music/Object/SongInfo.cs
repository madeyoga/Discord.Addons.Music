using Discord.Addons.Music.Object;
using Newtonsoft.Json.Linq;

namespace Discord.Addons.Music.Objects
{
    public class SongInfo : IAudioInfo
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Duration { get; set; }

        public SongInfo(string id, string url, string title, string author, string thumbnailUrl, string duration)
        {
            Id = id;
            Url = url;
            Title = title;
            Author = author;
            ThumbnailUrl = thumbnailUrl;
            Duration = duration;
        }

        public static SongInfo ParseYtdlResponse(JObject ytdlResponseObject)
        {
            string id = ytdlResponseObject["id"].Value<string>();
            string url = ytdlResponseObject["webpage_url"].Value<string>();
            string title = ytdlResponseObject["title"].Value<string>();
            string author = ytdlResponseObject["uploader"].Value<string>();
            string thumbnailUrl = ytdlResponseObject["thumbnails"][0]["url"].Value<string>();
            string duration = ytdlResponseObject["duration"].Value<string>();

            return new SongInfo(id, url, title, author, thumbnailUrl, duration);
        }
    }
}
