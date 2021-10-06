using System;
using System.Collections.Generic;
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
        /// Randomizer Options.
        /// </summary>
        public IReadOnlyDictionary<string, Option> Options { private get; set; }
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public IReadOnlyConfig Config { private get; set; }

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

            // Make a backup of the previous week's weekly and create a new
            // weekly for the current week.
            if (Weekly.WeekNumber != currentWeek)
            {
                try
                {
                    await CommandUtils.RevokeAllRolesAsync(ctx, new[]
                    {
                        Config.WeeklyCompletedRole,
                        Config.WeeklyForfeitedRole
                    });
                }
                catch (InvalidOperationException)
                {
                    await CommandUtils.SendFailReaction(ctx);
                    throw;
                }

                // Backup weekly settings to json before overriding.
                await WeeklyIO.StoreWeeklyAsync(Weekly, $"weekly.{Weekly.WeekNumber}.json");

                // Set weekly to unset settings until it's rolled out.
                Weekly.Load(Weekly.NotSet);
                await WeeklyIO.StoreWeeklyAsync(Weekly);
            }

            // Load in the new preset in attachment.
            Preset preset = default;
            try
            {
                preset = await CommandUtils.LoadPresetAttachmentAsync(ctx, Options);
            }
            catch (InvalidOperationException)
            {
                await CommandUtils.SendFailReaction(ctx);
                throw;
            }

            if (preset.Equals(default))
            {
                await ctx.RespondAsync("Not a valid weekly preset!");
                await CommandUtils.SendFailReaction(ctx);
            }

            await ctx.RespondAsync(CommandUtils.ValidatePresetOptions(ctx, preset, Options));

            // Roll or reroll weekly seed with preset options.
            Weekly.PresetName = "";
            Weekly.Preset = preset;
            Weekly.Seed = RandomUtils.GetRandomSeed();
            await WeeklyIO.StoreWeeklyAsync(Weekly);

            await ctx.RespondAsync(Display.RaceEmbed(preset, Weekly.Seed, Weekly.Timestamp));

            await ctx.RespondAsync("Weekly has been successfully reset!");
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
