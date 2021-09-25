using Discord.Addons.Music.Audio;
using Discord.Audio;
using Nano.Net.Services.Music;

namespace ExampleMusicBot.Services.Music
{
    public class GuildVoiceState
    {
        public AudioPlayer Player { get; set; }
        public TrackScheduler Scheduler { get; set; }

        public GuildVoiceState(IAudioClient client)
        {
            Player = new AudioPlayer(client);
            Scheduler = new TrackScheduler(Player);
        }
    }
}
