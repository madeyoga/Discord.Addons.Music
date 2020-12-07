# Unofficial Discord.Net Addons for Music
Unofficial Discord.Net extension for Music using FFmpeg

## Requirements
- [Libopus & Opus.dll](https://opus-codec.org/downloads/)
- [Youtube-dl](https://youtube-dl.org/)
- [FFmpeg](https://ffmpeg.org/download.html)

## Installation

### NuGet
- [Discord.Addons.Music](https://www.nuget.org/packages/Discord.Addons.Music/)

## Basic Usage
This is a basic example for `AudioPlayer` and `TrackLoader` to play a single `AudioTrack`.

### Initialize AudioPlayer
```C#
using Discord.Addons.Music.Core;

Player = new AudioPlayer();
```

### Set Guild's IAudioClient
```C#
Player.SetAudioClient(audioClient)
```

### Load and Play AudioTrack
```C#
public async Task loadAndPlay(string query)
{
    List<AudioTrack> tracks;

    // Check if query is an Url or Keywords
    if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
    {
        tracks = await TrackLoader.LoadAudioTrack(query, fromUrl: true);
    }
    else
    {
        tracks = await TrackLoader.LoadAudioTrack(query, fromUrl: false);
    }
    
    if (tracks.Count == 0)
    {
        Console.WriteLine("Empty result");
        return;
    }
    
    // Play first entry
    Player.PlayTrack(tracks[0]);
}
```

### More Examples
- Examples for guild management and queue system, at [ExampleMusicBot project](https://github.com/madeyoga/Discord.Addons.Music/tree/master/ExampleMusicBot/Services/Music)
