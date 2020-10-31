using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Audio;
using Nano.Net.Services;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Music.Core;

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

            SocketVoiceChannel clientVoiceChannel = (((SocketUser) Context.Client.CurrentUser) as SocketGuildUser)?.VoiceChannel;
            if (clientVoiceChannel is null)
            {
                await audioService.JoinChannel(voiceChannel, Context.Guild.Id);
            }

            Console.WriteLine(query);
            await audioService.loadAndPlay(query);
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

            await audioService.JoinChannel(voiceChannel, Context.Guild.Id);
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Alias("leave")]
        public async Task StopAsync()
        {
            audioService.Player.Stop();
            await audioService.LeaveChannel(Context);
        }

        [Command("pause", RunMode = RunMode.Async)]
        public async Task PauseAsync()
        {
            audioService.Player.Pause();
            await ReplyAsync("Pause!");
        }

        [Command("resume", RunMode = RunMode.Async)]
        public async Task ResumeAsync()
        {
            audioService.Player.Resume();
            await ReplyAsync("Resume!");
        }

        [Command("volume", RunMode = RunMode.Async)]
        public async Task VolumeAsync([Remainder] string volumeNumber)
        {
            audioService.Player.SetVolume(double.Parse(volumeNumber) / 100);
            await ReplyAsync("Volume changed to " + volumeNumber + "!");
        }

        [Command("np", RunMode = RunMode.Async)]
        public async Task AnnounceNowplayAsync()
        {
            await ReplyAsync("Nowplaying " + audioService.Player.PlayingTrack.TrackInfo.Title);
        }
    }
}
