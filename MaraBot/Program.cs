﻿using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;

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
            var config = ConfigIO.LoadConfig();
            var presets = PresetIO.LoadPresets();
			using (StreamReader r = new StreamReader("config/options.json"))
			{
				var json = r.ReadToEnd();
				Option.Options = JsonConvert.DeserializeObject<Dictionary<string, Option>>(json);
			}

            var discord = new DiscordShardedClient(new DiscordConfiguration()
            {
                Token = config.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged     
            });
           
            var services = new ServiceCollection()
                .AddSingleton<IWeeklyConfig>(_ => config)
                .AddSingleton<IReadOnlyDictionary<string, Preset>>(_ => presets)
                .BuildServiceProvider();
            
            var commands = await discord.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" },
                Services = services
            });

            commands.RegisterCommands<Commands.PresetsCommandModule>();
            commands.RegisterCommands<Commands.PresetCommandModule>();
            commands.RegisterCommands<Commands.RaceCommandModule>();
            commands.RegisterCommands<Commands.CreatePresetCommandModule>();
            
            await discord.StartAsync();
            await Task.Delay(-1);
        }   
    }
}
