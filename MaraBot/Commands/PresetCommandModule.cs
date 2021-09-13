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
    /// Implements the preset command.
    /// This command is used to display info on a given preset.
    /// </summary>
    public class PresetCommandModule : BaseCommandModule
    {
        /// <summary>
        /// List of presets available.
        /// </summary>
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }

        /// <summary>
        /// Executes the preset command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="presetName">Preset name to retrieve.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("preset")]
        [Description("Get the info on a given preset.")]
        [Cooldown(10, 600, CooldownBucketType.User)]
        [RequireGuild]
        [RequirePermissions(
            Permissions.SendMessages |
            Permissions.AddReactions)]
        public async Task Execute(CommandContext ctx, string presetName)
        {
            if (!Presets.ContainsKey(presetName))
            {
                await ctx.RespondAsync($"{presetName} is not a a valid preset");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            await ctx.RespondAsync(Display.PresetEmbed(Presets[presetName]));
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
