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
        public IReadOnlyWeekly Weekly { private get; set; }
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
        [RequireBotPermissions(Permissions.SendMessages)]
        public async Task Execute(CommandContext ctx)
        {
            if (Weekly.WeekNumber < 0)
            {
                await ctx.RespondAsync(
                    $"Weekly week number '{Weekly.WeekNumber}' is not valid!\n" +
                    CommandUtils.kFriendlyMessage);
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            if (!string.IsNullOrEmpty(Weekly.PresetName))
            {
                if (Presets.TryGetValue(Weekly.PresetName, out var preset))
                {
                    await ctx.RespondAsync(Display.RaceEmbed(preset, Weekly.Seed, Weekly.Timestamp));
                    await CommandUtils.SendSuccessReaction(ctx);
                    return;
                }

                await ctx.RespondAsync($"Weekly preset '{Weekly.PresetName}' is not a a valid preset\n");
                await CommandUtils.SendFailReaction(ctx);
            }
            else
            {
                await ctx.RespondAsync(Display.RaceEmbed(Weekly.Preset, Weekly.Seed, Weekly.Timestamp));
                await CommandUtils.SendSuccessReaction(ctx);
            }
        }
    }
}
