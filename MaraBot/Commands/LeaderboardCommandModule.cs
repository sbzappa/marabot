using System.Collections.Generic;
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
        public IReadOnlyWeekly Weekly { private get; set; }
        /// <summary>
        /// Presets settings.
        /// </summary>
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }
        /// <summary>
        /// Randomizer Options.
        /// </summary>
        public IReadOnlyDictionary<string, Option> Options { private get; set; }
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public Config Config { private get; set; }

        /// <summary>
        /// Executes the leaderboard command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="weekNumber"></param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("leaderboard")]
        [Description("Get the weekly leaderboard.")]
        [Aliases("lb")]
        [Cooldown(15, 600, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequireBotPermissions(
            Permissions.SendMessages |
            Permissions.AccessChannels)]
        public async Task Execute(CommandContext ctx, int weekNumber = -1)
        {
            var currentWeek = RandomUtils.GetWeekNumber();
            weekNumber = weekNumber == -1 ? currentWeek : weekNumber;

            var weekly = Weekly;
            if (weekNumber != currentWeek)
            {
                weekly = await WeeklyIO.LoadWeeklyAsync(Presets, Options, $"weekly.{weekNumber}.json");
            }

            if (weekly.Leaderboard == null || weekly.Leaderboard.Count == 0)
            {
                await ctx.RespondAsync($"No leaderboard available for week {weekNumber}.");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            if (weekNumber == currentWeek)
            {
                var success = await CommandUtils.ChannelExistsInGuild(ctx, Config.WeeklySpoilerChannel);
                if (!success)
                {
                    await ctx.RespondAsync("This week's leaderboard can only be displayed on the spoiler channel!");
                    await CommandUtils.SendFailReaction(ctx);
                    return;
                }
            }

            await ctx.RespondAsync(await Display.LeaderboardEmbedAsync(ctx.Guild, weekly, false));
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}
