using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MaraBot.Commands
{
    using Core;
    using Messages;

    /// <summary>
    /// Implements the presets command.
    /// This command is used to display all available presets.
    /// </summary>
    public class PresetsCommandModule : BaseCommandModule
    {
        /// <summary>
        /// List of presets available.
        /// </summary>
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }

        /// <summary>
        /// Executes the presets command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("presets")]
        [Description("Get the list of presets.")]
        [Cooldown(5, 600, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequireBotPermissions(Permissions.SendMessages)]
        public async Task Execute(CommandContext ctx)
        {
            await ctx.RespondAsync(Display.PresetsEmbed(Presets));
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
