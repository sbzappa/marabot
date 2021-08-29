# Mara Bot

A Secret of Mana Randomizer bot for Discord.

## Available commands

- `!race presetName`: Generate a race using the specified preset.
- `!preset presetName`: Display information for the specified preset.
- `!presets`: Display all available presets. All presets are in the `presets` folder.
- `!newpreset [rawOptions]`: Generate a json preset for specified options. 
- `!weekly`: Generate or return the current weekly race. 
- `!completed HH:MM:SS`: Add your name to the leaderboard for the weekly race.
- `!leaderboard`: Display the weekly leaderboard.

## Requirements

- .Net Core: https://dotnet.microsoft.com/download
- DSharpPlus: https://github.com/DSharpPlus/DSharpPlus

## Configuration

First, follow these guidelines to set up a discord bot account:
https://dsharpplus.github.io/articles/basics/bot_account.html

You'll then need to create a `config.json` file with the OAuth token
required for the bot to authenticate itself with Discord.

```
{
  "prefix": "!",
  "token": "my-token-goes-here"
}
```

You can copy your `config.json` in either: 
- `marabot/config/`
- `$HOME/marabot/config/`
