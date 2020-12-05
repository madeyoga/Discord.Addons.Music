using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Addons.Music.Core
{
    public abstract class AudioLoadResultHandler
    {
        public abstract void OnLoadTrack(AudioTrack track);
        public abstract void OnLoadPlaylist(List<AudioTrack> tracks);
    }
}
