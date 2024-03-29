# Unofficial Discord.Net Addons for Audio
[![Nuget](https://img.shields.io/nuget/v/Discord.Addons.Music?color=Green&style=for-the-badge)](https://www.nuget.org/packages/Discord.Addons.Music/)
[![Nuget](https://img.shields.io/nuget/dt/Discord.Addons.Music?color=GREEN&style=for-the-badge)](https://www.nuget.org/packages/Discord.Addons.Music/)
[![contributionswelcome](https://img.shields.io/badge/contributions-welcome-brightgreen/?style=for-the-badge)]((https://github.com/madeyoga/Discord.Addons.Music/issues))
[![discord_invite](https://img.shields.io/discord/458296099049046018?style=for-the-badge)](https://discord.gg/Y8sB4ay)

Audio player library for Discord.Net using FFmpeg and Youtube-dl

**This project is still in development and not production ready**

## Requirements
- [Libopus & Opus.dll](https://dsharpplus.github.io/articles/audio/voicenext/prerequisites.html)
- [Youtube-dl](https://youtube-dl.org/)
- [FFmpeg](https://ffmpeg.org/download.html)

## Installation

### NuGet
- [Discord.Addons.Music](https://www.nuget.org/packages/Discord.Addons.Music/)

## Getting started
This is a basic example on how to use `AudioPlayer`.

```C#
// Initialize AudioPlayer
AudioPlayer audioPlayer = new AudioPlayer();

// Set player's audio client
// This is required for AudioPlayer to create an audio stream to Discord
SocketVoiceChannel voiceChannel = (Context.User as SocketGuildUser)?.VoiceChannel;
var audioClient = await voiceChannel.ConnectAsync();
audioPlayer.SetAudioClient(audioClient);
```

### Playing an audio
To play an audio, we need to load an AudioTrack instance. To do this, we can use `TrackLoader` class:

```C#
string query = "Tuturu Ringtone";
bool wellFormedUri = Uri.IsWellFormedUriString(query, UriKind.Absolute);
List<AudioTrack> tracks = await TrackLoader.LoadAudioTrack(query, fromUrl: wellFormedUri);

// Pick the first entry and use AudioPlayer.StartTrack to play it on Thread Pool
AudioTrack firstTrack = tracks.ElementAt(0);

// Fire & forget
player.StartTrackAsync(firstTrack, interrupt: true).ConfigureAwait(false);

// OR
// await track to finish playing
await player.StartTrackAsync(firstTrack, interrupt: true);
```

### Subscribe to AudioPlayer Events
AudioPlayer implements IAudioEvent and currently there are 3 audio events that can be subscribed: 
- OnTrackStartAsync
- OnTrackEndAsync
- OnTrackErrorAsync

For example, a track scheduler:
```C#
public class TrackScheduler
{
    public Queue<AudioTrack> SongQueue { get; set; }
    private AudioPlayer player;

    public TrackScheduler(AudioPlayer player)
    {
        SongQueue = new Queue<AudioTrack>();
        this.player = player;
        this.player.OnTrackStartAsync += OnTrackStartAsync;
        this.player.OnTrackEndAsync += OnTrackEndAsync;
    }

    public Task Enqueue(AudioTrack track)
    {
        if (player.PlayingTrack != null)
        {
            SongQueue.Enqueue(track);
        }
        else
        {
            // fire and forget
            player.StartTrackAsync(track).ConfigureAwait(false);
        }
        return Task.CompletedTask;
    }

    public async Task NextTrack()
    {
        AudioTrack nextTrack;
        if (SongQueue.TryDequeue(out nextTrack))
            await player.StartTrackAsync(nextTrack);
        else
            player.Stop();
    }

    private Task OnTrackStartAsync(IAudioClient audioClient, IAudioSource track)
    {
        Console.WriteLine("Track start! " + track.Info.Title);
        return Task.CompletedTask;
    }

    private async Task OnTrackEndAsync(IAudioClient audioClient, IAudioSource track)
    {
        Console.WriteLine("Track end! " + track.Info.Title);

        await NextTrack();
    }
}
```

**Note:**
- AudioTrack is PCM AudioSource
- Opus AudioSource is not yet supported

Contributions are very very welcome :]

## Demo project
- Demo for music state management / queue system, at [ExampleMusicBot](https://github.com/madeyoga/Discord.Addons.Music/tree/master/ExampleMusicBot/Services/Music) or at [Demo](https://github.com/madeyoga/Nano.Net)

## Contributing
Looking for a constructive feedback, feedback about best practices would really help me out. If you find a flaw in my logic, please open an issue or a PR and we'll sort things out.

