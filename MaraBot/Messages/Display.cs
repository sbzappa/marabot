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

        public static readonly string[] kRankingEmoijs = new []
        {
            ":first_place:",
            ":second_place:",
            ":third_place:"
        };

        public static Task Race(CommandContext ctx, Preset preset, string seed)
        {
            return Race(ctx, preset, seed, DateTime.Now);
        }

        public static Task Race(CommandContext ctx, Preset preset, string seed, DateTime timestamp)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Requested Seed",
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Generated"
                },
                Timestamp = timestamp
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

            return ctx.RespondAsync(mainBuilder);
        }

        public static Task Presets(CommandContext ctx, IReadOnlyDictionary<string, Preset> presets)
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

            return ctx.RespondAsync(embed);
        }

        public static Task Preset(CommandContext ctx, Preset preset)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Preset Information",
                Color = DiscordColor.Green
            };

            embed
                .AddField("Preset", preset.Name)
                .AddField("Description", preset.Description)
                .AddField("Version", preset.Version)
                .AddField("Author", preset.Author)
                .AddOptionsToEmbed(preset);

            return ctx.RespondAsync(embed);
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
                    ++columnIndex;
                else
                    optionStrings[columnIndex] += "\n";
            }

            embed.AddField(title, optionStrings[0], optionStrings.Length > 1);
            for (int i = 1; i < optionStrings.Length; ++i)
                embed.AddField("\u200B", optionStrings[i], true);

            return embed;
        }

        private static DiscordEmbedBuilder AddOptionsToEmbed(this DiscordEmbedBuilder embed, Preset preset)
        {
            Mode mode = Option.OptionValueToMode(preset.Options["mode"]);

            embed.AddField(Option.ModeToPrettyString(Mode.Mode), Option.ModeToPrettyString(mode));

            if(preset.GeneralOptions.Count > 0)
                embed.AddOptionDictionaryToEmbed($"{Option.ModeToPrettyString(Mode.General)} Options", preset.GeneralOptions);
            if(preset.ModeOptions.Count > 0)
                embed.AddOptionDictionaryToEmbed($"{Option.ModeToPrettyString(mode)} Options", preset.ModeOptions);
            if(preset.OtherOptions.Count > 0)
                embed.AddOptionDictionaryToEmbed($"{Option.ModeToPrettyString(Mode.Other)} Options", preset.OtherOptions);

            return embed;
        }


        public static Task Leaderboard(CommandContext ctx, Weekly weekly, bool preventSpoilers)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Leaderboard",
                Color = DiscordColor.Yellow
            };

            embed
                .AddField("Weekly Seed", $"week #{weekly.WeekNumber}");

            IEnumerable<KeyValuePair<string, TimeSpan>> leaderboard = weekly.Leaderboard;

            // To avoid giving away any ranking, avoid sorting the leaderboard when preventing spoilers.
            if (!preventSpoilers)
            {
                leaderboard = leaderboard
                    .OrderBy(kvp => kvp.Value);
            }

            var emojis = String.Join("\n",
                kRankingEmoijs.Take(Math.Min(kRankingEmoijs.Length, leaderboard.Count()))
            );
            var userStrings = String.Empty;
            var timeStrings = String.Empty;

            foreach (var entry in leaderboard)
            {
                userStrings += $"{entry.Key}\n";
                timeStrings += $"{entry.Value}\n";
            }

            if (preventSpoilers)
            {
                timeStrings = Formatter.Spoiler(timeStrings);
            }

            if (!preventSpoilers)
                embed.AddField("\u200B", emojis, true);

            embed.AddField("User", userStrings, true);
            embed.AddField("Time", timeStrings, true);

            return ctx.RespondAsync(embed);
        }
    }
}
