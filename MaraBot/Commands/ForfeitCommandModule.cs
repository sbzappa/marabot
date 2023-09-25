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
    /// Implements the forfeit command.
    /// This command is used to add your time to the leaderboard.
    /// </summary>
    public class ForfeitCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Weekly settings.
        /// </summary>
        public Weekly Weekly { private get; set; }
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public Config Config { private get; set; }

        /// <summary>
        /// Executes the forfeit command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("forfeit")]
        [Aliases("forfeited", "ff")]
        [Description("Forfeit the weekly.")]
        [Cooldown(5, 600, CooldownBucketType.User)]
        [RequireGuild]
        [RequireBotPermissions(
            Permissions.SendMessages |
            Permissions.ManageMessages |
            Permissions.ManageRoles |
            Permissions.AccessChannels)]
        public async Task Execute(CommandContext ctx)
        {
            // Give a sufficient delay before deleting message
            await Task.Delay(500);

            // Delete user message to avoid spoilers, if we can delete the message.
            await ctx.Message.DeleteAsync();

            // Add user to leaderboard.
            Weekly.AddToLeaderboard(ctx.User.Username, TimeSpan.MaxValue);
            await WeeklyIO.StoreWeeklyAsync(Weekly);

            // Send message in current channel and in spoiler channel.
            var message = $"{ctx.User.Mention} forfeited the weekly!";
            await ctx.RespondAsync(message);
            await CommandUtils.SendToChannelAsync(ctx, Config.WeeklySpoilerChannel, message);

            // Grant user their new role.
            await CommandUtils.GrantRolesToSelfAsync(ctx, new [] {Config.WeeklyForfeitedRole});

            // Display leaderboard in the spoiler channel.
            await CommandUtils.SendToChannelAsync(ctx, Config.WeeklySpoilerChannel, await Display.LeaderboardEmbedAsync(ctx.Guild, Weekly, false));
        }
    }
}
