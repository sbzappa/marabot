using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MaraBot.Core;
using MaraBot.Messages;

namespace MaraBot.Commands
{
    public class LeaderboardCommandModule : BaseCommandModule
    {
        public Weekly Weekly { private get; set; }

        [Command("leaderboard")]
        [Cooldown(5, 900, CooldownBucketType.User)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx)
        {
            var weekNumber = RandomUtils.GetWeekNumber();
            if (Weekly.WeekNumber != weekNumber)
            {
                await ctx.RespondAsync("No weekly seed generated for this week. Generate one using the weekly command first.");

                var invalidEmoji = DiscordEmoji.FromName(ctx.Client, Display.kInvalidCommandEmoji);
                await ctx.Message.CreateReactionAsync(invalidEmoji);
            }

            await Display.Leaderboard(ctx, Weekly);

            var validEmoji = DiscordEmoji.FromName(ctx.Client, Display.kValidCommandEmoji);
            await ctx.Message.CreateReactionAsync(validEmoji);
        }
    }
}
