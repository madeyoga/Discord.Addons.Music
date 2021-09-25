using Discord.Addons.Music.Core;
using Discord.Addons.Music.Exception;
using Discord.Addons.Music.Object;
using Discord.Audio;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Discord.Addons.Music.Audio
{
    public class AudioPlayer : IAudioEvent
    {
        // Audio Loop Flags
        private volatile bool Paused = false;

        // Events
        public event IAudioEvent.TrackStartAsync OnTrackStartAsync;
        public event IAudioEvent.TrackEndAsync OnTrackEndAsync;
        public event IAudioEvent.TrackErrorAsync OnTrackErrorAsync;

        // Abstract audio track later
        public AudioTrack PlayingTrack { get; set; }
        public Stream DiscordStream { get; set; }
        public IAudioClient AudioClient { get; set; }
        private float Volume { get; set; }
        private CancellationTokenSource cts;

        public AudioPlayer(IAudioClient audioClient)
        {
            AudioClient = audioClient;

            OnTrackStartAsync += TrackStartEventAsync;
            OnTrackEndAsync += TrackEndEventAsync;
            OnTrackErrorAsync += TrackErrorEventAsync;
        }

        private Task TrackStartEventAsync(IAudioClient audioClient, AudioTrack track)
        {
            Paused = false;
            PlayingTrack = track;
            PlayingTrack.LoadProcess();
            return Task.CompletedTask;
        }

        private async Task TrackEndEventAsync(IAudioClient audioClient, AudioTrack track)
        {
            await ResetStreams();
            PlayingTrack = null;
            cts.Dispose();
        }

        private async Task TrackErrorEventAsync(IAudioClient audioClient, AudioTrack track, TrackErrorException exception)
        {
            Paused = false;
            await ResetStreams();
            PlayingTrack = null;
            cts.Dispose();
        }

        protected async Task AudioLoopAsync(AudioTrack track, CancellationToken cts)
        {
            byte[] buffer = new byte[1024];
            int read = -1;

            while (true)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                if (DiscordStream == null)
                {
                    await OnTrackErrorAsync(AudioClient, track, new TrackErrorException("Error when playing audio track: NULL Discord Stream."));
                    return;
                }

                if (!Paused)
                {
                    // Read audio byte sample
                    read = await PlayingTrack.SourceStream.ReadAsync(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
                        if (Volume != 1)
                        {
                            await DiscordStream.WriteAsync(AdjustVolume(buffer, Volume), 0, read);
                        }
                        else
                        {
                            await DiscordStream.WriteAsync(buffer, 0, read);
                        }

                        await Task.Delay(20);
                    }
                    // Finished playing
                    else
                    {
                        break;
                    }
                }
                else
                {
                    await Task.Delay(2000);
                }
            }
        }

        public async Task<bool> StartTrackAsync(AudioTrack track, bool interrupt = true)
        {
            if (interrupt)
            {
                await ResetStreams();
            }
            else
            {
                if (PlayingTrack != null) return false;
            }

            cts = new CancellationTokenSource();
            await Task.Run(async () => 
            {
                await OnTrackStartAsync(AudioClient, track);
                await AudioLoopAsync(track, cts.Token);
                await OnTrackEndAsync(AudioClient, track);
            });

            //await Task.Factory.StartNew(async (action) =>
            //{
            //    await OnTrackStartAsync(AudioClient, track);
            //    await AudioLoopAsync(track);
            //    await OnTrackEndAsync(AudioClient, track);
            //}, CancellationToken.None, TaskCreationOptions.LongRunning);

            return true;
        }

        public void Stop()
        {
            cts.Cancel(false);
        }

        public void SetVolume(float volume)
        {
            if (volume > 100) volume = 100;
            else if (volume < 0) volume = 0;
            
            if (volume > 1)
            {
                volume /= 100;
            }

            Volume = volume;
        }

        protected static unsafe byte[] AdjustVolume(byte[] audioSamples, double volume)
        {
            if (Math.Abs(volume - 1f) < 0.0001f)
                return audioSamples;

            // 16-bit precision for the multiplication
            var volumeFixed = (int)Math.Round(volume * 65536d);

            var count = audioSamples.Length / 2;

            fixed (byte* srcBytes = audioSamples)
            {
                var src = (short*)srcBytes;

                for (var i = count; i != 0; i--, src++)
                    *src = (short)(((*src) * volumeFixed) >> 16);
            }

            return audioSamples;
        }

        protected async Task ResetStreams()
        {
            await DiscordStream.FlushAsync();
            if (PlayingTrack != null)
            {
                PlayingTrack.SourceStream.Close();
                PlayingTrack.FFmpegProcess.Close();
            }
        }

        ~AudioPlayer()
        {
            DiscordStream.Close();   
            if (PlayingTrack != null)
            {
                PlayingTrack.SourceStream.Close();
                PlayingTrack.FFmpegProcess.Close();
            }
        }
    }
}
