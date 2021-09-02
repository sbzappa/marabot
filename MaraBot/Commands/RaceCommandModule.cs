using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace MaraBot.Commands
{
    using Core;
    using Messages;

    public class RaceCommandModule : BaseCommandModule
    {
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }

        [Command("race")]
        [Cooldown(15, 900, CooldownBucketType.User)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx, string presetName)
        {
            var success = LaunchPresetRace(ctx, presetName);

            var emoji = DiscordEmoji.FromName(ctx.Client, await success ? Display.kValidCommandEmoji : Display.kInvalidCommandEmoji);
            await ctx.Message.CreateReactionAsync(emoji);
        }

        public async Task<bool> LaunchPresetRace(CommandContext ctx, string presetName)
        {
            if (!Presets.ContainsKey(presetName))
            {
                await ctx.RespondAsync($"{presetName} is not a a valid preset");
                return false;
            }

            var seed = RandomUtils.GetRandomSeed();
            var preset = Presets[presetName];

            await Display.Race(ctx, preset, seed);
            return true;
        }
    }
}
