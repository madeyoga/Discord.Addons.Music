using Discord.Addons.Music.Exception;
using Discord.Addons.Music.Source;
using Discord.Audio;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Player
{
    public class AudioPlayer : IAudioEvent
    {
        // Audio Loop Flags
        private volatile bool Paused = false;

        // Events
        public event IAudioEvent.TrackStartAsync OnTrackStartAsync;
        public event IAudioEvent.TrackEndAsync OnTrackEndAsync;
        public event IAudioEvent.TrackErrorAsync OnTrackErrorAsync;

        public IAudioSource PlayingTrack { get; set; }
        public Stream DiscordStream { get; set; }
        public IAudioClient AudioClient { get; private set; }
        private double Volume { get; set; }
        private CancellationTokenSource cts;

        public AudioPlayer()
        {
            OnTrackStartAsync += TrackStartEventAsync;
            OnTrackEndAsync += TrackEndEventAsync;
            OnTrackErrorAsync += TrackErrorEventAsync;
            Volume = 1;
        }

        public AudioPlayer(IAudioClient audioClient)
        {
            AudioClient = audioClient;
            DiscordStream = audioClient.CreatePCMStream(AudioApplication.Music);
            OnTrackStartAsync += TrackStartEventAsync;
            OnTrackEndAsync += TrackEndEventAsync;
            OnTrackErrorAsync += TrackErrorEventAsync;
            Volume = 1;
        }

        public void SetAudioClient(IAudioClient audioClient)
        {
            AudioClient = audioClient;
            if (DiscordStream != null) DiscordStream.Dispose();
            DiscordStream = audioClient.CreatePCMStream(AudioApplication.Music);
        }

        private Task TrackStartEventAsync(IAudioClient audioClient, IAudioSource track)
        {
            Paused = false;
            ResetStreams();
            PlayingTrack = track;
            PlayingTrack.LoadProcess();
            return Task.CompletedTask;
        }

        private Task TrackEndEventAsync(IAudioClient audioClient, IAudioSource track)
        {
            Paused = false;
            ResetStreams();
            cts.Dispose();
            PlayingTrack = null;
            return Task.CompletedTask;
        }

        private Task TrackErrorEventAsync(IAudioClient audioClient, IAudioSource track, TrackErrorException exception)
        {
            Paused = false;
            ResetStreams();
            cts.Dispose();
            PlayingTrack = null;
            return Task.CompletedTask;
        }

        protected async Task AudioLoopAsync(IAudioSource track, CancellationToken ct)
        {
            int read = -1;
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                if (DiscordStream == null)
                {
                    await OnTrackErrorAsync(AudioClient, track, new TrackErrorException("Error when playing audio track: Discord stream gone."));
                    return;
                }

                if (!Paused)
                {
                    // Read audio byte sample
                    read = await PlayingTrack.Provide20msAudio(ct);
                    if (read > 0)
                    {
                        if (Volume != 1)
                        {
                            await DiscordStream.WriteAsync(AdjustVolume(PlayingTrack.GetBufferFrame(), Volume), 0, read, ct);
                        }
                        else
                        {
                            await DiscordStream.WriteAsync(PlayingTrack.GetBufferFrame(), 0, read, ct);
                        }
                    }
                    // Finished playing
                    else
                    {
                        break;
                    }
                }
                else
                {
                    await Task.Delay(2000, ct);
                }
            }
        }

        public bool StartTrackAsync(IAudioSource track, bool interrupt = true)
        {
            if (track == null) return false;
            if (!interrupt && PlayingTrack != null) return false;

            _ = Task.Run(async () =>
            {
                cts = new CancellationTokenSource();
                await OnTrackStartAsync(AudioClient, track);
                await AudioLoopAsync(track, cts.Token);
                await OnTrackEndAsync(AudioClient, track);
            });

            return true;
        }

        public void Stop()
        {
            try
            {
                cts.Cancel(false);
            }
            catch(ObjectDisposedException) { }
            cts.Dispose();
        }

        public void SetVolume(double volume)
        {
            if (volume > 100) volume = 100;
            else if (volume < 0) volume = 0;
            
            if (volume > 1)
            {
                volume /= 100;
            }

            Volume = volume;
        }

        public void SetPaused(bool paused)
        {
            Paused = paused;
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

        protected void ResetStreams()
        {
            if (DiscordStream != null)
                DiscordStream.Flush();
            if (PlayingTrack != null)
                PlayingTrack.Dispose();
        }

        ~AudioPlayer()
        {
            if (DiscordStream != null)
                DiscordStream.Dispose();
            if (PlayingTrack != null)
                PlayingTrack.Dispose();
        }
    }
}
