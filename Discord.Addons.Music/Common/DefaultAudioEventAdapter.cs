using Discord.Addons.Music.Core;
using Discord.Addons.Music.Exception;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Common
{
    public class DefaultAudioEventAdapter : IAudioEventAdapter
    {
        public void OnTrackEnd(AudioTrack track)
        {
            throw new NotImplementedException();
        }

        public void OnTrackError(AudioTrack track, TrackErrorException exception)
        {
            throw new NotImplementedException();
        }

        public void OnTrackStart(AudioTrack track)
        {
            throw new NotImplementedException();
        }

        public void OnTrackStuck(AudioTrack track, TrackStuckException exception)
        {
            throw new NotImplementedException();
        }

        public Task<Task> OnTrackEndAsync(AudioTrack track)
        {
            throw new NotImplementedException();
        }

        public Task<Task> OnTrackStartAsync(AudioTrack track)
        {
            throw new NotImplementedException();
        }
    }
}
