using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace MaraBot.Commands
{
    using Core;
    using Messages;

    public class PresetCommandModule : BaseCommandModule
    {
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }

        [Command("preset")]
        [Description("Get the info on a given preset.")]
        [Cooldown(10, 600, CooldownBucketType.User)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx, string presetName)
        {
            if (!Presets.ContainsKey(presetName))
            {
                await ctx.RespondAsync($"{presetName} is not a a valid preset");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            await Display.Preset(ctx, Presets[presetName]);
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
