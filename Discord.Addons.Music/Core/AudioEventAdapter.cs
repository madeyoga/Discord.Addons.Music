using Discord.Addons.Music.Exception;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Core
{
    public interface AudioEventAdapter
    {
        public void OnTrackStart(AudioTrack track);

        public void OnTrackEnd(AudioTrack track);

        public void OnTrackError(AudioTrack track, TrackErrorException exception);

        public void OnTrackStuck(AudioTrack track, TrackStuckException exception);

        public Task OnTrackStartAsync(AudioTrack track);
    }
}
