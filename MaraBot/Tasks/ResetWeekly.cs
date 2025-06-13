using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using MaraBot.Core;
using MaraBot.IO;
using MaraBot.Messages;
using Microsoft.Extensions.Logging;

namespace MaraBot.Tasks
{
    public class ResetWeekly
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public DiscordShardedClient Discord { get; set; }

        /// <summary>
        /// Weekly settings.
        /// </summary>
        public Weekly Weekly { get; set; }
        /// <summary>
        /// Randomizer Options.
        /// </summary>
        public IReadOnlyDictionary<string, Option> Options { private get; set; }
        /// <summary>
        /// Mystery Settings.
        /// </summary>
        public IReadOnlyDictionary<string, MysterySetting> MysterySettings { private get; set; }
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public Config Config { private get; set; }
        /// <summary>
        /// Mutex registry.
        /// </summary>
        public MutexRegistry MutexRegistry { private get; set; }

        public async Task StartAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await TryResetWeekly();
                    await Task.Delay(WeeklyUtils.GetRemainingWeeklyDuration(Weekly.WeekNumber), _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task TryResetWeekly()
        {
            var guilds = Discord.ShardClients
                .Select(kvp => kvp.Value)
                .SelectMany(shard => shard.Guilds)
                .Select(kvp => kvp.Value);

            var previousWeek = Weekly.WeekNumber;
            var currentWeek = WeeklyUtils.GetWeekNumber();
            var backupAndResetWeekly = previousWeek != currentWeek;

            // Make a backup of the previous week's weekly and create a new
            // weekly for the current week.
            if (!backupAndResetWeekly)
                return;

            foreach (var guild in guilds)
            {
                await CommandUtils.RevokeAllRolesAsync(guild, new[]
                {
                    Config.WeeklyCompletedRole,
                    Config.WeeklyForfeitedRole
                });

                await CommandUtils.SendToChannelAsync(
                    guild,
                    Config.WeeklyChannel,
                    await Display.LeaderboardEmbedAsync(guild, Weekly, false));
            }

            // Backup weekly settings to json before overriding.
            await WeeklyIO.StoreWeeklyAsync(Weekly, $"weekly.{previousWeek}.json");

            // Set weekly to blank with a fresh leaderboard.
            Weekly.Load(Weekly.NotSet);
            await WeeklyIO.StoreWeeklyAsync(Weekly);

            // Generate a new race.
            var name = $"Weekly {currentWeek}";

            Weekly.Preset = default;
            Weekly.PresetName = String.Empty;

            try
            {
                (Weekly.Preset, Weekly.Seed, _) = await CommandUtils.GenerateMysteryRaceAsync(default, name, default, MysterySettings, Options, Config);
            }
            catch (InvalidOperationException e)
            {
                Discord.Logger.LogWarning(
                    "Could not generate weekly race.\n" +
                    e.Message);
                return;
            }

            //try
            //{
            //    var (newPreset, newSeed, newValidationHash) = await CommandUtils.GenerateValidationHash(Weekly.Preset, Weekly.Seed, Config, Options, MutexRegistry);
            //    if (newPreset.Equals(Weekly.Preset) && newSeed.Equals(Weekly.Seed))
            //    {
            //        Weekly.ValidationHash = newValidationHash;
            //    }
            //}
            //catch (Exception exception)
            //{
            //    Discord.Logger.LogWarning(
            //        "Could not create a validation hash.\n" +
            //        exception.Message);
            //}

            await WeeklyIO.StoreWeeklyAsync(Weekly);

            var raceEmbed = Display.RaceEmbed(Weekly.Preset, Weekly.Seed, Weekly.ValidationHash);

            foreach (var guild in guilds)
            {
                await CommandUtils.SendToChannelAsync(guild, Config.WeeklyChannel, raceEmbed);
            }
        }

        public void StopAsync()
        {
            _cts.Cancel();
        }

    }
}
