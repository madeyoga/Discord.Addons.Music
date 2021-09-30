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
        private Task loopTask;

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
            if (DiscordStream != null) DiscordStream.Dispose();
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
                        return;
                    }
                }
                else
                {
                    await Task.Delay(2000);
                }
            }
        }

        /// <summary>
        /// Start playing an audio source on the thread pool.
        /// </summary>
        /// <param name="track">Audio source to be played</param>
        /// <param name="interrupt">interrupt true will force audio player to skip and play the provided audio source.</param>
        /// <returns>true if the provided audio source was played, false if don't interrupt (interrupt is false) and audio player is still playing audio</returns>
        public bool StartTrack(IAudioSource track, bool interrupt = true)
        {
            if (track == null)
                return false;

            if (PlayingTrack != null)
            {
                if (interrupt)
                {
                    Stop();
                    loopTask.Wait();
                }
                else
                {
                    return false;
                }
            }

            PlayingTrack = track;

            loopTask = Task.Run(async () =>
            {
                if (cts != null)
                {
                    cts.Dispose();
                }
                cts = new CancellationTokenSource();
                await OnTrackStartAsync(AudioClient, track);
                await AudioLoopAsync(track, cts.Token);
                await OnTrackEndAsync(AudioClient, track);
            });

            return true;
        }

        /// <summary>
        /// Start playing an audio source on the thread pool. Deprecated in version 0.1.1, please use StartTrack instead.
        /// </summary>
        /// <param name="track"></param>
        /// <param name="interrupt"></param>
        /// <returns></returns>
        [Obsolete("StartTrackAsync is deprecated since it is actually not an async method, please use StartTrack instead.")]
        public bool StartTrackAsync(IAudioSource track, bool interrupt = true)
        {
            if (track == null)
                return false;

            if (!interrupt && PlayingTrack != null)
                return false;

            Task.Run(async () =>
            {
                cts = new CancellationTokenSource();
                await OnTrackStartAsync(AudioClient, track);
                await AudioLoopAsync(track, cts.Token);
                await OnTrackEndAsync(AudioClient, track);
            });

            return true;
        }

        /// <summary>
        /// Stop current playing audio source
        /// </summary>
        public void Stop()
        {
            try
            {
                cts.Cancel(false);
            }
            catch(ObjectDisposedException) 
            { }
            cts.Dispose();
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
