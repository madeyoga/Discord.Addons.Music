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

        public Task EnqueueAsync(AudioTrack track)
        {
            if (player.StartTrackAsync(track, false) == false)
            {
                SongQueue.Enqueue(track);
            }
            return Task.CompletedTask;
        }

        public Task NextTrack()
        {
            player.StartTrackAsync(SongQueue.Dequeue(), true);
            return Task.CompletedTask;
        }

        public Task OnTrackStartAsync(IAudioClient audioClient, AudioTrack track)
        {
            Console.WriteLine("Track start! " + track.Info.Title);
            return Task.CompletedTask;
        }

        public async Task OnTrackEndAsync(IAudioClient audioClient, AudioTrack track)
        {
            Console.WriteLine("Track end! " + track.Info.Title);

            if (SongQueue.Count > 0)
            {
                await NextTrack();
            }
        }
    }
}
