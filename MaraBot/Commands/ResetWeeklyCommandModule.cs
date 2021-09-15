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
        [RequireBotPermissions(
            Permissions.SendMessages |
            Permissions.AddReactions |
            Permissions.ManageRoles)]
        public async Task Execute(CommandContext ctx)
        {
            // Safety measure to avoid potential misuses of this command.
            if (!await CommandUtils.MemberHasPermittedRole(ctx, Config.OrganizerRoles))
            {
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            var currentWeek = RandomUtils.GetWeekNumber();

            if (Weekly.WeekNumber == currentWeek)
            {
                await ctx.RespondAsync($"Weekly for week {currentWeek} already exists.");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            // Backup weekly settings to json before overriding.
            WeeklyIO.StoreWeeklyAsync(Weekly, $"weekly.{Weekly.WeekNumber}.json");

            // Set weekly to unset settings until it's rolled out.
            Weekly.Load(Weekly.NotSet);
            WeeklyIO.StoreWeeklyAsync(Weekly);

            await CommandUtils.RevokeSpoilerRoles(ctx, new []
            {
                Config.WeeklyCompletedRole,
                Config.WeeklyForfeitedRole
            });

            await ctx.RespondAsync("Weekly has been successfully reset!");
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
