using Discord;
using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ExampleMusicBot.Services.Music
{
    public class GuildVoiceStateManager
    {
        public readonly ConcurrentDictionary<ulong, GuildVoiceState> VoiceStates = new ConcurrentDictionary<ulong, GuildVoiceState>();

        public GuildVoiceState GetGuildVoiceState(IGuild guild)
        {
            GuildVoiceState voiceState;

            if (!VoiceStates.ContainsKey(guild.Id))
            {
                VoiceStates.TryAdd(guild.Id, new GuildVoiceState());
            }
            voiceState = VoiceStates[guild.Id];

            return voiceState;
        }
    }
}
