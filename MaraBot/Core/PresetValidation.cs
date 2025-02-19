using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MaraBot.Core
{
    /// <summary>
    /// Holds validation logic for presets.
    /// </summary>
    public static class PresetValidation
    {
        /// <summary>
        /// Prefix for validation error messages.
        /// </summary>
        public const string kValidationErrorPrefix = ":red_square:";

        /// <summary>
        /// Prefix for validation info messages.
        /// </summary>
        public const string kValidationInfoPrefix  = ":information_source:";

        /// <summary>
        /// Prefix for validation good messages.
        /// </summary>
        public const string kValidationGoodPrefix  = ":white_check_mark:";

        /// <summary>
        /// Validates if the options variable contains a correct set of randomizer options.
        /// Returns the empty list if no errors are found.
        /// Otherwise, returns a list of strings explaining the errors.
        /// </summary>
        public static List<string> ValidateOptions(in Preset preset, IReadOnlyDictionary<string, Option> allOptions)
        {
            Dictionary<string, string> optionsCopy = new Dictionary<string, string>(preset.Options);
            List<string> errors = new List<string>();

            /*
             * Version
             */

            // Version key is required
            if (!optionsCopy.ContainsKey("version"))
                errors.Add($"{kValidationErrorPrefix} Options must contain the randomizer version used (e.g. 'version=1.23').");
            else
            {
                var versionRegex = new Regex("^[0-9].[0-9][0-9]$");
                var versionString = optionsCopy["version"];

                if (!versionRegex.IsMatch(versionString))
                {
                    errors.Add($"{kValidationErrorPrefix} '{versionString}' is not a recognized version number.");
                }

                // Remove version key. Not necessary in further validation.
                optionsCopy.Remove("version");
            }

            /*
             * Category
             */
            GameMode gameMode = GameMode.Rando;

            // Mode key is required
            if (!optionsCopy.ContainsKey("mode"))
                errors.Add($"{kValidationErrorPrefix} Options must contain what mode is used (e.g. 'mode=rando').");
            // Mode value must be correct
            else if (!(allOptions["mode"] as EnumOption).Values.ContainsKey(optionsCopy["mode"]))
                errors.Add($"{kValidationErrorPrefix} '{optionsCopy["mode"]}' is not a known game mode.");
            // No mode errors, so we can use the mode for mode-specific validation
            else
                gameMode = Option.OptionValueToGameMode(optionsCopy["mode"]);

            /*
             * Other
             */
            foreach (var pair in optionsCopy)
            {
                // Skip if key doesn't exist
                if (!allOptions.ContainsKey(pair.Key))
                {
                    errors.Add($"{kValidationErrorPrefix} '{pair.Key}' is not a known option.");
                    continue;
                }

                var option = allOptions[pair.Key];
                var pairMode = option.Category;

                // Skip mode validation, as it's already been done
                if (pairMode == Category.Mode)
                    continue;

                // All options must be general options or belong to the selected mode
                if (pairMode != (Category)gameMode && pairMode != Category.General)
                    errors.Add($"{kValidationInfoPrefix} '{pair.Key}' belongs to the {Option.CategoryToPrettyString(pairMode)} mode, but the selected game mode is {Option.GameModeToPrettyString(gameMode)}.");

                // General enum validation
                if (option is EnumOption)
                {
                    var enumOption = option as EnumOption;

                    // Skip if value doesn't exist
                    foreach (var o in option.List ? Option.ParseList(pair.Value) : new List<string> { pair.Value })
                        if (!enumOption.Values.ContainsKey(o))
                        {
                            errors.Add($"{kValidationErrorPrefix} '{pair.Key}' has no known value '{o}'");
                            optionsCopy.Remove(pair.Key); // Make sure we don't validate on unknown values
                            continue; // Check if there are more unknown values, so that we can show those errors too
                        }
                }

                // General numeric validation
                if (option is NumericOption)
                {
                    var numericOption = option as NumericOption;

                    // Skip if value is not within bounds
                    foreach (var o in option.List ? Option.ParseList(pair.Value) : new List<string> { pair.Value })
                    {
                        bool success = double.TryParse(o, out var v);
                        if(!success)
                        {
                            errors.Add($"{kValidationErrorPrefix} '{pair.Key}' has non-numeric value '{o}'");
                            optionsCopy.Remove(pair.Key); // Make sure we don't validate on invalid values
                            continue; // Check if there are more invalid values, so that we can show those errors too
                        }
                        if (v < numericOption.Min || v > numericOption.Max)
                        {
                            errors.Add($"{kValidationErrorPrefix} '{pair.Key}' has out-of-bounds value '{o}' (must be between {numericOption.Min} and {numericOption.Max})");
                            optionsCopy.Remove(pair.Key); // Make sure we don't validate on invalid values
                            continue; // Check if there are more invalid values, so that we can show those errors too
                        }
                        // TODO: precision checks?
                    }
                }
            }

            /*
             * Open
             */
            if (gameMode == GameMode.Open)
            {
                // If 'Oops! All owls' is not selected, the 'But why owls?' option doesn't make sense
                if ((!optionsCopy.ContainsKey("opEnemies") || optionsCopy["opEnemies"] != "oops") && optionsCopy.ContainsKey("oopsAllThis"))
                    errors.Add($"{kValidationInfoPrefix} Selecting a different 'Oops! All owls' enemy is useless if you don't have 'Oops! All owls' enabled.");

                // If 'Enemy stat growth' is set to 'None (vanilla)', setting a difficulty doesn't make sense
                if (optionsCopy.ContainsKey("opStatGrowth") && optionsCopy["opStatGrowth"] == "vanilla" && optionsCopy.ContainsKey("opDifficulty"))
                    errors.Add($"{kValidationInfoPrefix} Selecting a different difficulty is useless if you have vanilla enemy stat growth enabled.");

                // If 'Enemy stat growth' is not set to 'No Future', setting a "No Future" level doesn't make sense
                if ((!optionsCopy.ContainsKey("opStatGrowth") || optionsCopy["opStatGrowth"] != "nofuture") && optionsCopy.ContainsKey("opNoFutureLevel"))
                    errors.Add($"{kValidationInfoPrefix} Selecting a \"No Future\" level is useless if you don't have \"No Future\" enemy stat growth");

                // If 'Goal' is not set to 'Gift exchange', the 'Xmas gifts' option doesn't make sense
                if ((!optionsCopy.ContainsKey("opGoal") || optionsCopy["opGoal"] != "gift") && optionsCopy.ContainsKey("opXmasGifts"))
                    errors.Add($"{kValidationInfoPrefix} Selecting the amount of gifts needed is useless if you don't have the 'Gift exchange' goal enabled.");

                // Use sensible mana seed settings
                if ((!optionsCopy.ContainsKey("opGoal") || optionsCopy["opGoal"] != "mtr") && (
                        optionsCopy.ContainsKey("opNumSeeds") ||
                        optionsCopy.ContainsKey("opMinSeeds") ||
                        optionsCopy.ContainsKey("opMaxSeeds")))
                    errors.Add($"{kValidationInfoPrefix} Using custom mana seed settings is useless if not enabling the Mana Tree Revival goal.");
                if ((!optionsCopy.ContainsKey("opNumSeeds") || optionsCopy["opNumSeeds"] != "random") &&
                        (optionsCopy.ContainsKey("opMinSeeds") || optionsCopy.ContainsKey("opMaxSeeds")))
                    errors.Add($"{kValidationInfoPrefix} Setting the min/max amount of mana seeds required is useless if not having number of seeds required on 'random'.");
                if (optionsCopy.ContainsKey("opMinSeeds") || optionsCopy.ContainsKey("opMaxSeeds"))
                {
                    // Comparison of string numbers works as expected in this case
                    string min = optionsCopy.ContainsKey("opMinSeeds") ? optionsCopy["opMinSeeds"] : "1";
                    string max = optionsCopy.ContainsKey("opMaxSeeds") ? optionsCopy["opMaxSeeds"] : "8";
                    int comp = String.Compare(min, max);
                    if (comp > 0)
                        errors.Add($"{kValidationErrorPrefix} Max amount of mana seeds required is less than min amount.");
                    if (comp == 0)
                        errors.Add($"{kValidationInfoPrefix} Min amount of mana seeds required is the same as the max amount. You should instead just set the 'Mana Tree Revival seeds required'.");
                }

                // Don't use the override christmas settings if already on a christmas goal
                if (optionsCopy.ContainsKey("opGoal") && (optionsCopy["opGoal"] == "gift" || optionsCopy["opGoal"] == "reindeer"))
                {
                    if (optionsCopy.ContainsKey("opXmasMaps"))
                        errors.Add($"{kValidationInfoPrefix} Setting the Christmas theme explicitly is unnecessary, as the goal is already Christmas-themed.");
                    if (optionsCopy.ContainsKey("opXmasItems"))
                        errors.Add($"{kValidationInfoPrefix} Setting the random Christmas drops explicitly is unnecessary, as the goal is already Christmas-themed.");
                }

                // Check for race-safety.
                if (preset.Options.ContainsKey("opSpoilerLog") && preset.Options["opSpoilerLog"] == "no" ||
                    preset.Options.ContainsKey("raceMode") && preset.Options["raceMode"] == "yes")
                    errors.Add($"{kValidationGoodPrefix} Options are race-safe.");
                else
                    errors.Add($"{kValidationInfoPrefix} Options are not race-safe, because a spoiler log gets generated.");
            }

            /*
             * Ancient Cave
             */
            if (gameMode == GameMode.AncientCave)
            {
                if (optionsCopy.ContainsKey("acBoy") && optionsCopy.ContainsKey("acGirl") && optionsCopy.ContainsKey("acSprite"))
                    errors.Add($"{kValidationErrorPrefix} Must have at least one character to start with.");
                if (optionsCopy.ContainsKey("acBiomeTypes") && Option.ParseList(optionsCopy["acBiomeTypes"]).Count == 0)
                    errors.Add($"{kValidationErrorPrefix} Must have at least one floor type selected.");
            }

            /*
             * Boss Rush
             */
            if (gameMode == GameMode.BossRush)
            {
                if (optionsCopy.ContainsKey("brBoy") && optionsCopy.ContainsKey("brGirl") && optionsCopy.ContainsKey("brSprite"))
                    errors.Add($"{kValidationErrorPrefix} Must have at least one character to start with.");
            }

            /*
             * Chaos
             */
            if (gameMode == GameMode.Chaos)
            {
                if (optionsCopy.ContainsKey("chBoy") && optionsCopy.ContainsKey("chGirl") && optionsCopy.ContainsKey("chSprite"))
                    errors.Add($"{kValidationErrorPrefix} Must have at least one character to start with.");
            }

            // Add GG message if everything is fine
            if (errors.Count == 0)
                errors.Add($"{kValidationGoodPrefix} All good! :slight_smile:");

            return errors;
        }

        /// <summary>
        /// Generates a validation message from validated preset options.
        /// </summary>
        public static string GenerateValidationMessage(in Preset preset, IReadOnlyDictionary<string, Option> allOptions)
        {
            var validationMessage = "";

            List<string> errors = ValidateOptions(preset, allOptions);
            foreach (var e in errors)
                validationMessage += $"> {e}\n";

            return validationMessage;
        }
    }
}
