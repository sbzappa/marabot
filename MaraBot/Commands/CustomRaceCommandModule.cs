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
        public IReadOnlyConfig Config { private get; set; }

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

            if (!preset.Equals(default))
            {
                // Print validation in separate message to make sure
                // we can pin just the race embed
                await ctx.RespondAsync(CommandUtils.ValidatePresetOptions(ctx, preset, Options));

                var seed = RandomUtils.GetRandomSeed();
                await ctx.RespondAsync(Display.RaceEmbed(preset, seed));
            }

            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
