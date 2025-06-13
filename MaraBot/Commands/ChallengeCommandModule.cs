using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MaraBot.Core;
using MaraBot.Messages;
using Microsoft.Extensions.Logging;

namespace MaraBot.Commands
{
    /// <summary>
    /// Implements the race command.
    /// </summary>
    public class ChallengeCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Randomizer Options.
        /// </summary>
        public IReadOnlyDictionary<string, Option> Options { private get; set; }
        /// <summary>
        /// Challenge presets.
        /// </summary>
        public IReadOnlyDictionary<string, Preset> Challenges { private get; set; }
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public Config Config { private get; set; }
        /// <summary>
        /// Mutex registry.
        /// </summary>
        public MutexRegistry MutexRegistry { private get; set; }

        /// <summary>
        /// Executes the challenge command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="rawArgs">Command line arguments.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("challenge")]
        [Description(
            "Generate a new challenge race.\n" +
            "- Choose from an existing challenge preset\n" +
            "- OR generate a race using a random challenge preset.")]
        [Cooldown(5, 600, CooldownBucketType.User)]
        [RequireGuild]
        [RequireBotPermissions(Permissions.SendMessages)]
        public async Task Execute(CommandContext ctx, [Description("(optional) preset name")] string presetName = "")
        {
            Preset preset;

            // Generate from random preset.
            if (String.IsNullOrEmpty(presetName))
            {
                var index = WeeklyUtils.GetRandomIndex(0, Challenges.Count);
                preset = Challenges.Values.ElementAt(index);
            }
            // Generate from existing preset name.
            else if (!Challenges.TryGetValue(presetName, out preset))
            {
                await ctx.RespondAsync($"Invalid preset {presetName}. Type !challenges for a list of all challenge presets.");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            // Print validation in separate message to make sure
            // we can pin just the race embed
            await ctx.RespondAsync(PresetValidation.GenerateValidationMessage(preset, Options));

            var seed = WeeklyUtils.GetRandomSeed();

            var response = await ctx.RespondAsync(Display.RaceEmbed(preset, seed));
            await CommandUtils.SendSuccessReaction(ctx);

            var validationHash = String.Empty;
            try
            {
                var (newPreset, newSeed, newValidationHash) = await CommandUtils.GenerateValidationHash(preset, seed, Config, Options, MutexRegistry);
                if (newPreset.Equals(preset) && newSeed.Equals(seed))
                {
                    validationHash = newValidationHash;
                }
            }
            catch (Exception exception)
            {
                ctx.Client.Logger.LogWarning(
                    "Could not create a validation hash.\n" +
                    exception.Message);
            }

            //await response.ModifyAsync(Display.RaceEmbed(preset, seed, validationHash).Build());
            await CommandUtils.SendRaceValidatedReaction(ctx);
        }
    }
}
