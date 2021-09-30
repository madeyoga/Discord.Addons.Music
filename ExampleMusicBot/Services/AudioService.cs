using Discord;
using Discord.Addons.Music.Common;
using Discord.Addons.Music.Source;
using Discord.Commands;
using ExampleMusicBot.Services.Music;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<AudioTrack> loadAndPlay(string query, IGuild guild)
        {
            List<AudioTrack> tracks;

            // If query is Url
            bool wellFormedUri = Uri.IsWellFormedUriString(query, UriKind.Absolute);
            tracks = await TrackLoader.LoadAudioTrack(query, fromUrl: wellFormedUri);
            
            if (tracks.Count == 0)
            {
                return null;
            }

            Console.WriteLine("Loaded " + tracks.Count + " entri(es)");

            GuildVoiceState voiceState = MusicManager.GetGuildVoiceState(guild);

            foreach(AudioTrack track in tracks)
            {
                Console.WriteLine("Enqueue " + track.Info.Title);
                voiceState.Scheduler.Enqueue(track);
                return track;
            }

            return null;
        }
    }
}
