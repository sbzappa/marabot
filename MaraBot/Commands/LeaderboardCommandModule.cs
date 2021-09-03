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
        [Aliases("lb")]
        [Cooldown(2, 900, CooldownBucketType.Channel)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx, int weekNumber = -1)
        {
            var currentWeek = RandomUtils.GetWeekNumber();
            weekNumber = weekNumber == -1 ? currentWeek : weekNumber;

            var weekly = Weekly;
            if (weekNumber != currentWeek)
            {
                weekly = await WeeklyIO.LoadWeekly($"weekly.{weekNumber}.json");
            }

            if (weekly.Leaderboard == null || weekly.Leaderboard.Count == 0)
            {
                await ctx.RespondAsync($"No leaderboard available for week {weekNumber}.");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            await Display.Leaderboard(ctx, weekly, weekNumber == currentWeek);
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
