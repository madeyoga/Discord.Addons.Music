using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Discord.Addons.Music.Objects
{
    public class SongInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Duration { get; set; }

        public SongInfo(string id, string title, string author, string thumbnailUrl, string duration)
        {
            Id = id;
            Title = title;
            Author = author;
            ThumbnailUrl = thumbnailUrl;
            Duration = duration;
        }

        public static SongInfo ParseYtdlResponse(string jsonString)
        {
            JObject ytdlResponseObject = JObject.Parse(jsonString);
            
            string id = ytdlResponseObject["id"].Value<string>();
            string title = ytdlResponseObject["title"].Value<string>();
            string author = ytdlResponseObject["uploader"].Value<string>();
            string thumbnailUrl = ytdlResponseObject["thumbnails"][0]["url"].Value<string>();
            string duration = ytdlResponseObject["duration"].Value<string>();

            return new SongInfo(id, title, author, thumbnailUrl, duration);
        }
    }
}
