using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace MaraBot.Commands
{
    using Core;
    using Messages;

    public class WeeklyCommandModule : BaseCommandModule
    {
        public Weekly Weekly { private get; set; }
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }

        [Command("weekly")]
        [Cooldown(2, 900, CooldownBucketType.Channel)]
        [RequireGuild]
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
            await Display.Race(ctx, preset, Weekly.Seed, Weekly.Timestamp);
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
