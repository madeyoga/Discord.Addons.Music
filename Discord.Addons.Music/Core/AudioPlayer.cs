using Discord.Audio;
using Discord.Addons.Music.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord.Addons.Music.Exception;
using System.Threading;

namespace Discord.Addons.Music.Core
{
    public class AudioPlayer
    {
        public AudioTrack PlayingTrack { get; set; }
        public AudioOutStream DiscordStream { get; set; }
        public IAudioClient AudioClient { get; set; }

        private ulong guildId { get; set; }
        private IAudioEventAdapter audioEvent;
        private Thread audioStreamThread;

        // Flags
        private volatile bool isPaused = false;
        private volatile bool isStopped = true;
        private volatile bool isFinishedPlaying = true;
        private volatile bool isVolumeAdjusted = false;

        // Audio attributes
        private double Volume;

        public AudioPlayer()
        {

        }

        public void RegisterEventAdapter(IAudioEventAdapter audioEvent)
        {
            this.audioEvent = audioEvent;
        }

        public void SetAudioClient(IAudioClient client)
        {
            AudioClient = client;
            if (DiscordStream != null)
            {
                DiscordStream.Flush();
                DiscordStream.Dispose();
                DiscordStream.Close();
            }
            DiscordStream = client.CreatePCMStream(AudioApplication.Music);
        }

        public async Task PlayTrackAsync(AudioTrack track)
        {
            if (audioEvent == null)
                audioEvent = new DefaultAudioEventAdapter();

            // handle if still playing, FAIL
            if (IsPlaying())
            {
                Stop();
                Dispose();
            }
            PlayingTrack = track;

            // Load playing track source stream
            PlayingTrack.SourceStream = PlayingTrack.FFmpegProcess.StandardOutput.BaseStream;

            await Task.Factory.StartNew(async () =>
            {
                isFinishedPlaying = false;
                isStopped = false;
                isPaused = false;

                // OnTrackStart Playing here
                await audioEvent.OnTrackStartAsync(PlayingTrack);

                byte[] buffer = new byte[164384];
                int read;
                // Playing loop
                while (!isStopped)
                {
                    // Event Loop
                    while (!isPaused && !isStopped && !isFinishedPlaying)
                    {
                        read = await PlayingTrack.SourceStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read > 0)
                        {
                            if (isVolumeAdjusted)
                                await DiscordStream.WriteAsync(AdjustVolume(buffer, Volume), 0, read);
                            else
                                await DiscordStream.WriteAsync(buffer, 0, read);

                            await Task.Delay(10);
                        }
                        else
                        {
                            isFinishedPlaying = true;
                            isStopped = true;
                        }
                    }

                    // Paused
                    if (isPaused && !isStopped && !isFinishedPlaying)
                    {
                        await Task.Delay(2000);
                        continue;
                    }
                }

                await DiscordStream.FlushAsync();
                // If finished playing or stopped: OnTrackEnd
                audioEvent.OnTrackEnd(PlayingTrack);
                //Dispose();
            });
        }

        public void PlayTrack(AudioTrack track)
        {
            if (audioEvent == null)
                audioEvent = new DefaultAudioEventAdapter();

            // handle if still playing, FAIL
            if (IsPlaying())
            {
                Stop();
                Dispose();
            }
            PlayingTrack = track;

            audioStreamThread = new Thread(StartAudioStream);
            audioStreamThread.Start();
        }

        private void StartAudioStream()
        {
            isFinishedPlaying = false;
            isStopped = false;
            isPaused = false;

            audioEvent.OnTrackStart(PlayingTrack);

            // Playing loop
            while (!isStopped)
            {
                // Event Loop
                while (!isPaused && !isStopped && !isFinishedPlaying)
                {
                    byte[] buffer = new byte[164384];
                    int read;
                    read = PlayingTrack.SourceStream.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
                        // Write queued Buffers to DiscordStream.
                        if (isVolumeAdjusted)
                            DiscordStream.Write(AdjustVolume(buffer, Volume), 0, read);
                        else
                            DiscordStream.Write(buffer, 0, read);

                        //Thread.Sleep(10);
                    }
                    else
                    {
                        isFinishedPlaying = true;
                        isStopped = true;
                    }
                }

                // Paused
                if (isPaused && !isStopped && !isFinishedPlaying)
                {
                    Task.Delay(2000);
                    continue;
                }
            }

            // If finished playing or stopped: OnTrackEnd
            DiscordStream.Flush();

            audioEvent.OnTrackEnd(PlayingTrack);
        }

        private void Dispose()
        {
            DiscordStream.Flush();
            if (PlayingTrack != null)
            {
                PlayingTrack.SourceStream.Flush();

                PlayingTrack.SourceStream.Dispose();
                PlayingTrack.SourceStream.Close();
                PlayingTrack.FFmpegProcess.Dispose();
                PlayingTrack.FFmpegProcess.Close();
            }
            //PlayingTrack = null;
        }

        public void SetPaused(bool paused)
        {
            isPaused = paused;
        }

        public void Stop()
        {
            isStopped = true;
        }

        public void SetVolume(double volume)
        {
            if (volume == 1.0)
            {
                isVolumeAdjusted = false;
            }
            else
            {
                isVolumeAdjusted = true;
            }
            Volume = volume;
        }

        public double GetVolume()
        {
            return Volume;
        }

        public long GetDuration()
        {
            return PlayingTrack.SourceStream.Length;
        }

        public long GetPosition()
        {
            return PlayingTrack.SourceStream.Position;
        }

        public bool IsPlaying()
        {
            return !isStopped && !isFinishedPlaying;
        }

        private static unsafe byte[] AdjustVolume(byte[] audioSamples, double volume)
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

        ~AudioPlayer()
        {
            DiscordStream.Flush();
            DiscordStream.Dispose();
            DiscordStream.Close();

            if (PlayingTrack != null)
            {
                PlayingTrack.SourceStream.Dispose();
                PlayingTrack.SourceStream.Close();
                PlayingTrack.FFmpegProcess.Dispose();
                PlayingTrack.FFmpegProcess.Close();
            }
        }
    }
}
