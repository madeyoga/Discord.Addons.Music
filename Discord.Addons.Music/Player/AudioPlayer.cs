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
        private volatile bool paused = false;

        // Events
        public event IAudioEvent.TrackStartAsync OnTrackStartAsync;
        public event IAudioEvent.TrackEndAsync OnTrackEndAsync;
        public event IAudioEvent.TrackErrorAsync OnTrackErrorAsync;

        public IAudioSource PlayingTrack { get; private set; }
        public Stream DiscordStream { get; set; }
        public IAudioClient AudioClient { get; private set; }
        private CancellationTokenSource cts;
        private double volume = 1;

        public AudioPlayer()
        {
            OnTrackStartAsync += TrackStartEventAsync;
            OnTrackEndAsync += TrackEndEventAsync;
            OnTrackErrorAsync += TrackErrorEventAsync;
        }

        public AudioPlayer(IAudioClient audioClient)
        {
            AudioClient = audioClient;
            DiscordStream = audioClient.CreatePCMStream(AudioApplication.Music);
            OnTrackStartAsync += TrackStartEventAsync;
            OnTrackEndAsync += TrackEndEventAsync;
            OnTrackErrorAsync += TrackErrorEventAsync;
        }

        /// <summary>
        /// Audio client is required for Player to create an audio stream.
        /// </summary>
        /// <param name="audioClient"></param>
        public void SetAudioClient(IAudioClient audioClient)
        {
            AudioClient = audioClient;
            DiscordStream?.Dispose();
            DiscordStream = audioClient.CreatePCMStream(AudioApplication.Music);
        }

        private Task TrackStartEventAsync(IAudioClient audioClient, IAudioSource track)
        {
            paused = false;
            PlayingTrack.LoadProcess();
            return Task.CompletedTask;
        }

        private Task TrackEndEventAsync(IAudioClient audioClient, IAudioSource track)
        {
            paused = false;
            ResetStreams();
            PlayingTrack = null;
            cts?.Dispose();
            return Task.CompletedTask;
        }

        private Task TrackErrorEventAsync(IAudioClient audioClient, IAudioSource track, TrackErrorException exception)
        {
            paused = false;
            ResetStreams();
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
                    return;
                }

                if (DiscordStream == null)
                {
                    await OnTrackErrorAsync(AudioClient, track, new TrackErrorException("Error when playing audio track: Discord stream gone."));
                    return;
                }

                if (!paused)
                {
                    // Read audio byte sample
                    read = await PlayingTrack.Provide20msAudio(ct).ConfigureAwait(false);
                    if (read > 0)
                    {
                        if (Volume != 1)
                        {
                            await DiscordStream.WriteAsync(AdjustVolume(PlayingTrack.GetBufferFrame(), Volume), 0, read, ct).ConfigureAwait(false);
                        }
                        else
                        {
                            await DiscordStream.WriteAsync(PlayingTrack.GetBufferFrame(), 0, read, ct).ConfigureAwait(false);
                        }
                    }
                    // Finished playing
                    else
                    {
                        return;
                    }
                }
                else
                {
                    await Task.Delay(4000);
                }
            }
        }

        /// <summary>
        /// Plays an IAudioSource. This method will interrupt and then play the given audio source.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public async Task StartTrackAsync(IAudioSource track)
        {
            if (track == null)
                return;

            if (PlayingTrack != null)
            {
                Stop();
            }

            PlayingTrack = track;

            cts?.Dispose();
            cts = new CancellationTokenSource();

            await OnTrackStartAsync(AudioClient, PlayingTrack).ConfigureAwait(false);
            await AudioLoopAsync(PlayingTrack, cts.Token).ConfigureAwait(false);
            await OnTrackEndAsync(AudioClient, PlayingTrack).ConfigureAwait(false);
        }

        /// <summary>
        /// Stop current playing audio source
        /// </summary>
        public void Stop()
        {
            try
            {
                cts?.Cancel(false);
            }
            catch(ObjectDisposedException) 
            { }
            cts?.Dispose();
        }

        public double Volume
        {
            get
            {
                return volume;
            }
            set
            {
                if (value > 100) value = 100;
                else if (value < 0) value = 0;

                if (value > 1)
                {
                    value /= 100;
                }

                volume = value;
            }
        }

        public bool Paused
        {
            get => paused;
            set => paused = value;
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
            DiscordStream?.Flush();
            PlayingTrack?.Dispose();
        }

        ~AudioPlayer()
        {
            DiscordStream?.Dispose();
            PlayingTrack?.Dispose();
            cts?.Dispose();
        }
    }
}
