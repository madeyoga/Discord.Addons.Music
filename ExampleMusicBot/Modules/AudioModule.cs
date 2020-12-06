using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Audio;
using Nano.Net.Services;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Music.Core;
using ExampleMusicBot.Services.Music;

namespace Nano.Net.Modules
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        public YoutubeService YoutubeService { get; set; }

        private AudioService audioService;

        public AudioModule(AudioService audioService)
        {
            this.audioService = audioService;
        }

        [Command("search")]
        [Alias("s")]
        public async Task SearchYoutubeVideoAsync([Remainder] string query)
        {
            dynamic data = await YoutubeService.SearchVideosByQuery(query);

            List<string> videoTitles = new List<string>();

            int counter = 1;
            foreach (var video in data["items"])
            {
                string videoTitle = $"[**{counter}**]. **{video["snippet"]["title"]}**";
                videoTitles.Add(videoTitle);
                counter += 1;
            }

            string reply = string.Join("\n", videoTitles);

            await Context.Channel.SendMessageAsync(reply);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayAsync([Remainder] string query)
        {
            // Ensure voice
            SocketVoiceChannel voiceChannel = (Context.User as SocketGuildUser)?.VoiceChannel;

            if (voiceChannel is null)
            {
                await ReplyAsync("You r not in a voice channel");
                return;
            }

            Console.WriteLine(query);
            await audioService.loadAndPlay(query, Context.Guild);
        }

        [Command("join", RunMode = RunMode.Async)]
        [Alias("summon")]
        public async Task JoinAsync()
        {
            SocketVoiceChannel voiceChannel = (Context.User as SocketGuildUser)?.VoiceChannel;

            if (voiceChannel is null)
            {
                await ReplyAsync("You r not in a voice channel");
                return;
            }

            await audioService.JoinChannel(voiceChannel, Context.Guild);
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Alias("leave")]
        public async Task StopAsync()
        {
            GuildVoiceState voiceState = audioService.MusicManager.GetGuildVoiceState(Context.Guild);
            voiceState.Player.Stop();
            await audioService.LeaveChannel(Context);
        }

        [Command("pause", RunMode = RunMode.Async)]
        public async Task PauseAsync()
        {
            GuildVoiceState voiceState = audioService.MusicManager.GetGuildVoiceState(Context.Guild);
            voiceState.Player.SetPaused(true);
            await ReplyAsync("Pause!");
        }

        [Command("resume", RunMode = RunMode.Async)]
        public async Task ResumeAsync()
        {
            GuildVoiceState voiceState = audioService.MusicManager.GetGuildVoiceState(Context.Guild);
            voiceState.Player.SetPaused(false);
            await ReplyAsync("Resume!");
        }

        [Command("volume", RunMode = RunMode.Async)]
        public async Task VolumeAsync([Remainder] string volumeNumber)
        {
            GuildVoiceState voiceState = audioService.MusicManager.GetGuildVoiceState(Context.Guild);
            voiceState.Player.SetVolume(double.Parse(volumeNumber) / 100);
            await ReplyAsync("Volume changed to " + volumeNumber + "!");
        }

        [Command("np", RunMode = RunMode.Async)]
        public async Task AnnounceNowplayAsync()
        {
            GuildVoiceState voiceState = audioService.MusicManager.GetGuildVoiceState(Context.Guild);
            await ReplyAsync("Nowplaying " + voiceState.Player.PlayingTrack.TrackInfo.Title);
        }
    }
}
