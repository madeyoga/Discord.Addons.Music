using Discord;
using Discord.Addons.Music.Common;
using Discord.Addons.Music.Core;
using Discord.Commands;
using ExampleMusicBot.Services.Music;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nano.Net.Services
{
    public class AudioService
    {
        public GuildVoiceStateManager MusicManager { get; set; }

        public AudioService()
        {
            MusicManager = new GuildVoiceStateManager();
        }

        public async Task JoinChannel(IVoiceChannel channel, IGuild guild)
        {
            var audioClient = await channel.ConnectAsync();

            GuildVoiceState voiceState = MusicManager.GetGuildVoiceState(guild);
            voiceState.Player.SetAudioClient(audioClient);
        }

        public async Task LeaveChannel(SocketCommandContext Context)
        {
            if (MusicManager.VoiceStates.TryGetValue(Context.Guild.Id, out GuildVoiceState voiceState))
            {
                try
                {
                    await voiceState.Player.AudioClient.StopAsync();
                    MusicManager.VoiceStates.TryRemove(Context.Guild.Id, out voiceState);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + ", cannot disconnect");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("I'm not connected");
            }
        }

        public async Task loadAndPlay(string query, IGuild guild)
        {
            List<AudioTrack> tracks;

            // If query is Url
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                Console.WriteLine(query + " is url");
                tracks = await TrackLoader.LoadAudioTrack(query, fromUrl: true);
            }
            else
            {
                Console.WriteLine(query + " is not url");
                tracks = await TrackLoader.LoadAudioTrack(query, fromUrl: false);
            }
            
            if (tracks.Count == 0)
            {
                return;
            }

            Console.WriteLine("Loaded " + tracks.Count + " entri(es)");

            GuildVoiceState voiceState = MusicManager.GetGuildVoiceState(guild);

            foreach(AudioTrack track in tracks)
            {
                await voiceState.Scheduler.EnqueueAsync(track);
                Console.WriteLine("Enqueued " + track.Info.Title);
            }
        }
    }
}
