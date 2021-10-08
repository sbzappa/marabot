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
    /// Implements the custom command.
    /// This command is used to create a race using a custom .json preset file.
    /// </summary>
    public class CustomRaceCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Randomizer Options.
        /// </summary>
        public IReadOnlyDictionary<string, Option> Options { private get; set; }
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public Config Config { private get; set; }

        /// <summary>
        /// Executes the custom command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("custom")]
        [Description("Start a custom race based on a .json file")]
        [Cooldown(3, 600, CooldownBucketType.User)]
        [RequireGuild]
        [RequireBotPermissions(Permissions.SendMessages)]
        public async Task Execute(CommandContext ctx)
        {
            // Safety measure to avoid potential misuses of this command. May be revisited in the future.
            if (!await CommandUtils.MemberHasPermittedRole(ctx, Config.OrganizerRoles))
            {
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            Preset preset;
            var seed = string.Empty;
            try
            {
                (preset, seed) = await CommandUtils.LoadRaceAttachment(ctx, Options);
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

            await ctx.RespondAsync(Display.RaceEmbed(preset, seed));
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
