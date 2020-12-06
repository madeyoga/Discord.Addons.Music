using Discord.Addons.Music.Core;
using Discord.Addons.Music.Exception;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nano.Net.Services.Music
{
    public class TrackScheduler : IAudioEventAdapter
    {
        Queue<AudioTrack> SongQueue { get; set; }

        private AudioPlayer player;

        public TrackScheduler(AudioPlayer player)
        {
            SongQueue = new Queue<AudioTrack>();
            this.player = player;
        }

        public void Enqueue(AudioTrack track)
        {
            // If player is still playing, queue the track
            if (player.IsPlaying())
            {
                SongQueue.Enqueue(track);
            }
            else
            {
                player.PlayTrack(track);
            }
        }

        public async Task EnqueueAsync(AudioTrack track)
        {
            // If player is still playing, queue the track
            if (player.IsPlaying())
            {
                SongQueue.Enqueue(track);
            }
            else
            {
                await player.PlayTrackAsync(track);
            }
        }

        public void OnTrackEnd(AudioTrack track)
        {
            Console.WriteLine("Track end " + track.TrackInfo.Title);
            if (SongQueue.Count > 0)
            {
                AudioTrack nextTrack = SongQueue.Dequeue();
                Console.WriteLine("Next track " + nextTrack.TrackInfo.Title);
                player.PlayTrack(nextTrack);
            }
        }

        public void OnTrackError(AudioTrack track, TrackErrorException exception)
        {
            throw new NotImplementedException();
        }

        public void OnTrackStart(AudioTrack track)
        {
            Console.WriteLine("Start playing track " + track.TrackInfo.Title);
        }

        public async Task<Task> OnTrackStartAsync(AudioTrack track)
        {
            Console.WriteLine("Track start! " + track.TrackInfo.Title);
            
            return Task.CompletedTask;
        }

        public void OnTrackStuck(AudioTrack track, TrackStuckException exception)
        {
            throw new NotImplementedException();
        }

        public async Task<Task> OnTrackEndAsync(AudioTrack track)
        {
            Console.WriteLine("Track end! " + track.TrackInfo.Title);

            if (SongQueue.Count > 0)
            {
                AudioTrack nextTrack = SongQueue.Dequeue();
                Console.WriteLine("Next track " + nextTrack.TrackInfo.Title);
                await player.PlayTrackAsync(nextTrack);
            }

            return Task.CompletedTask;
        }
    }
}
