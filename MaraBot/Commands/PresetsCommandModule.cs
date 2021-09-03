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

    public class PresetsCommandModule : BaseCommandModule
    {
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }

        [Command("presets")]
        [Description("Get the list of presets.")]
        [Cooldown(2, 900, CooldownBucketType.Channel)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx)
        {
            await Display.Presets(ctx, Presets);
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
