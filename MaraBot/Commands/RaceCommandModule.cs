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
    /// Implements the race command.
    /// This command is used to create a new create with a specified preset.
    /// </summary>
    public class RaceCommandModule : BaseCommandModule
    {
        /// <summary>
        /// List of presets available.
        /// </summary>
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }

        /// <summary>
        /// Executes the race command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="presetName">Preset name to create the race with.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("race")]
        [Description("Generate a race based on the preset given.")]
        [Cooldown(3, 600, CooldownBucketType.User)]
        [RequireGuild]
        [RequirePermissions(Permissions.SendMessages)]
        public async Task Execute(CommandContext ctx, string presetName)
        {
            if (!Presets.ContainsKey(presetName))
            {
                await ctx.RespondAsync($"{presetName} is not a a valid preset");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            var seed = RandomUtils.GetRandomSeed();
            var preset = Presets[presetName];

            await ctx.RespondAsync(Display.RaceEmbed(preset, seed));
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
