using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MaraBot.Core;
using MaraBot.Messages;

namespace MaraBot.Commands
{
    using IO;

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
        public IConfig Config { private get; set; }

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

            if (ctx.Message.Attachments == null || ctx.Message.Attachments.Count != 1)
            {
                await ctx.RespondAsync("No attachment for custom race. You must supply a valid json file.");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            var attachment = ctx.Message.Attachments[0];
            var url = attachment.Url;

            var request = WebRequest.Create(url);
            var responseTask = request.GetResponseAsync();

            if (Path.GetExtension(attachment.FileName).ToLower() != ".json")
            {
                await ctx.RespondAsync("Expected file extension to be .json.");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            var response = await responseTask;
            var dataStream = response.GetResponseStream();

            using (StreamReader r = new StreamReader(dataStream))
            {
                var jsonContent = await r.ReadToEndAsync();
                var preset = PresetIO.LoadPreset(jsonContent, Options);
                if (preset == null)
                {
                    await ctx.RespondAsync($"Could not parse custom json preset. Please supply a valid json file.");
                    await CommandUtils.SendFailReaction(ctx);
                    return;
                }

                string validationMessage = "";
                if (preset.Version == PresetValidation.VERSION)
                    validationMessage += "**Preset has been validated successfully. Result:**\n";
                else
                    validationMessage += $"**Preset randomizer version {preset.Version} doesn't match validator randomizer version {PresetValidation.VERSION}. Validation might be wrong in certain places. Validation Result:**\n";

                List<string> errors = PresetValidation.ValidateOptions(preset.Options);
                foreach (var e in errors)
                    validationMessage += $"> {e}\n";

                // Print validation in separate message to make sure
                // we can pin just the race embed
                await ctx.RespondAsync(validationMessage);

                var seed = RandomUtils.GetRandomSeed();
                await ctx.RespondAsync(Display.RaceEmbed(preset, seed));
            }

            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
