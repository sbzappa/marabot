using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;

namespace MaraBot
{
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
            var configTask = ConfigIO.LoadConfigAsync();
            var options = await OptionsIO.LoadOptionsAsync();
            var presets = await PresetIO.LoadPresetsAsync(options);
            var weeklyTask = WeeklyIO.LoadWeeklyAsync(presets, options);
            var responsesTask = EightBallIO.LoadResponsesAsync();

            var config = await configTask;

            var discord = new DiscordShardedClient(new DiscordConfiguration()
            {
                Token = config.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers
            });

            var weekly = await weeklyTask;
            var responses = await responsesTask;

            var services = new ServiceCollection()
                .AddSingleton<IReadOnlyDictionary<string, Preset>>(_ => presets)
                .AddSingleton<IReadOnlyDictionary<string, Option>>(_ => options)
                .AddSingleton(config)
                .AddSingleton(responses)
                .AddSingleton(weekly)
                .AddSingleton<IReadOnlyWeekly>(_ => weekly)
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

            commands.RegisterCommands<Commands.PresetsCommandModule>();
            commands.RegisterCommands<Commands.PresetCommandModule>();
            commands.RegisterCommands<Commands.RaceCommandModule>();
            commands.RegisterCommands<Commands.CustomRaceCommandModule>();
            commands.RegisterCommands<Commands.CreatePresetCommandModule>();
            commands.RegisterCommands<Commands.WeeklyCommandModule>();
            commands.RegisterCommands<Commands.CompletedCommandModule>();
            commands.RegisterCommands<Commands.ForfeitCommandModule>();
            commands.RegisterCommands<Commands.LeaderboardCommandModule>();
            commands.RegisterCommands<Commands.ResetWeeklyCommandModule>();
            commands.RegisterCommands<Commands.SpoilerRoleCommandModule>();
            commands.RegisterCommands<Commands.EightBallCommandModule>();

            foreach(var c in commands) {
                c.Value.CommandExecuted += CommandEvents.OnCommandExecuted;
                c.Value.CommandErrored += CommandEvents.OnCommandErrored;
            }

            await discord.StartAsync();
            await Task.Delay(-1);
        }
    }
}
