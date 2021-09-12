using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MaraBot.IO;

namespace MaraBot.Commands
{
    using Core;
    using Messages;

    /// <summary>
    /// Implements the reset command.
    /// This command is used to reset the current weekly race.
    /// </summary>
    public class ResetWeeklyCommandModule : BaseCommandModule
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
        /// Executes the weekly command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("reset")]
        [Description("Resets the weekly race.")]
        [Cooldown(2, 900, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequirePermissions(
            Permissions.SendMessages |
            Permissions.AddReactions |
            Permissions.ManageRoles)]
        public async Task Execute(CommandContext ctx)
        {
            // Safety measure to avoid potential misuses of this command.
            if (!ctx.Member.Roles.Any(role => (Config.OrganizerRoles?.Contains(role.Name)).GetValueOrDefault()))
            {
                var guildRoles = ctx.Guild.Roles
                    .Where(role => (Config.OrganizerRoles?.Contains(role.Value.Name)).GetValueOrDefault());

                await ctx.RespondAsync(
                    "Insufficient privileges to reset the weekly.\n" +
                    "This command is only available to the following roles:\n" +
                    String.Join(
                        ", ",
                        CommandUtils.MentionRoleWithoutPing(ctx, guildRoles.Select(r => r.Value).ToArray())
                    )
                );
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            var currentWeek = RandomUtils.GetWeekNumber();

            if (Weekly.WeekNumber == currentWeek)
            {
                await ctx.RespondAsync($"Weekly for week {currentWeek} already exists.");
                await CommandUtils.SendFailReaction(ctx, false);
                return;
            }

            // Backup weekly settings to json before overriding.
            WeeklyIO.StoreWeeklyAsync(Weekly, $"weekly.{Weekly.WeekNumber}.json");

            // Set weekly to unset settings until it's rolled out.
            Weekly.Load(Weekly.NotSet);
            WeeklyIO.StoreWeeklyAsync(Weekly);

            string[] spoilerRolenames =
            {
                Config.WeeklyCompletedRole,
                Config.WeeklyForfeitedRole
            };

            var spoilerRoles = ctx.Guild.Roles
                .Where(role => spoilerRolenames.Contains(role.Value.Name))
                .Select(role => role.Value);

            await CommandUtils.RevokeSpoilerRoles(ctx, spoilerRoles);

            await ctx.RespondAsync("Weekly has been successfully reset!");
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
