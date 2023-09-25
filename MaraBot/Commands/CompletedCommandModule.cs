using System;
using System.Text.RegularExpressions;
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
        public Config Config { private get; set; }

        /// <summary>
        /// Executes the completed command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="timeRaw">Elapsed time. Expecting HH:MM:SS format.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("completed")]
        [Aliases("done")]
        [Description("Add your time to the leaderboard.")]
        [Cooldown(5, 600, CooldownBucketType.User)]
        [RequireGuild]
        [RequireBotPermissions(
            Permissions.SendMessages |
            Permissions.ManageMessages |
            Permissions.ManageRoles |
            Permissions.AccessChannels)]
        public async Task Execute(CommandContext ctx, [RemainingText]string timeRaw)
        {
            // Give a sufficient delay before deleting message
            await Task.Delay(500);

            // Delete user message to avoid spoilers, if we can delete the message.
            await ctx.Message.DeleteAsync();

            // Remove potential emojis embedded in time...
            timeRaw = Regex.Replace(timeRaw, "<(?<text>(:.+?:)).+?>", "$1");

            if (!TimeSpan.TryParse(timeRaw, out var time))
            {
                await ctx.RespondAsync($"Invalid time! Expecting time with a HH:MM:SS format.\n");
                return;
            }

            // Add user to leaderboard.
            Weekly.AddToLeaderboard(ctx.User.Username, time);
            await WeeklyIO.StoreWeeklyAsync(Weekly);

            // Send message in current channel and in spoiler channel.
            var message = $"Adding {ctx.User.Mention} to the leaderboard!";
            await CommandUtils.SendToChannelAsync(ctx, Config.WeeklySpoilerChannel, message);

            // Grant user their new role.
            await CommandUtils.GrantRolesToSelfAsync(ctx, new [] {Config.WeeklyCompletedRole});

            // Display leaderboard in the spoiler channel.
            await CommandUtils.SendToChannelAsync(ctx, Config.WeeklySpoilerChannel, await Display.LeaderboardEmbedAsync(ctx.Guild, Weekly, false));
        }
    }
}
