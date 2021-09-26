using System;
using System.Collections.Generic;

namespace MaraBot.Core
{
    /// <summary>
    /// Holds validation logic for presets.
    /// </summary>
    public static class PresetValidation
    {
        /// <summary>
        /// Randomizer version the validation is written for.
        /// </summary>
        public static readonly string VERSION = "1.22";

        /// <summary>
        /// List of all options.
        /// </summary>
        public static Dictionary<string, Option> AllOptions;

        /// <summary>
        /// Prefix for validation error messages.
        /// </summary>
        public static readonly string VALIDATION_ERROR_PREFIX = ":red_square:";

        /// <summary>
        /// Prefix for validation info messages.
        /// </summary>
        public static readonly string VALIDATION_INFO_PREFIX  = ":information_source:";

        /// <summary>
        /// Prefix for validation good messages.
        /// </summary>
        public static readonly string VALIDATION_GOOD_PREFIX  = ":white_check_mark:";

        /// <summary>
        /// Validates if the options variable contains a correct set of randomizer options.
        /// Returns the empty list if no errors are found.
        /// Otherwise, returns a list of strings explaining the errors.
        /// </summary>
        public static List<string> ValidateOptions(Dictionary<string, string> options)
        {
            Dictionary<string, string> optionsCopy = new Dictionary<string, string>(options);
            List<string> errors = new List<string>();

            /*
             * Mode
             */
            Mode mode = Mode.Other;

            // Mode key is required
            if (!optionsCopy.ContainsKey("mode"))
                errors.Add($"{VALIDATION_ERROR_PREFIX} Options must contain what mode is used (e.g. 'mode=rando').");
            // Mode value must be correct
            else if (!AllOptions["mode"].Values.ContainsKey(optionsCopy["mode"]))
                errors.Add($"{VALIDATION_ERROR_PREFIX} '{optionsCopy["mode"]}' is not a known mode.");
            // No mode errors, so we can use the mode for mode-specific validation
            else
                mode = Option.OptionValueToMode(optionsCopy["mode"]);

            /*
             * Other
             */
            foreach (var pair in optionsCopy)
            {
                // Skip if key doesn't exist
                if (!AllOptions.ContainsKey(pair.Key))
                {
                    errors.Add($"{VALIDATION_ERROR_PREFIX} '{pair.Key}' is not a known option.");
                    continue;
                }

                var pairMode = AllOptions[pair.Key].Mode;

                // Skip mode validation, as it's already been done
                if (pairMode == Mode.Mode)
                    continue;

                // All options must be general options or belong to the selected mode
                if (pairMode != mode && pairMode != Mode.General)
                    errors.Add($"{VALIDATION_INFO_PREFIX} '{pair.Key}' belongs to the {Option.ModeToPrettyString(pairMode)} mode, but the selected mode is {Option.ModeToPrettyString(mode)}.");

                // Skip if value doesn't exist
                foreach (var o in AllOptions[pair.Key].Type == OptionType.List
                        ? Option.ParseList(pair.Value) : new List<string> { pair.Value })
                    if (!AllOptions[pair.Key].Values.ContainsKey(o))
                    {
                        errors.Add($"{VALIDATION_ERROR_PREFIX} '{pair.Key}' has no known value '{o}'");
                        optionsCopy.Remove(pair.Key); // Make sure we don't validate on unknown values
                        continue;
                    }
            }

            /*
             * Open
             */
            if (mode == Mode.Open)
            {
                // If 'Oops! All owls' is not selected, the 'But why owls?' option doesn't make sense
                if ((!optionsCopy.ContainsKey("opEnemies") || optionsCopy["opEnemies"] != "oops") && optionsCopy.ContainsKey("oopsAllThis"))
                    errors.Add($"{VALIDATION_INFO_PREFIX} Selecting a different 'Oops! All owls' enemy is useless if you don't have 'Oops! All owls' enabled.");

                // If 'Enemy stat growth' is set to 'None (vanilla), setting a difficulty doesn't make sense
                if (optionsCopy.ContainsKey("opStatGrowth") && optionsCopy["opStatGrowth"] == "vanilla" && optionsCopy.ContainsKey("opDifficulty"))
                    errors.Add($"{VALIDATION_INFO_PREFIX} Selecting a different difficulty is useless if you have vanilla enemy stat growth enabled.");

                // Use sensible mana seed settings
                if ((!optionsCopy.ContainsKey("opGoal") || optionsCopy["opGoal"] != "mtr") && (
                        optionsCopy.ContainsKey("opNumSeeds") ||
                        optionsCopy.ContainsKey("opMinSeeds") ||
                        optionsCopy.ContainsKey("opMaxSeeds")))
                    errors.Add($"{VALIDATION_INFO_PREFIX} Using custom mana seed settings is useless if not enabling the Mana Tree Revival goal.");
                if ((!optionsCopy.ContainsKey("opNumSeeds") || optionsCopy["opNumSeeds"] != "random") &&
                        (optionsCopy.ContainsKey("opMinSeeds") || optionsCopy.ContainsKey("opMaxSeeds")))
                    errors.Add($"{VALIDATION_INFO_PREFIX} Setting the min/max amount of mana seeds required is useless if not having number of seeds required on 'random'.");
                if (optionsCopy.ContainsKey("opMinSeeds") || optionsCopy.ContainsKey("opMaxSeeds"))
                {
                    // Comparison of string numbers works as expected in this case
                    string min = optionsCopy.ContainsKey("opMinSeeds") ? optionsCopy["opMinSeeds"] : "1";
                    string max = optionsCopy.ContainsKey("opMaxSeeds") ? optionsCopy["opMaxSeeds"] : "8";
                    int comp = String.Compare(min, max);
                    if (comp > 0)
                        errors.Add($"{VALIDATION_ERROR_PREFIX} Max amount of mana seeds required is less than min amount.");
                    if (comp == 0)
                        errors.Add($"{VALIDATION_INFO_PREFIX} Min amount of mana seeds required is the same as the max amount. You should instead just set the 'Mana Tree Revival seeds required'.");
                }

                // Don't use the override christmas settings if already on a christmas goal
                if (optionsCopy.ContainsKey("opGoal") && (optionsCopy["opGoal"] == "gift" || optionsCopy["opGoal"] == "reindeer"))
                {
                    if (optionsCopy.ContainsKey("opXmasMaps"))
                        errors.Add($"{VALIDATION_INFO_PREFIX} Setting the Christmas theme explicitly is unnecessary, as the goal is already Christmas-themed.");
                    if (optionsCopy.ContainsKey("opXmasItems"))
                        errors.Add($"{VALIDATION_INFO_PREFIX} Setting the random Christmas drops explicitly is unnecessary, as the goal is already Christmas-themed.");
                }

                // Check for race-safety.
                // Note that we check the actual options,
                // because we need to know when opSpoilerLog
                // is unparseable.
                if (options.ContainsKey("opSpoilerLog") && options["opSpoilerLog"] == "no")
                    errors.Add($"{VALIDATION_GOOD_PREFIX} Options are race-safe.");
                else if (options.ContainsKey("opSpoilerLog"))
                    errors.Add($"{VALIDATION_INFO_PREFIX} Options might not be race-safe, because I don't know if a spoiler log gets generated.");
                else
                    errors.Add($"{VALIDATION_INFO_PREFIX} Options are not race-safe, because a spoiler log gets generated.");
            }

            /*
             * Ancient Cave
             */
            if (mode == Mode.AncientCave)
            {
				if (optionsCopy.ContainsKey("acBoy") && optionsCopy.ContainsKey("acGirl") && optionsCopy.ContainsKey("acSprite"))
					errors.Add($"{VALIDATION_ERROR_PREFIX} Must have at least one character to start with.");
				if (optionsCopy.ContainsKey("acBiomeTypes") && Option.ParseList(optionsCopy["acBiomeTypes"]).Count == 0)
					errors.Add($"{VALIDATION_ERROR_PREFIX} Must have at least one floor type selected.");
			}

            /*
             * Boss Rush
             */
            if (mode == Mode.BossRush)
            {
				if (optionsCopy.ContainsKey("brBoy") && optionsCopy.ContainsKey("brGirl") && optionsCopy.ContainsKey("brSprite"))
					errors.Add($"{VALIDATION_ERROR_PREFIX} Must have at least one character to start with.");
			}

            /*
             * Chaos
             */
            if (mode == Mode.Chaos)
            {
				if (optionsCopy.ContainsKey("chBoy") && optionsCopy.ContainsKey("chGirl") && optionsCopy.ContainsKey("chSprite"))
					errors.Add($"{VALIDATION_ERROR_PREFIX} Must have at least one character to start with.");
			}

            // Add GG message if everything is fine
            if (errors.Count == 0)
                errors.Add($"{VALIDATION_GOOD_PREFIX} All good! :slight_smile:");

            return errors;
        }
    }
}
