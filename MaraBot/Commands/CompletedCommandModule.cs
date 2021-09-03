using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
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

        [Command("completed")]
        [Aliases("done")]
        [Description("Add your time to the leaderboard.")]
        [Cooldown(2, 900, CooldownBucketType.User)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx, TimeSpan time)
        {
            // Delete user message to avoid spoilers, if we can delete the message
            if(await CommandUtils.HasBotPermissions(ctx, Permissions.ManageMessages))
                await ctx.Message.DeleteAsync();

            await ctx.RespondAsync($"Adding {ctx.User.Mention} to the leaderboard!");

            var username = ctx.User.Username;

            if (Weekly.Leaderboard == null)
                Weekly.Leaderboard = new Dictionary<string, TimeSpan>();

            if (Weekly.Leaderboard.ContainsKey(username))
                Weekly.Leaderboard[username] = time;
            else
                Weekly.Leaderboard.Add(username, time);

            WeeklyIO.StoreWeekly(Weekly);
            await Display.Leaderboard(ctx, Weekly, true);
        }
    }
}
