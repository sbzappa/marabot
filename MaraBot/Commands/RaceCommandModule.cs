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
            if (!Presets.ContainsKey(presetName))
            {
                await ctx.RespondAsync($"{presetName} is not a a valid preset");
                
                var invalidEmoji = DiscordEmoji.FromName(ctx.Client, Display.kInvalidCommandEmoji);
                await ctx.Message.CreateReactionAsync(invalidEmoji);
                
                return; 
            }

            var preset = Presets[presetName];
            var seed = RandomUtils.GetRandomSeed();

            await Display.Race(ctx, preset, seed);
            
            var successEmoji = DiscordEmoji.FromName(ctx.Client, Display.kValidCommandEmoji);
            await ctx.Message.CreateReactionAsync(successEmoji);
        }
    }
}