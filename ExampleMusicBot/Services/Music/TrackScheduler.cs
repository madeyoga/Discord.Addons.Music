using Discord.Addons.Music.Audio;
using Discord.Addons.Music.Core;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nano.Net.Services.Music
{
    public class TrackScheduler
    {
        Queue<AudioTrack> SongQueue { get; set; }

        private AudioPlayer player;

        public TrackScheduler(AudioPlayer player)
        {
            SongQueue = new Queue<AudioTrack>();
            this.player = player;
            this.player.OnTrackStartAsync += OnTrackStartAsync;
            this.player.OnTrackEndAsync += OnTrackEndAsync;
        }

        public async Task EnqueueAsync(AudioTrack track)
        {
            if (await player.StartTrackAsync(track, false) == false)
            {
                SongQueue.Enqueue(track);
            }
        }

        public async Task OnTrackStartAsync(IAudioClient audioClient, AudioTrack track)
        {
            Console.WriteLine("Track start! " + track.Info.Title);

            if (SongQueue.Count > 0)
            {
                AudioTrack nextTrack = SongQueue.Dequeue();
                Console.WriteLine("Next track " + nextTrack.Info.Title);
                await player.StartTrackAsync(nextTrack);
            }
        }

        public async Task OnTrackEndAsync(IAudioClient audioClient, AudioTrack track)
        {
            Console.WriteLine("Track end! " + track.Info.Title);

            if (SongQueue.Count > 0)
            {
                AudioTrack nextTrack = SongQueue.Dequeue();
                Console.WriteLine("Next track " + nextTrack.Info.Title);
                await player.StartTrackAsync(nextTrack);
            }
        }
    }
}
