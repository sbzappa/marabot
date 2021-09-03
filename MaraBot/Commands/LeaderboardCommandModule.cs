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
        public async Task Execute(CommandContext ctx, int weekNumber = -1)
        {
            var currentWeek = RandomUtils.GetWeekNumber();
            weekNumber = weekNumber == -1 ? currentWeek : weekNumber;

            var weekly = Weekly;
            if (weekNumber != currentWeek)
            {
                weekly = WeeklyIO.LoadWeekly($"weekly.{weekNumber}.json");
            }

            if (weekly.Leaderboard == null || weekly.Leaderboard.Count == 0)
            {
                await ctx.RespondAsync($"No leaderboard available for week {weekNumber}.");

                var invalidEmoji = DiscordEmoji.FromName(ctx.Client, Display.kInvalidCommandEmoji);
                await ctx.Message.CreateReactionAsync(invalidEmoji);

                return;
            }

            await Display.Leaderboard(ctx, weekly, weekNumber == currentWeek);

            var validEmoji = DiscordEmoji.FromName(ctx.Client, Display.kValidCommandEmoji);
            await ctx.Message.CreateReactionAsync(validEmoji);
        }
    }
}
