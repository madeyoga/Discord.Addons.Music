using Discord.Addons.Music.Core;
using Discord.Addons.Music.Exception;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Common
{
    public class DefaultAudioEventAdapter : AudioEventAdapter
    {
        public void OnTrackEnd(AudioTrack track)
        {
        }

        public void OnTrackError(AudioTrack track, TrackErrorException exception)
        {
        }

        public void OnTrackStart(AudioTrack track)
        {
        }

        public Task OnTrackStartAsync(AudioTrack track)
        {
            throw new NotImplementedException();
        }

        public void OnTrackStuck(AudioTrack track, TrackStuckException exception)
        {
        }
    }
}
