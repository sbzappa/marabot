using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MaraBot.Core;
using MaraBot.Messages;

namespace MaraBot.Commands
{
    public class CompletedCommandModule : BaseCommandModule
    {
        public Weekly Weekly { private get; set; }

        [Command("done")]
        [Cooldown(2, 900, CooldownBucketType.User)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx, TimeSpan time)
        {
            var weekNumber = RandomUtils.GetWeekNumber();
            if (Weekly.WeekNumber != weekNumber)
            {
                await ctx.RespondAsync("No weekly seed generated for this week. Generate one using the weekly command first.");

                var invalidEmoji = DiscordEmoji.FromName(ctx.Client, Display.kInvalidCommandEmoji);
                await ctx.Message.CreateReactionAsync(invalidEmoji);
            }

            var username = ctx.User.Username;

            if (Weekly.Leaderboard == null)
                Weekly.Leaderboard = new Dictionary<string, TimeSpan>();

            if (Weekly.Leaderboard.ContainsKey(username))
                Weekly.Leaderboard[username] = time;
            else
                Weekly.Leaderboard.Add(username, time);

            WeeklyIO.StoreWeekly(Weekly);
            await Display.Leaderboard(ctx, Weekly);

            var validEmoji = DiscordEmoji.FromName(ctx.Client, Display.kValidCommandEmoji);
            await ctx.Message.CreateReactionAsync(validEmoji);
        }
    }
}
