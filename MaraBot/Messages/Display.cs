using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
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
        public const string kRaceValidatedEmoji = ":regional_indicator_v:";

        public static readonly string[] kRankingEmoijs = new []
        {
            ":first_place:",
            ":second_place:",
            ":third_place:"
        };

        /// <summary>
        /// Displays race settings.
        /// </summary>
        /// <param name="preset">Preset used in race.</param>
        /// <param name="seed">Generated seed.</param>
        /// <param name="validationHash">Validation hash (optional).</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder RaceEmbed(in Preset preset, string seed, string validationHash = default)
        {
            return RaceEmbed(preset, seed, validationHash, DateTime.Now);
        }

        /// <summary>
        /// Displays race settings.
        /// </summary>
        /// <param name="preset">Preset used in race.</param>
        /// <param name="seed">Generated seed.</param>
        /// <param name="validationHash"></param>
        /// <param name="timestamp">Timestamp at which race was generated.</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder RaceEmbed(in Preset preset, string seed, string validationHash, DateTime timestamp)
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

            if (String.IsNullOrEmpty(rawOptionsString))
                rawOptionsString = "\u200B";

            if (!String.IsNullOrEmpty(preset.Name))
            {
                embed.AddField(preset.Name, String.IsNullOrEmpty(preset.Description) ? "\u200B" : preset.Description);
            }

            embed.AddField("Author", preset.Author);

            if (preset.Difficulty != 0)
                embed.AddField("Difficulty", CommandUtils.IntegerToRabiteEmojis(preset.Difficulty));

            embed
                .AddOptions(preset)
                .AddField("Seed", Formatter.BlockCode(seed));

            if (!String.IsNullOrEmpty(validationHash))
            {
                embed.AddField("Validation Hash", Formatter.BlockCode(validationHash));
            }

            embed.AddField("Raw Options", Formatter.BlockCode(rawOptionsString));

            return embed;
        }

        /// <summary>
        /// Displays available presets.
        /// </summary>
        /// <param name="presets">List of available presets.</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder PresetsEmbed(IReadOnlyDictionary<string, Preset> presets)
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

        public static DiscordMessageBuilder PresetsFlipBook(
            Preset preset,
            int index,
            int count,
            string buttonFirstID,
            string buttonPreviousID,
            string buttonNextID,
            string buttonLastID)
        {
            return new DiscordMessageBuilder()
                .AddPresetsFlipBook(
                    preset, index, count,
                    buttonFirstID, buttonPreviousID, buttonNextID, buttonLastID);
        }

        public static T AddPresetsFlipBook<T>(
            this T builder,
            Preset preset,
            int index,
            int count,
            string buttonFirstID,
            string buttonPreviousID,
            string buttonNextID,
            string buttonLastID)
            where T : BaseDiscordMessageBuilder<T>
        {
            builder
                .AddEmbed(Display.PresetEmbed(preset))
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Success, buttonFirstID, "FIRST", index == 0),
                    new DiscordButtonComponent(ButtonStyle.Success, buttonPreviousID, "PREVIOUS", index == 0),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "dont_care", $"{index + 1}/{count}", true),
                    new DiscordButtonComponent(ButtonStyle.Success, buttonNextID, "NEXT", index == count - 1),
                    new DiscordButtonComponent(ButtonStyle.Success, buttonLastID, "LAST", index == count - 1)
                });
            return builder;
        }

        /// <summary>
        /// Displays a preset information.
        /// </summary>
        /// <param name="preset">Preset to display.</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder PresetEmbed(in Preset preset)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Preset Information",
                Color = DiscordColor.Green
            };

            embed.AddField("Preset", preset.Name);

            if (!String.IsNullOrEmpty(preset.Description))
                embed.AddField("Description", preset.Description);

            if (!String.IsNullOrEmpty(preset.Author))
                embed.AddField("Author", preset.Author);

            if (preset.Difficulty != 0)
                embed.AddField("Difficulty", CommandUtils.IntegerToRabiteEmojis(preset.Difficulty));

            embed.AddOptions(preset);

            return embed;
        }

        private static DiscordEmbedBuilder AddOptionDictionary(this DiscordEmbedBuilder embed, string title, IReadOnlyDictionary<string, string> options)
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

            embed.AddField(title, optionStrings[0], true);

            columnIndex = 1;
            for (; columnIndex < optionStrings.Length; ++columnIndex)
                embed.AddField("\u200B", optionStrings[columnIndex], true);
            for (; columnIndex < kMaxNumberOfColumns; ++columnIndex)
                embed.AddField("\u200B", "\u200B", true);

            return embed;
        }

        private static DiscordEmbedBuilder AddOptions(this DiscordEmbedBuilder embed, Preset preset)
        {
            GameMode gameMode = preset.Options.ContainsKey("mode") ? Option.OptionValueToGameMode(preset.Options["mode"]) : GameMode.Rando;

            embed.AddField(Option.CategoryToPrettyString(Category.Mode), Option.GameModeToPrettyString(gameMode));

            if (gameMode == GameMode.Open && !String.IsNullOrEmpty(preset.OpenModeGoal))
                embed.AddField("Goal", preset.OpenModeGoal);

            string version = preset.Options.ContainsKey("version") ? preset.Options["version"] : "n/a";
            embed.AddField("Version", version);

            if (preset.GeneralOptions.Count > 0)
                embed.AddOptionDictionary($"{Option.CategoryToPrettyString(Category.General)} Options", preset.GeneralOptions);
            if (preset.ModeOptions.Count > 0)
                embed.AddOptionDictionary($"{Option.GameModeToPrettyString(gameMode)} Options", preset.ModeOptions);
            if (preset.OtherOptions.Count > 0)
                embed.AddOptionDictionary($"{Option.CategoryToPrettyString(Category.Other)} Options", preset.OtherOptions);

            return embed;
        }

        /// <summary>
        /// Displays the leaderboard for specified weekly.
        /// </summary>
        /// <param name="weekly">Weekly settings.</param>
        /// <param name="preventSpoilers">Hide potential spoilers.</param>
        /// <returns>Returns an embed builder.</returns>
        public static async Task<DiscordEmbedBuilder> LeaderboardEmbedAsync(DiscordGuild guild, IReadOnlyWeekly weekly, bool preventSpoilers)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Leaderboard",
                Color = DiscordColor.Yellow
            };

            embed
                .AddField("Weekly Seed", $"Week #{weekly.WeekNumber}" + (String.IsNullOrEmpty(weekly.PresetName) ? "" :  $"- {weekly.PresetName}"));

            IEnumerable<KeyValuePair<string, TimeSpan>> leaderboard = weekly.Leaderboard;

            // To avoid giving away any ranking, avoid sorting the leaderboard when preventing spoilers.
            if (leaderboard != null && !preventSpoilers)
            {
                leaderboard = leaderboard
                    .OrderBy(kvp => kvp.Value);
            }

            var rankStrings = String.Empty;
            var userStrings = String.Empty;
            var timeStrings = String.Empty;

            var rank = 0;
            var rankTreshold = TimeSpan.MinValue;

            if (leaderboard != null)
            {
                var allMembers = await guild.GetAllMembersAsync();

                foreach (var entry in leaderboard)
                {
                    if (entry.Value > rankTreshold)
                    {
                        ++rank;
                        rankTreshold = entry.Value;
                    }
                    rankStrings += $"{(rank <= 3 ? kRankingEmoijs[rank - 1] : CommandUtils.IntegerToOrdinal(rank))}\n";

                    if (CommandUtils.UsernameToUserMention(allMembers, entry.Key, out var userMention))
                        userStrings += $"{userMention}\n";
                    else
                        userStrings += $"{entry.Key}\n";

                    timeStrings += $"{(entry.Value.Equals(TimeSpan.MaxValue) ? "DNF" : entry.Value.ToString())}\n";
                }

                if (preventSpoilers)
                {
                    timeStrings = Formatter.Spoiler(timeStrings);
                }

                if (!preventSpoilers)
                    embed.AddField("\u200B", rankStrings, true);

                embed.AddField("User", userStrings, true);
                embed.AddField("Time", timeStrings, true);
            }

            return embed;
        }
    }
}
