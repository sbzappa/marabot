using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MaraBot.Commands
{
    using Core;
    using IO;
    using Messages;

    /// <summary>
    /// Implements the leaderboard command.
    /// This command is used to display the leaderboard for the weekly race.
    /// Optionally a leaderboard for previous weekly can be displayed by
    /// specifying the week number.
    /// </summary>
    public class LeaderboardCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Weekly settings.
        /// </summary>
        public Weekly Weekly { private get; set; }
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public IConfig Config { private get; set; }

        /// <summary>
        /// Executes the leaderboard command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="weekNumber"></param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("leaderboard")]
        [Description("Get the weekly leaderboard.")]
        [Aliases("lb")]
        [Cooldown(2, 900, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequirePermissions(
            Permissions.SendMessages |
            Permissions.AddReactions |
            Permissions.AccessChannels)]
        public async Task Execute(CommandContext ctx, int weekNumber = -1)
        {
            var currentWeek = RandomUtils.GetWeekNumber();
            weekNumber = weekNumber == -1 ? currentWeek : weekNumber;

            var weekly = Weekly;
            if (weekNumber != currentWeek)
            {
                weekly = await WeeklyIO.LoadWeeklyAsync($"weekly.{weekNumber}.json");
            }

            if (weekly.Leaderboard == null || weekly.Leaderboard.Count == 0)
            {
                await ctx.RespondAsync($"No leaderboard available for week {weekNumber}.");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            if (weekNumber == currentWeek)
            {
                var spoilerChannel = ctx.Guild.Channels
                    .FirstOrDefault(channel => Config.WeeklySpoilerChannel.Equals(channel.Value.Name));

                if (spoilerChannel.Value == null)
                {
                    await ctx.RespondAsync(
                        "No spoiler channel set.\n" +
                        "This shouldn't happen! Please contact your friendly neighbourhood developers!");
                }
                else if (ctx.Channel.Equals(spoilerChannel.Value))
                {
                    await ctx.RespondAsync(Display.LeaderboardEmbed(ctx, weekly, false));
                    await CommandUtils.SendSuccessReaction(ctx);
                }
                else
                {
                    await ctx.RespondAsync("This week's leaderboard can only be displayed on the spoiler channel!");
                    await CommandUtils.SendFailReaction(ctx);
                }
            }
            else
            {
                await ctx.RespondAsync(Display.LeaderboardEmbed(ctx, weekly, false));
                await CommandUtils.SendSuccessReaction(ctx);
            }
        }
    }
}
