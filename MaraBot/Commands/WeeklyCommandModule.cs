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
    /// Implements the weekly command.
    /// This command is used to display the current weekly race.
    /// </summary>
    public class WeeklyCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Weekly settings.
        /// </summary>
        public Weekly Weekly { private get; set; }
        /// <summary>
        /// List of presets available.
        /// </summary>
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }

        /// <summary>
        /// Executes the weekly command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("weekly")]
        [Description("Get the weekly race.")]
        [Cooldown(2, 900, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequirePermissions(
            Permissions.SendMessages |
            Permissions.AddReactions)]
        public async Task Execute(CommandContext ctx)
        {
            if (!Presets.ContainsKey(Weekly.PresetName) || Weekly.WeekNumber < 0)
            {
                await ctx.RespondAsync(
                    $"Weekly preset '{Weekly.PresetName}' is not a a valid preset\n" +
                    "This shouldn't happen! Please contact your friendly neighbourhood developers!");
                await CommandUtils.SendFailReaction(ctx, false);
                return;
            }

            var preset = Presets[Weekly.PresetName];
            await ctx.RespondAsync(Display.RaceEmbed(ctx, preset, Weekly.Seed, Weekly.Timestamp));
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
