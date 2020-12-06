using Discord.Addons.Music.Core;
using Discord.Audio;
using Nano.Net.Services.Music;
using System;
using System.Collections.Generic;
using System.Text;

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

            Player.RegisterEventAdapter(Scheduler);
        }
    }
}
