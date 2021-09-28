using Discord.Addons.Music.Player;
using Discord.Addons.Music.Source;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nano.Net.Services.Music
{
    public class TrackScheduler
    {
        public Queue<AudioTrack> SongQueue { get; set; }
        private AudioPlayer player;

        public TrackScheduler(AudioPlayer player)
        {
            SongQueue = new Queue<AudioTrack>();
            this.player = player;
            this.player.OnTrackStartAsync += OnTrackStartAsync;
            this.player.OnTrackEndAsync += OnTrackEndAsync;
        }

        public void EnqueueAsync(AudioTrack track)
        {
            if (player.StartTrackAsync(track, false) == false)
            {
                SongQueue.Enqueue(track);
            }
        }

        public void NextTrack()
        {
            player.StartTrackAsync(SongQueue.Dequeue(), true);
        }

        public Task OnTrackStartAsync(IAudioClient audioClient, IAudioSource track)
        {
            Console.WriteLine("Track start! " + track.Info.Title);
            return Task.CompletedTask;
        }

        public Task OnTrackEndAsync(IAudioClient audioClient, IAudioSource track)
        {
            Console.WriteLine("Track end! " + track.Info.Title);

            if (SongQueue.Count > 0)
            {
                NextTrack();
            }
            return Task.CompletedTask;
        }
    }
}
