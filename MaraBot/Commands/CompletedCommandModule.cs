using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MaraBot.Commands
{
    public class CompletedCommandModule : BaseCommandModule
    {
        [Command("completed")]
        [Cooldown(2, 900, CooldownBucketType.User)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx)
        {
            // TODO...
            // 1. Retrieve the race time set in parameter and add it to the leaderboard for the weekly.
            throw new NotImplementedException();
        }
    }
}