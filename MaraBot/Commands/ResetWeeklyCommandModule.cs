using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MaraBot.IO;
using Microsoft.Extensions.Logging;

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
        /// Mystery Settings.
        /// </summary>
        public IReadOnlyDictionary<string, MysterySetting> MysterySettings { private get; set; }
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public Config Config { private get; set; }
        /// <summary>
        /// Mutex registry.
        /// </summary>
        public MutexRegistry MutexRegistry { private get; set; }

        /// <summary>
        /// Executes the weekly command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="rawArgs">Command line arguments.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("reset")]
        [Description(
            "(Admin command) Resets the weekly race.\n" +
            "- Upload a log file to generate from an existing race\n" +
            "- OR upload a preset file to generate from a custom preset\n" +
            "- OR generate a race using random weekly settings.")]
        [Cooldown(30, 600, CooldownBucketType.Channel)]
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

            var previousWeek = Weekly.WeekNumber;
            var currentWeek = WeeklyUtils.GetWeekNumber();
            var backupAndResetWeekly = previousWeek != currentWeek;

            using var mutexLock = await MutexLock.WaitAsync(MutexRegistry.WeeklyWriteAccessMutex);

            // Make a backup of the previous week's weekly and create a new
            // weekly for the current week.
            if (backupAndResetWeekly)
            {
                try
                {
                    await CommandUtils.RevokeAllRolesAsync(ctx.Guild, new[]
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
                await WeeklyIO.StoreWeeklyAsync(Weekly, $"weekly.{previousWeek}.json");

                // Set weekly to unset settings until it's rolled out.
                Weekly.Load(Weekly.NotSet);
                await WeeklyIO.StoreWeeklyAsync(Weekly);

                await ctx.RespondAsync($"Weekly leaderboard has been successfully reset!");
                await CommandUtils.SendSuccessReaction(ctx);
            }

            // Load in the new preset in attachment.
            Preset preset;
            string seed, validationHash;
            string author, name, description;

            try
            {
                // Parse command line arguments to retrieve preset author, name and description if available.
                CommandUtils.ParseCustomRaceCommandLineArguments(rawArgs, out author, out name, out description);

                (preset, seed, validationHash) = await CommandUtils.GenerateRace(ctx, author, name, description, MysterySettings, Options, Config);
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

            Weekly.PresetName = String.Empty;
            Weekly.Preset = preset;
            if (string.IsNullOrEmpty(seed))
            {
                Weekly.Seed = WeeklyUtils.GetRandomSeed();
            }
            else
            {
                Weekly.Seed = seed;
                Weekly.ValidationHash = validationHash;
            }

            if (String.IsNullOrEmpty(Weekly.ValidationHash))
            {
                try
                {
                    var (newPreset, newSeed, newValidationHash) = await CommandUtils.GenerateValidationHash(ctx, Weekly.Preset, Weekly.Seed, Config, Options, MutexRegistry);
                    if (newPreset.Equals(preset) && newSeed.Equals(seed))
                    {
                        Weekly.ValidationHash = newValidationHash;
                    }
                }
                catch (Exception exception)
                {
                    ctx.Client.Logger.LogWarning(
                        "Could not create a validation hash.\n" +
                        exception.Message);
                }
            }

            await WeeklyIO.StoreWeeklyAsync(Weekly);

            await ctx.RespondAsync(Display.RaceEmbed(preset, Weekly.Seed, Weekly.ValidationHash, Weekly.Timestamp));

            await ctx.RespondAsync("Weekly preset has been successfully reset!");
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
