# Mara Bot

A Secret of Mana Randomizer bot for Discord.

## Available commands

- `!race`: Generate a custom race using the .json preset or .txt log file attached to this message. If no file is attached, generate a randomized race using mystery weekly settings.
- `!newpreset [rawOptions]`: Generate a JSON preset (with options filled in, if given). Only available in DMs to reduce spam.
- `!weekly`: Display the current weekly race settings. 
- `!completed <HH:MM:SS>`: Add your name to the leaderboard for the weekly race or override your time in the leaderboard with a new one. Also gain access to the spoiler channel.
- `!forfeit`: Forfeit the weekly, but gain access to the spoiler channel. WIll add you to the leaderboard as DNF.
- `!leaderboard [weekNumber]`: Display specified week's leaderboard. Without parameters, display this week's leaderboard (only if in spoiler channel).
- `!reset [--author string][--name string][--description string]`: Reset the current weekly when the week is over and create a new race using a .json preset or .txt log file attached to this message. If no file is attached, generate a randomized race using mystery weekly settings (only available to people with a race organizer role for security reasons).
- `!challenge [presetName]`: Generate a custom challenge race using a preset name or challenge randomly from the list of challenge presets.
- `!challenges`: Browse through existing challenge presets.
- `!spoiler [--done member][--completed member][--forfeit member][--revoke member]`: Grant or Revoke spoiler roles manually (only available to people with a race organizer role for security reasons).
- `!8ball [rawOptions]`: Ask Mara a question and bask in her wisdom!!! 

## Running a bot instance
### Requirements

- .Net Core 3.1: https://dotnet.microsoft.com/download

### Permissions

For all commands to work, the following permissions must be given to the bot on discord:

- Send Messages
- Manage Messages
- Manage Roles (only required for roles defined in `config.json`)
- Access Channels
- Add Reactions (optional, used to confirm execution of a command)

### Configuration

First, follow these guidelines to set up a discord bot account:
https://dsharpplus.github.io/articles/basics/bot_account.html

You'll then need to create a `config.json` file with the OAuth token
required for the bot to authenticate itself with Discord. Look for
`config/config_template.json` in the repository for an example, or see below:

```
{
  "prefix": "!",
  "token": "my-token-goes-here",
  "organizerRoles": [
    "organizes races"
  ],
  "weeklyCompletedRole": "did the weekly",
  "weeklyForfeitedRole": "forfeited the weekly",
  "weeklyChannel": "weekly",
  "weeklySpoilerChannel": "spoilers",
  "challengeChannel": "challenges",
  "randomizerVersion": "version of randomizer executable",
  "randomizerExecutablePath": "(optional) path to randomizer executable",
  "romPath": "(optional) path to rom"
}
```

You can copy your `config.json` in either: 
- `config/`
- `$HOME/marabot/config/`

The randomizer executable and the associated rom can be used to generate validation hashes.
The rom is never distributed.

### Compilation and Running

To compile the bot, run `dotnet build` in the project root.

The executable will be located at `MaraBot/bin/Debug/netcoreapp3.1/MaraBot`.
A daemon script for Linux is provided at `linux/marabotd`.

You can also build in release mode by running `dotnet build -c Release`.
The executable will then be located at `MaraBot/bin/Release/netcoreapp3.1/MaraBot`.
Note that the daemon assumes we have a fresh release build.
