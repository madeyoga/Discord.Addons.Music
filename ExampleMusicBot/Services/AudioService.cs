using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Music.Core;
using Discord.Addons.Music.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Nano.Net.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> _audioClients = new ConcurrentDictionary<ulong, IAudioClient>();
        public AudioPlayer Player { get; set; }

        public AudioService()
        {
        }

        public async Task JoinChannel(IVoiceChannel channel, ulong guildId)
        {

            var audioClient = await channel.ConnectAsync();
            _audioClients.TryAdd(guildId, audioClient);

            Player = new AudioPlayer(guildId);
            Player.RegisterEventAdapter(new TrackScheduler());
            Player.SetAudioClient(audioClient);
        }

        public async Task LeaveChannel(SocketCommandContext Context)
        {
            if (_audioClients.TryGetValue(Context.Guild.Id, out IAudioClient audioClient))
            {
                try
                {
                    await audioClient.StopAsync();
                    _audioClients.TryRemove(Context.Guild.Id, out audioClient);
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

        public async Task loadAndPlay(string query)
        {
            List<AudioTrack> tracks;

            // If query is Url
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                Console.WriteLine(query + " is url");
                // await TrackLoader.LoadAudioTrack(query, new DefaultAudioResultHandler());
                tracks = await TrackLoader.LoadAudioTrack(query, fromUrl: true);
            }
            else
            {
                Console.WriteLine(query + " is not url");
                // await TrackLoader.LoadAudioTrack(query, new DefaultAudioResultHandler(), fromUrl: false);
                tracks = await TrackLoader.LoadAudioTrack(query, fromUrl: false);
            }
            
            Console.WriteLine("Loaded " + tracks.Count + " entri(es)");
            
            foreach(AudioTrack track in tracks)
            {
                Console.WriteLine(track.TrackInfo.Title);
            }
            
            await Player.StartTrack(tracks[0]);
        }
    }
}
