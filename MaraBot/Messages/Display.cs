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
    /// <summary>
    /// Display message to Discord.
    /// </summary>
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

        /// <summary>
        /// Displays race settings.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="preset">Preset used in race.</param>
        /// <param name="seed">Generated seed.</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder RaceEmbed(CommandContext ctx, Preset preset, string seed)
        {
            return RaceEmbed(ctx, preset, seed, DateTime.Now);
        }

        /// <summary>
        /// Displays race settings.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="preset">Preset used in race.</param>
        /// <param name="seed">Generated seed.</param>
        /// <param name="timestamp">Timestamp at which race was generated.</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder RaceEmbed(CommandContext ctx, Preset preset, string seed, DateTime timestamp)
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
                .AddOptions(preset)
                .AddField("Seed", Formatter.BlockCode(seed))
                .AddField("Raw Options", Formatter.BlockCode(rawOptionsString));

            return embed;
        }

        /// <summary>
        /// Displays available presets.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="presets">List of available presets.</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder PresetsEmbed(CommandContext ctx, IReadOnlyDictionary<string, Preset> presets)
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

            return embed;
        }

        /// <summary>
        /// Displays a preset information.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="preset">Preset to display.</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder PresetEmbed(CommandContext ctx, Preset preset)
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
                .AddOptions(preset);

            return embed;
        }

        private static DiscordEmbedBuilder AddOptionDictionary(this DiscordEmbedBuilder embed, string title, Dictionary<string, string> options)
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

        private static DiscordEmbedBuilder AddOptions(this DiscordEmbedBuilder embed, Preset preset)
        {
            Mode mode = Option.OptionValueToMode(preset.Options["mode"]);

            embed.AddField(Option.ModeToPrettyString(Mode.Mode), Option.ModeToPrettyString(mode));

            if(preset.GeneralOptions.Count > 0)
                embed.AddOptionDictionary($"{Option.ModeToPrettyString(Mode.General)} Options", preset.GeneralOptions);
            if(preset.ModeOptions.Count > 0)
                embed.AddOptionDictionary($"{Option.ModeToPrettyString(mode)} Options", preset.ModeOptions);
            if(preset.OtherOptions.Count > 0)
                embed.AddOptionDictionary($"{Option.ModeToPrettyString(Mode.Other)} Options", preset.OtherOptions);

            return embed;
        }

        /// <summary>
        /// Displays the leaderboard for specified weekly.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="weekly">Weekly settings.</param>
        /// <param name="preventSpoilers">Hide potential spoilers.</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder LeaderboardEmbed(CommandContext ctx, Weekly weekly, bool preventSpoilers)
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
                timeStrings += entry.Value.Equals(TimeSpan.MaxValue) ? "DNF" : $"{entry.Value}\n";
            }

            if (preventSpoilers)
            {
                timeStrings = Formatter.Spoiler(timeStrings);
            }

            if (!preventSpoilers)
                embed.AddField("\u200B", emojis, true);

            embed.AddField("User", userStrings, true);
            embed.AddField("Time", timeStrings, true);

            return embed;
        }
    }
}
