using Discord.Addons.Music.Exception;
using Discord.Addons.Music.Source;
using Discord.Audio;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Player
{
    public interface IAudioEvent
    {
        delegate Task TrackStartAsync(IAudioClient audioClient, IAudioSource track);
        event TrackStartAsync OnTrackStartAsync;

        delegate Task TrackEndAsync(IAudioClient audioClient, IAudioSource track);
        event TrackEndAsync OnTrackEndAsync;

        delegate Task TrackErrorAsync(IAudioClient audioClient, IAudioSource track, TrackErrorException exception);
        event TrackErrorAsync OnTrackErrorAsync;
    }
}
