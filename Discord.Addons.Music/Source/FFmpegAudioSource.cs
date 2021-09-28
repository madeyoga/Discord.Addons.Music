using Discord.Addons.Music.Object;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Source
{
    public abstract class FFmpegAudioSource : IAudioSource
    {
        private bool disposedValue;

        public Stream SourceStream { get; set; }
        public Process FFmpegProcess { get; set; }
        public IAudioInfo Info { get; set; }
        public string Url { get; set; }
        public abstract byte[] GetBufferFrame();
        public abstract bool IsOpus();
        public abstract void LoadProcess();
        public abstract Task<int> Provide20msAudio(CancellationToken ct);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                SourceStream.Dispose();
                FFmpegProcess.Dispose();
                SourceStream = null;
                FFmpegProcess = null;
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~FFmpegAudioSource()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
