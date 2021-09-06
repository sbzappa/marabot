using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MaraBot.Commands
{
    using Core;
    using IO;
    using Messages;

    /// <summary>
    /// Implements the completed command.
    /// This command is used to add your time to the leaderboard.
    /// </summary>
    public class CompletedCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Weekly settings.
        /// </summary>
        public Weekly Weekly { private get; set; }

        /// <summary>
        /// Executes the completed command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="time">Elapsed time. Expecting HH:MM:SS format.</param>
        /// <returns>Returns an asynchronous task.</returns>
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

            WeeklyIO.StoreWeeklyAsync(Weekly);
            await Display.LeaderboardAsync(ctx, Weekly, true);
        }
    }
}
