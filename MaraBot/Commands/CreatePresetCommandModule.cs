using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using Newtonsoft.Json;

namespace MaraBot.Commands
{
    using Core;
    using IO;

    /// <summary>
    /// Implements the newpreset command.
    /// This command is used to create a dummy preset file with specified options.
    /// </summary>
    public class CreatePresetCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Randomizer Options.
        /// </summary>
        public IReadOnlyDictionary<string, Option> Options { private get; set; }

        /// <summary>
        /// Executes the newpreset command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="optionString">Raw options string.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("newpreset")]
        [Description("Create a dummy preset file, with options if given.")]
        [Cooldown(5, 600, CooldownBucketType.User)]
        [RequireDirectMessage]
        [RequireBotPermissions(Permissions.SendMessages)]
        public async Task Execute(CommandContext ctx, [RemainingText][Description("Raw options")] string optionString)
        {
            Preset preset;
            try
            {
                preset = CommandUtils.CreatePresetFromOptionsString( ctx.User.Username, "DummyName", "DummyDescription", optionString);
            }
            catch (InvalidOperationException e)
            {
                await ctx.RespondAsync(e.Message);
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            string message = "";
            if (optionString != null)
                message += PresetValidation.GenerateValidationMessage(preset, Options);

            string json = PresetIO.StorePreset(preset);
            message += Formatter.BlockCode(json);

            await ctx.RespondAsync(message);
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
