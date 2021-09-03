using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MaraBot.Commands
{
    using Core;
    using Messages;

    public class RaceCommandModule : BaseCommandModule
    {
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }

        [Command("race")]
        [Description("Generate a race based on the preset given.")]
        [Cooldown(3, 600, CooldownBucketType.User)]
        [RequireGuild]
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

            await Display.Race(ctx, preset, seed);
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
