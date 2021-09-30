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

        public void Enqueue(AudioTrack track)
        {
            if (player.StartTrack(track, false) == false)
            {
                SongQueue.Enqueue(track);
            }
        }

        public void NextTrack()
        {
            AudioTrack nextTrack;
            SongQueue.TryDequeue(out nextTrack);
            player.StartTrack(nextTrack, true);
        }

        private Task OnTrackStartAsync(IAudioClient audioClient, IAudioSource track)
        {
            Console.WriteLine("Track start! " + track.Info.Title);
            return Task.CompletedTask;
        }

        private Task OnTrackEndAsync(IAudioClient audioClient, IAudioSource track)
        {
            Console.WriteLine("Track end! " + track.Info.Title);
            
            NextTrack();

            return Task.CompletedTask;
        }
    }
}
