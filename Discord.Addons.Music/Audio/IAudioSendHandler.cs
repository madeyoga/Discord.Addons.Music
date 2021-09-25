using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Addons.Music.Audio
{
    public interface IAudioSendHandler
    {
        int[] Provide20msAudio();
    }
}
