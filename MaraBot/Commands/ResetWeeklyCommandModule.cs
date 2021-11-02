using System;
using System.Collections.Generic;
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
        public Config Config { private get; set; }

        /// <summary>
        /// Executes the weekly command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="rawArgs">Command line arguments.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("reset")]
        [Description("Resets the weekly race.")]
        [Cooldown(2, 900, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequireBotPermissions(
            Permissions.SendMessages |
            Permissions.ManageRoles)]
        public async Task Execute(CommandContext ctx, [RemainingText][Description(CommandUtils.kCustomRaceArgsDescription)] string rawArgs)
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
            Preset preset;
            string seed;
            string validationHash;
            try
            {
                (preset, seed, validationHash) = await CommandUtils.LoadRaceAttachment(ctx, rawArgs, Options);
            }
            catch (InvalidOperationException e)
            {
                await ctx.RespondAsync(e.Message);
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            if (preset.Equals(default))
            {
                await ctx.RespondAsync("Not a valid weekly preset!");
                await CommandUtils.SendFailReaction(ctx);
            }

            await ctx.RespondAsync(PresetValidation.GenerateValidationMessage(preset, Options));

            Weekly.PresetName = "";
            Weekly.Preset = preset;
            if (string.IsNullOrEmpty(seed))
            {
                Weekly.Seed = RandomUtils.GetRandomSeed();
            }
            else
            {
                Weekly.Seed = seed;
                Weekly.ValidationHash = validationHash;
            }

            await WeeklyIO.StoreWeeklyAsync(Weekly);

            await ctx.RespondAsync(Display.RaceEmbed(preset, Weekly.Seed, Weekly.ValidationHash, Weekly.Timestamp));

            await ctx.RespondAsync("Weekly has been successfully reset!");
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
