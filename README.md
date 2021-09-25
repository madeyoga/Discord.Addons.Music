# Unofficial Discord.Net Addons for Audio
[![Nuget](https://img.shields.io/nuget/v/Discord.Addons.Music?color=Green&style=for-the-badge)](https://www.nuget.org/packages/Discord.Addons.Music/)
[![Nuget](https://img.shields.io/nuget/dt/Discord.Addons.Music?color=GREEN&style=for-the-badge)](https://www.nuget.org/packages/Discord.Addons.Music/)
[![contributionswelcome](https://img.shields.io/badge/contributions-welcome-brightgreen/?style=for-the-badge)]((https://github.com/madeyoga/Discord.Addons.Music/issues))
[![discord_invite](https://img.shields.io/discord/458296099049046018?style=for-the-badge)](https://discord.gg/Y8sB4ay)

Audio player library for Discord.Net

**This project is still in development and not yet ready for production use**

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

### Set AudioPlayer's IAudioClient
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

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.
