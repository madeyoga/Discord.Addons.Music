using Discord.Addons.Music.Source;
using Discord.Commands;
using Discord.WebSocket;
using ExampleMusicBot.Services.Music;
using Nano.Net.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

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

            SocketVoiceChannel selfVoiceChannel = (Context.Guild.CurrentUser as SocketGuildUser)?.VoiceChannel;
            if (selfVoiceChannel is null)
            {
                await audioService.JoinChannel(voiceChannel, Context.Guild);
            }

            AudioTrack loadedTrack = await audioService.LoadAndPlay(query, Context.Guild);
            if (loadedTrack != null)
            {
                await ReplyAsync($":musical_note: Added to queue {loadedTrack.Info.Title}");
            }
            else
            {
                await ReplyAsync($"Not found anything with {query}");
            }
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
            if (voiceState.Player.Paused)
            {
                voiceState.Player.Paused = false;
                await ReplyAsync("Resume!");
            }
            else
            {
                voiceState.Player.Paused = true;
                await ReplyAsync("Pause!");
            }
        }

        [Command("volume", RunMode = RunMode.Async)]
        public async Task VolumeAsync([Remainder] string volumeNumber)
        {
            GuildVoiceState voiceState = audioService.MusicManager.GetGuildVoiceState(Context.Guild);
            voiceState.Player.Volume = double.Parse(volumeNumber) / 100;
            await ReplyAsync("Volume changed to " + volumeNumber + "!");
        }

        [Command("np", RunMode = RunMode.Async)]
        public async Task AnnounceNowplayAsync()
        {
            GuildVoiceState voiceState = audioService.MusicManager.GetGuildVoiceState(Context.Guild);
            IAudioSource np = voiceState.Player.PlayingTrack;

            if (np == null)
            {
                await ReplyAsync("Not playing anything.");
            }
            else
            {
                await ReplyAsync(":musical_note: Now playing: " + voiceState.Player.PlayingTrack.Info.Title);
            }
        }

        [Command("skip", RunMode = RunMode.Async)]
        public async Task SkipAsync()
        {
            GuildVoiceState voiceState = audioService.MusicManager.GetGuildVoiceState(Context.Guild);
            await voiceState.Scheduler.NextTrack();
            await AnnounceNowplayAsync();
        }
    }
}
