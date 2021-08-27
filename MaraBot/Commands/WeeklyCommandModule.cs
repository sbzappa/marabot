using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace MaraBot.Commands
{
    using Core;
    
    public class WeeklyCommandModule : BaseCommandModule
    {
        public IWeeklyConfig Config { private get; set; }
        
        private const string k_ValidCommandEmoji = ":white_check_mark:";
        private const string k_InvalidCommandEmoji = ":no_entry_sign:"; 
        
        [Command("weekly")]
        [Cooldown(2, 900, CooldownBucketType.Channel)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx)
        {
            // TODO...
            // 1. Randomly select a preset among list of weekly presets and generate a race.
            // 2. In a more advanced state, write weekly seed to server log and repost the same
            // race for the current week.
            throw new NotImplementedException();
        }
    }
}