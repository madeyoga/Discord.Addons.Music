using Discord.Addons.Music.Core;
using Discord.Addons.Music.Exception;
using Discord.Audio;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Audio
{
    public interface IAudioEvent
    {
        delegate Task TrackStartAsync(IAudioClient audioClient, AudioTrack track);
        event TrackStartAsync OnTrackStartAsync;

        delegate Task TrackEndAsync(IAudioClient audioClient, AudioTrack track);
        event TrackEndAsync OnTrackEndAsync;

        delegate Task TrackErrorAsync(IAudioClient audioClient, AudioTrack track, TrackErrorException exception);
        event TrackErrorAsync OnTrackErrorAsync;
    }
}
