using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using System.Net.NetworkInformation;

namespace MaraBot
{
    using Core;

    internal class Program
    {
        public static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var configTask = ConfigIO.LoadConfig();
            var weeklyTask = WeeklyIO.LoadWeekly();
            var options = await OptionsIO.LoadOptions();
            var presetsTask = PresetIO.LoadPresets(options);

            var config = await configTask;

            var discord = new DiscordShardedClient(new DiscordConfiguration()
            {
                Token = config.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });

            var presets = await presetsTask;
            var weekly = await weeklyTask;

            var services = new ServiceCollection()
                .AddSingleton<IReadOnlyDictionary<string, Preset>>(_ => presets)
                .AddSingleton<IReadOnlyDictionary<string, Option>>(_ => options)
                .AddSingleton<IConfig>(_ => config)
                .AddSingleton(weekly)
                .BuildServiceProvider();

            // Test for internet connection before launching the bot.
            using (var ping = new Ping())
            {
                var maxNumberOfTries = 100;
                var count = 0;

                while (++count < maxNumberOfTries)
                {
                    PingReply reply = null;
                    try
                    {
                        reply = ping.Send("www.google.com");
                    }
                    catch (PingException _)
                    {
                        await Task.Delay(1000);
                    }

                    if (reply != null && reply.Status == IPStatus.Success)
                        break;
                }
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
            commands.RegisterCommands<Commands.LeaderboardCommandModule>();

            await discord.StartAsync();
            await Task.Delay(-1);
        }
    }
}
