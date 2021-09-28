using Discord.Addons.Music.Player;
using Nano.Net.Services.Music;

namespace ExampleMusicBot.Services.Music
{
    public class GuildVoiceState
    {
        public AudioPlayer Player { get; set; }
        public TrackScheduler Scheduler { get; set; }

        public GuildVoiceState()
        {
            Player = new AudioPlayer();
            Scheduler = new TrackScheduler(Player);
        }
    }
}
