using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using MaraBot.Core;

namespace MaraBot.Messages
{
    public static class Display
    {
        const int kMinNumberOfElementsPerColumn = 4;
        const int kMaxNumberOfColumns = 3;
       
        public const string kValidCommandEmoji = ":white_check_mark:";
        public const string kInvalidCommandEmoji = ":no_entry_sign:";
        
        public static async Task Race(CommandContext ctx, Preset preset, string seed)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Requested Seed",
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Generated" 
                },
                Timestamp = DateTime.Now
            };

            var rawOptionsString = string.Join(" ", 
                preset.Options.Select(kvp => $"{kvp.Key}={kvp.Value}")
            );
            
            embed
                .AddField(preset.Name, preset.Description)
                .AddOptionsToEmbed(preset)
                .AddField("Seed", Formatter.BlockCode(seed))
                .AddField("Raw Options", Formatter.BlockCode(rawOptionsString));
            
            var mainBuilder = new DiscordMessageBuilder()
               .AddEmbed(embed);

            await ctx.RespondAsync(mainBuilder);
        }

        public static async Task Presets(CommandContext ctx, IReadOnlyDictionary<string, Preset> presets)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Available Presets",
                Color = DiscordColor.Green
            };

            string presetKeys = String.Empty;
            string presetNames = String.Empty;
            string presetDescriptions = String.Empty;
            
            foreach (var preset in presets)
            {
                presetKeys += $"**{preset.Key}**\n";
                presetNames += $"{preset.Value.Name}\n";
                presetDescriptions += $"{preset.Value.Description}\n"; 
            }
                
            embed
                .AddField("Key", presetKeys, true)
                .AddField("Name", presetNames, true)
                .AddField("Description", presetDescriptions, true);

            await ctx.RespondAsync(embed); 
        }

        public static async Task Preset(CommandContext ctx, Preset preset)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Preset Information",
                Color = DiscordColor.Green
            };

            embed
                .AddField("Name", preset.Name)
                .AddField("Description", preset.Description)
                .AddField("Version", preset.Version)
                .AddField("Author", preset.Author)
                .AddOptionsToEmbed(preset);
            
            await ctx.RespondAsync(embed); 
        }

		private static DiscordEmbedBuilder AddOptionDictionaryToEmbed(this DiscordEmbedBuilder embed, string title, Dictionary<string, string> options)
		{
            int numberOfColumns = Math.Min((int)Math.Ceiling(options.Count / (float) kMinNumberOfElementsPerColumn), kMaxNumberOfColumns);
            int numberOfElementsPerColumn = (int)Math.Ceiling(options.Count / (float) numberOfColumns);
            
            var optionStrings = new string[numberOfColumns];
            for (int i = 0; i < numberOfColumns; ++i)
                optionStrings[i] = String.Empty;
            
            int index = 0;
            int columnIndex = 0;
            foreach (var option in options)
            {
                optionStrings[columnIndex] += $"{option.Key}: _{option.Value}_";

                if ((++index % numberOfElementsPerColumn == 0))
                {
                    ++columnIndex;
                }
                else
                {
                    optionStrings[columnIndex] += "\n";
                }
            }

			embed.AddField(title, optionStrings[0], optionStrings.Length > 1);
            for (int i = 1; i < optionStrings.Length; ++i)
                embed.AddField("\u200B", optionStrings[i], true);

			return embed;
		}

        private static DiscordEmbedBuilder AddOptionsToEmbed(this DiscordEmbedBuilder embed, Preset preset)
        {
			Dictionary<string, string> generalOptions = new Dictionary<string, string>();
			Dictionary<string, string> modeOptions = new Dictionary<string, string>();
			Dictionary<string, string> otherOptions = new Dictionary<string, string>();

			Mode mode = Option.StringToMode(preset.Options["mode"]);
			var options = preset.MakeDisplayable();

			foreach(var option in options)
			{
				if(option.Item1 == Mode.Mode)
					continue;
				if(option.Item1 == mode)
					modeOptions.Add(option.Item2, option.Item3);
				else if(option.Item1 == Mode.General)
					generalOptions.Add(option.Item2, option.Item3);
				else
					otherOptions.Add(option.Item2, option.Item3);
			}

            embed.AddField(Option.ModeToString(Mode.Mode), Option.ModeToString(mode));
			if(generalOptions.Count > 0)
				embed.AddOptionDictionaryToEmbed($"{Option.ModeToString(Mode.General)} Options", generalOptions);
			if(modeOptions.Count > 0)
				embed.AddOptionDictionaryToEmbed($"{Option.ModeToString(mode)} Options", modeOptions);
			if(otherOptions.Count > 0)
				embed.AddOptionDictionaryToEmbed($"{Option.ModeToString(Mode.Other)} Options", otherOptions);

            return embed;
        }
    }
}
