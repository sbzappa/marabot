using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Bot configuration.
        /// </summary>
        public IConfig Config { private get; set; }

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
        [RequireBotPermissions(
            Permissions.SendMessages |
            Permissions.ManageMessages |
            Permissions.ManageRoles |
            Permissions.AccessChannels)]
        public async Task Execute(CommandContext ctx, TimeSpan time)
        {
            // Give a sufficient delay before deleting message
            await Task.Delay(500);

            // Delete user message to avoid spoilers, if we can delete the message.
            await ctx.Message.DeleteAsync();

            // Add user to leaderboard.
            Weekly.AddToLeaderboard(ctx.User.Username, time);
            WeeklyIO.StoreWeeklyAsync(Weekly);

            // Send message in current channel and in spoiler channel.
            var message = $"Adding {ctx.User.Mention} to the leaderboard!";
            await ctx.RespondAsync(message);
            await CommandUtils.SendToChannelAsync(ctx, Config.WeeklySpoilerChannel, message);

            // Grant user their new role.
            await CommandUtils.GrantRolesToSelfAsync(ctx, new [] {Config.WeeklyCompletedRole});

            // Display leaderboard in the spoiler channel.
            await CommandUtils.SendToChannelAsync(ctx, Config.WeeklySpoilerChannel, Display.LeaderboardEmbed(Weekly, false));
        }
    }
}
