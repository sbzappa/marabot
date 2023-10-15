using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MaraBot.Core;
using MaraBot.Messages;

namespace MaraBot.Commands
{
    /// <summary>
    /// Implements the race command.
    /// </summary>
    public class RaceCommandModule : BaseCommandModule
    {
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
        /// Executes the race command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="rawArgs">Command line arguments.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("race")]
        [Description(
            "Generate a new race.\n" +
            "- Upload a log file to generate from an existing race\n" +
            "- OR upload a preset file to generate from a custom preset\n" +
            "- OR generate a race using random weekly settings.")]
        [Cooldown(5, 600, CooldownBucketType.User)]
        [RequireGuild]
        [RequireBotPermissions(Permissions.SendMessages)]
        public async Task Execute(CommandContext ctx, [RemainingText][Description(CommandUtils.kCustomRaceArgsDescription)] string rawArgs)
        {
            Preset preset;
            string seed, validationHash;
            string author, name, description;
;
            try
            {
                // Parse command line arguments to retrieve preset author, name and description if available.
                CommandUtils.ParseCustomRaceCommandLineArguments(rawArgs, out author, out name, out description);

                (preset, seed, validationHash) = await CommandUtils.GenerateRace(ctx, author, name, description, MysterySettings, Options);
            }
            catch (InvalidOperationException e)
            {
                await ctx.RespondAsync(e.Message);
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            // Print validation in separate message to make sure
            // we can pin just the race embed
            await ctx.RespondAsync(PresetValidation.GenerateValidationMessage(preset, Options));

            if (string.IsNullOrEmpty(seed))
                seed = RandomUtils.GetRandomSeed();

            if (String.IsNullOrEmpty(validationHash))
            {
                (preset, seed, validationHash) = await CommandUtils.GenerateSeed(ctx, preset, seed, Config.RandomizerBinaryPath, Config.RomPath, Options);
            }

            await ctx.RespondAsync(Display.RaceEmbed(preset, seed, validationHash));
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
