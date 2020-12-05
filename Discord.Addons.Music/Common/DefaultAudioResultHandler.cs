using Discord.Addons.Music.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Addons.Music.Common
{
    public class DefaultAudioResultHandler : AudioLoadResultHandler
    {
        public override void OnLoadPlaylist(List<AudioTrack> tracks)
        {
            throw new NotImplementedException();
        }

        public override void OnLoadTrack(AudioTrack track)
        {
            throw new NotImplementedException();
        }
    }
}
