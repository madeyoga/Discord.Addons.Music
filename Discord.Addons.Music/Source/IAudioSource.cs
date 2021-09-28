using Discord.Addons.Music.Object;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Source
{
    public interface IAudioSource : IDisposable
    {
        IAudioInfo Info { get; set; }
        Task<int> Provide20msAudio(CancellationToken ct);
        bool IsOpus();
        void LoadProcess();
        string Url { get; set; }
        byte[] GetBufferFrame();
    }
}
