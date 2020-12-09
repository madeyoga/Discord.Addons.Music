using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Common
{
    public class DefaultAudioPlayerManager
    {
        public Task OnMemberVoiceUpdate(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            return Task.CompletedTask;
        }
    }
}
