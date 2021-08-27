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
        [Cooldown(2, 900, CooldownBucketType.User)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx)
        {
            await Display.Presets(ctx, Presets);
            
            var successEmoji = DiscordEmoji.FromName(ctx.Client, Display.kValidCommandEmoji);
            await ctx.Message.CreateReactionAsync(successEmoji);
        }
    }
}