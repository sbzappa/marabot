using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;

namespace MaraBot
{
    using System.Threading;
    using Core;
    using IO;

    internal class Program
    {
        public static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var config = await ConfigIO.LoadConfigAsync();
            var options = await OptionsIO.LoadOptionsAsync();
            var weeklyTask = WeeklyIO.LoadWeeklyAsync(options);
            var challengePresetsTask = PresetIO.LoadChallengePresetsAsync(options, config);
            var responsesTask = EightBallIO.LoadResponsesAsync();
            var mysterySettingsTask = MysterySettingsIO.LoadMysterySettingsAsync();
            using var mutexRegistry = new MutexRegistry();

            var discord = new DiscordShardedClient(new DiscordConfiguration()
            {
                Token = config.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContents
            });

            var weekly = await weeklyTask;
            var challenges = await challengePresetsTask;
            var responses = await responsesTask;
            var mysterySettings = await mysterySettingsTask;

            var services = new ServiceCollection()
                .AddSingleton<IReadOnlyDictionary<string, Option>>(_ => options)
                .AddSingleton<IReadOnlyDictionary<string, MysterySetting>>(_ => mysterySettings)
                .AddSingleton(config)
                .AddSingleton(responses)
                .AddSingleton(weekly)
                .AddSingleton<IReadOnlyWeekly>(_ => weekly)
                .AddSingleton<IReadOnlyDictionary<string, Preset>>(_ => challenges)
                .AddSingleton(mutexRegistry)
                .BuildServiceProvider();

            // Test for network before connecting to discord.
            var numberOfTries = 50;
            while (numberOfTries-- > 0)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                    break;

                await Task.Delay(1000);
            }

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                discord.Logger.LogCritical("No network available! Please check your connection.");
                return;
            }

            var commands = await discord.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { config.Prefix },
                Services = services
            });

            commands.RegisterCommands<Commands.RaceCommandModule>();
            commands.RegisterCommands<Commands.CreatePresetCommandModule>();
            commands.RegisterCommands<Commands.WeeklyCommandModule>();
            commands.RegisterCommands<Commands.CompletedCommandModule>();
            commands.RegisterCommands<Commands.ForfeitCommandModule>();
            commands.RegisterCommands<Commands.LeaderboardCommandModule>();
            commands.RegisterCommands<Commands.ResetWeeklyCommandModule>();
            commands.RegisterCommands<Commands.EightBallCommandModule>();
            commands.RegisterCommands<Commands.CheatCommandModule>();
            commands.RegisterCommands<Commands.ChallengeCommandModule>();
            commands.RegisterCommands<Commands.ChallengesCommandModule>();

            foreach(var c in commands) {
                c.Value.CommandExecuted += CommandEvents.OnCommandExecuted;
                c.Value.CommandErrored += CommandEvents.OnCommandErrored;
            }

            await discord.StartAsync();
            await Task.Delay(-1);
        }
    }
}
