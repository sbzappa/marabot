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
        [RequirePermissions(
            Permissions.SendMessages |
            Permissions.ManageMessages |
            Permissions.ManageRoles |
            Permissions.AccessChannels)]
        public async Task Execute(CommandContext ctx, TimeSpan time)
        {
            // Delete user message to avoid spoilers, if we can delete the message.
            if (await CommandUtils.HasBotPermissions(ctx, Permissions.ManageMessages))
                await ctx.Message.DeleteAsync();

            // Add user to leaderboard.
            var username = ctx.User.Username;

            if (Weekly.Leaderboard == null)
                Weekly.Leaderboard = new Dictionary<string, TimeSpan>();

            if (Weekly.Leaderboard.ContainsKey(username))
                Weekly.Leaderboard[username] = time;
            else
                Weekly.Leaderboard.Add(username, time);

            WeeklyIO.StoreWeeklyAsync(Weekly);

            await ctx.RespondAsync($"Adding {ctx.User.Mention} to the leaderboard!");

            // Grant user their new role.
            var newRole = ctx.Guild.Roles
                .FirstOrDefault(role => Config.WeeklyCompletedRole.Equals(role.Value.Name));

            if (newRole.Value == null)
            {
                await ctx.RespondAsync(
                    "No role set for access to spoiler channel.\n" +
                    "This shouldn't happen! Please contact your friendly neighbourhood developers!");
                return;
            }
            else
            {
                await ctx.Member.GrantRoleAsync(newRole.Value);
            }

            // Display leaderboard in the spoiler channel.
            var spoilerChannel = ctx.Guild.Channels
                .FirstOrDefault(channel => Config.WeeklySpoilerChannel.Equals(channel.Value.Name));

            if (spoilerChannel.Value == null)
            {
                await ctx.RespondAsync(
                    "No spoiler channel set.\n" +
                    "This shouldn't happen! Please contact your friendly neighbourhood developers!");
                return;
            }
            else
            {
                await spoilerChannel.Value.SendMessageAsync(Display.LeaderboardEmbed(ctx, Weekly, false));
            }
        }
    }
}
