using System;
using System.Collections.Generic;
using System.IO;
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
    public class ResetChallenge
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public DiscordShardedClient Discord { get; set; }

        public IReadOnlyDictionary<string, Option> Options { get; set; }
        public IReadOnlyDictionary<string, Preset> Challenges { get; set; }
        public Config Config { get; set; }
        public MutexRegistry MutexRegistry { get; set; }

        private static readonly string k_ChallengeTimeStamp = "$HOME/challenge.stamp";

        public async Task StartAsync()
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            var challengeTimeStamp = k_ChallengeTimeStamp.Replace("$HOME", homeFolder);

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var timeStamp = DateTime.UnixEpoch;

                    if (File.Exists(challengeTimeStamp))
                        timeStamp = File.GetLastWriteTime(challengeTimeStamp);

                    if (await TryResetChallenge(timeStamp))
                    {
                        File.Create(challengeTimeStamp);
                        timeStamp = DateTime.UtcNow;
                    }

                    var duration = WeeklyUtils.GetRemainingChallengeDuration(timeStamp);

                    // At most check remaining duration every day.
                    if (duration > TimeSpan.FromDays(1))
                        duration = TimeSpan.FromDays(1);

                    if (duration > TimeSpan.Zero)
                        await Task.Delay(duration, _cts.Token);
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

        private async Task<bool> TryResetChallenge(DateTime timeStamp)
        {
            if (!WeeklyUtils.ShouldResetChallenge(timeStamp))
                return false;

            var guilds = Discord.ShardClients
                .Select(kvp => kvp.Value)
                .SelectMany(shard => shard.Guilds)
                .Select(kvp => kvp.Value);

            var index = WeeklyUtils.GetRandomIndex(0, Challenges.Count);
            var preset = Challenges.Values.ElementAt(index);
            var seed = WeeklyUtils.GetRandomSeed();

            var validationHash = String.Empty;
            try
            {
                var (newPreset, newSeed, newValidationHash) = await CommandUtils.GenerateValidationHash(preset, seed, Config, Options, MutexRegistry);
                if (newPreset.Equals(preset) && newSeed.Equals(seed))
                {
                    validationHash = newValidationHash;
                }
            }
            catch (Exception exception)
            {
                Discord.Logger.LogWarning(
                    "Could not create a validation hash.\n" +
                    exception.Message);
            }


            var raceEmbed = Display.RaceEmbed(preset, seed, validationHash);

            foreach (var guild in guilds)
            {
                await CommandUtils.SendToChannelAsync(guild, Config.ChallengeChannel, raceEmbed);
            }

            return true;
        }

        public void StopAsync()
        {
            _cts.Cancel();
        }

    }
}
