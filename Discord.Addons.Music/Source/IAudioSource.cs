namespace Discord.Addons.Music.Source
{
    public interface IAudioSource
    {
        byte[] Provide20msAudio();
        bool IsOpus();
    }
}
