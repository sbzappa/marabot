# Mara Bot

A Secret of Mana Randomizer bot for Discord.

## Available commands

- `!race presetName`: Generate a race using the specified preset.
- `!custom`: Generate a custom race from an imported .json file (Requires file to be uploaded to Discord).
- `!preset presetName`: Display information for the specified preset.
- `!presets`: Display all available presets. All presets are in the `presets/` folder.
- `!newpreset rawOptions`: Generate a JSON preset (with options filled in, if given). Only available in DMs to reduce spam.
- `!weekly`: Display the current weekly race settings. 
- `!completed HH:MM:SS`: Add your name to the leaderboard for the weekly race or override your time in the leaderboard with a new one. Also gain access to the spoiler channel.
- `!forfeit`: Forfeit the weekly, but gain access to the spoiler channel.
- `!leaderboard [weekNumber]`: Display specified week's leaderboard. Without parameters, display this week's leaderboard if in spoiler channel.

## Running a bot instance
### Requirements

- .Net Core: https://dotnet.microsoft.com/download
- DSharpPlus: https://github.com/DSharpPlus/DSharpPlus

### Permissions

For all commands to work, the following permissions must be given to the bot on discord

- Send Messages
- Manage Messages
- Manage Roles (only required for roles defined in `config.json`)
- Access Channels
- Add Reactions

### Configuration

First, follow these guidelines to set up a discord bot account:
https://dsharpplus.github.io/articles/basics/bot_account.html

You'll then need to create a `config.json` file with the OAuth token
required for the bot to authenticate itself with Discord. Look for
`config/config_template.json` in the repository for an example, or see below:

```
  "prefix": "!",
  "token": "my-token-goes-here",
  "organizerRoles": [
    "organizes races"
  ],
  "weeklyCompletedRole": "did the weekly",
  "weeklyForfeitedRole": "forfeited the weekly",
  "weeklySpoilerChannel": "spoilers"
}
```

You can copy your `config.json` in either: 
- `config/`
- `$HOME/marabot/config/`
