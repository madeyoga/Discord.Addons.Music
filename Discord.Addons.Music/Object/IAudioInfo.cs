namespace Discord.Addons.Music.Object
{
    public interface IAudioInfo
    {
        string Url { get; set; }
        string Title { get; set; }
        string Author { get; set; }
        string ThumbnailUrl { get; set; }
        string Duration { get; set; }
    }
}
