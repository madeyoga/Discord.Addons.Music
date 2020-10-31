using Discord.Audio;
using Discord.Addons.Music.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Core
{
    public class AudioPlayer
    {
        public AudioTrack PlayingTrack { get; set; }
        public AudioOutStream DiscordStream { get; set; }

        private ulong guildId { get; set; }
        private AudioEventAdapter audioEvent;
        private IAudioClient audioClient;

        // Flags
        private volatile bool isPaused = false;
        private volatile bool isStopped = false;
        private volatile bool isVolumeAdjusted = false;

        // Audio attributes
        private double Volume;
        private volatile Queue<int> reads = new Queue<int>();
        private volatile Queue<byte[]> buffers = new Queue<byte[]>();

        public AudioPlayer(ulong guildId)
        {
            this.guildId = guildId;
        }

        public AudioPlayer(ulong guildId, AudioEventAdapter audioEvent)
        {
            this.audioEvent = audioEvent;
            this.guildId = guildId;
        }

        public void RegisterEventAdapter(AudioEventAdapter audioEvent)
        {
            this.audioEvent = audioEvent;
        }

        public void SetAudioClient(IAudioClient client)
        {
            audioClient = client;
            if (DiscordStream != null)
            {
                DiscordStream.Flush();
                DiscordStream.Dispose();
                DiscordStream.Close();
            }
            DiscordStream = client.CreatePCMStream(AudioApplication.Music);
        }

        public async Task StartTrack()
        {
            if (audioEvent == null)
                audioEvent = new DefaultAudioEventAdapter();

            // handle if still playing
            //if (PlayingTrack != null)
            //{
            //    return;
            //}

            // Load playing track source stream
            //PlayingTrack.SourceStream = LoadTrackStream(PlayingTrack);
            PlayingTrack.SourceStream = PlayingTrack.FFmpegProcess.StandardOutput.BaseStream;
            
            await Task.Factory.StartNew(async () =>
            {
                // Read & Split buffers from Audio Stream.
                // Volatile queue does not work
                while (true)
                {
                    byte[] buffer = new byte[16096];
                    int read;
                    if ((read = await PlayingTrack.SourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        reads.Enqueue(read);
                        buffers.Enqueue(buffer);
                    }
                    else
                    {
                        break;
                    }
                }

                // No buffers
                if (buffers.Count <= 0)
                {
                    Console.WriteLine("Cannot play track: Empty stream.");
                    return;
                }

                bool isFinishedPlaying = false;
                isStopped = false;

                // OnTrackStart Playing here
                await audioEvent.OnTrackStartAsync(PlayingTrack);

                // Playing loop
                while (!isStopped)
                {
                    // Event Loop
                    while (!isPaused && !isStopped && !isFinishedPlaying)
                    {
                        // Write queued Buffers to DiscordStream.
                        if (isVolumeAdjusted)
                            await DiscordStream.WriteAsync(AdjustVolume(buffers.Dequeue(), Volume), 0, reads.Dequeue());
                        else
                            await DiscordStream.WriteAsync(buffers.Dequeue(), 0, reads.Dequeue());

                        // Write every second.
                        if (reads.Count != 0)
                        {
                            await Task.Delay(10);
                            continue;
                        }

                        // If Queue is empty, then the song is finished playing
                        isFinishedPlaying = true;
                    }

                    // Paused
                    if (reads.Count != 0)
                    {
                        await Task.Delay(2000);
                        continue;
                    }

                    // If finished playing or stopped: OnTrackEnd
                    audioEvent.OnTrackEnd(PlayingTrack);

                    isStopped = true;

                    // Clear buffers & reads queue
                    buffers.Clear();
                    reads.Clear();
                }

                await DisposeAsync();
            });
        }

        private Stream LoadTrackStream(AudioTrack track)
        {
            // Stream song
            // In 1 Process WORKED!!!!
            Process ffmpegProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --format bestaudio -o - {track.Url} | ffmpeg.exe -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            return ffmpegProcess.StandardOutput.BaseStream;
        }

        private async Task DisposeAsync()
        {
            await DiscordStream.FlushAsync();
            await PlayingTrack.SourceStream.FlushAsync();

            PlayingTrack.SourceStream.Dispose();
            PlayingTrack.SourceStream.Close();
            PlayingTrack.FFmpegProcess.Dispose();
            PlayingTrack.FFmpegProcess.Close();
            PlayingTrack = null;
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Resume()
        {
            isPaused = false;
        }

        public void Stop()
        {
            isStopped = true;
        }

        public void SetVolume(double volume)
        {
            isVolumeAdjusted = true;
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
        }
    }
}
