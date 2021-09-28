using Discord;
using System.Collections.Concurrent;

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
