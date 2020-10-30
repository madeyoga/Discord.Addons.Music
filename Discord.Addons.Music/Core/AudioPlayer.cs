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
                DiscordStream.Close();
            }
            DiscordStream = client.CreatePCMStream(AudioApplication.Music);
        }

        public async Task StartTrack()
        {
            if (audioEvent == null)
                audioEvent = new DefaultAudioEventAdapter();

            // Load playing track source stream
            PlayingTrack.SourceStream = LoadTrackStream(PlayingTrack);

            await Task.Run(async () =>
            {
                // Read & Split buffers from Audio Stream.
                while (true)
                {
                    byte[] buffer = new byte[4096];
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
            });

            await Task.Run(async () =>
            {
                bool isFinishedPlaying = false;

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
                            await Task.Delay(1);
                            continue;
                        }

                        // If Queue is empty, then the song is finished playing
                        isFinishedPlaying = true;
                    }

                    // Paused
                    if (reads.Count != 0)
                    {
                        await Task.Delay(1);
                        continue;
                    }

                    // If finished playing or stopped: OnTrackEnd
                    // audioEvent.OnTrackEnd(PlayingTrack);
                    audioEvent.OnTrackEnd(PlayingTrack);

                    isStopped = true;
                    await PlayingTrack.SourceStream.DisposeAsync();

                    // Clear buffers & reads queue
                    buffers.Clear();
                    reads.Clear();
                }
            });
        }

        private Stream LoadTrackStream(AudioTrack track)
        {
            // Stream song
            Process ytdlProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "youtube-dl.exe",
                Arguments = $"--format bestaudio -o Data/Music/{guildId}.mp3 {track.Url}",
                RedirectStandardOutput = true
            });
            ytdlProcess.WaitForExit();

            Process ffmpegProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-loglevel panic -i \"Data/Music/{guildId}.mp3\" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true
            });

            return ffmpegProcess.StandardOutput.BaseStream;
        }

        private async Task DisposeAsync()
        {
            await DiscordStream.FlushAsync();
            await PlayingTrack.SourceStream.DisposeAsync();
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
            isPaused = true;
            isStopped = true;

            DisposeAsync().GetAwaiter().GetResult();
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
            DiscordStream.Dispose();
            DiscordStream.Close();
            File.Delete($"Data/Music/{guildId}.mp3");
        }
    }
}
