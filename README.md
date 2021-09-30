# Unofficial Discord.Net Addons for Audio
[![Nuget](https://img.shields.io/nuget/v/Discord.Addons.Music?color=Green&style=for-the-badge)](https://www.nuget.org/packages/Discord.Addons.Music/)
[![Nuget](https://img.shields.io/nuget/dt/Discord.Addons.Music?color=GREEN&style=for-the-badge)](https://www.nuget.org/packages/Discord.Addons.Music/)
[![contributionswelcome](https://img.shields.io/badge/contributions-welcome-brightgreen/?style=for-the-badge)]((https://github.com/madeyoga/Discord.Addons.Music/issues))
[![discord_invite](https://img.shields.io/discord/458296099049046018?style=for-the-badge)](https://discord.gg/Y8sB4ay)

Audio player library for Discord.Net using FFmpeg and Youtube-dl

## Requirements
- [Libopus & Opus.dll](https://opus-codec.org/downloads/)
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

// Pick the first entry and use AudioPlayer.StartTrackAsync to play it
AudioTrack firstTrack = tracks.ElementAt(0);
player.StartTrackAsync(SongQueue.Dequeue(), interrupt: true);
```

### Handling Audio Event
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

    public void EnqueueAsync(AudioTrack track)
    {
        if (player.StartTrackAsync(track, interrupt: false) == false)
        {
            SongQueue.Enqueue(track);
        }
    }

    public void NextTrack()
    {
        player.StartTrackAsync(SongQueue.Dequeue(), interrupt: true);
    }

    private Task OnTrackStartAsync(IAudioClient audioClient, IAudioSource track)
    {
        Console.WriteLine("Track start! " + track.Info.Title);
        return Task.CompletedTask;
    }

    private Task OnTrackEndAsync(IAudioClient audioClient, IAudioSource track)
    {
        Console.WriteLine("Track end! " + track.Info.Title);

        if (SongQueue.Count > 0)
        {
            NextTrack();
        }
        return Task.CompletedTask;
    }
}
```

**Note:**
- AudioTrack is actually a PCM AudioSource
- Opus AudioSource is not yet supported

Contributions are very very welcome :]

## Example Music Bot
- Example for guild state management and queue system, at [ExampleMusicBot project](https://github.com/madeyoga/Discord.Addons.Music/tree/master/ExampleMusicBot/Services/Music)

## Contributing
Looking for a constructive feedback, feedback about best practices would really help me out.

Pull requests are very welcome,, Thanks!
