using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MaraBot.Core
{
	/**
	 * Holds information on an option flags preset.
	 */
    public class Preset
    {
		/**
		 * Name of the preset.
		 */
        public string Name;

		/**
		 * Version of the randomizer the preset is made for.
		 */
        public string Version;

		/**
		 * List of option flags as used in the randomizer options string.
		 */
        [JsonConverter(typeof(OptionsConverter))] public Dictionary<string, string> Options;

		/**
		 * Author of the preset.
		 */
        public string Author;

		/**
		 * Description of the preset.
		 */
        public string Description;

		public static Mode StringToMode(string modeString)
		{
			switch(modeString)
			{
				case "rando"      : return Mode.Rando      ;
				case "open"       : return Mode.Open       ;
				case "ancientcave": return Mode.AncientCave;
				case "bossrush"   : return Mode.BossRush   ;
				case "chaos"      : return Mode.Chaos      ;
				default           : return Mode.Other      ;
			}
		}

		/**
		 * Validates if the Options variable contains a correct set of randomizer options.
		 * Returns the empty string if no errors are found.
		 * Otherwise, returns a string explaining the error.
		 */
		public string ValidateOptions()
		{
			/*
			 * Mode
			 */
			// Mode key is required
			if(!Options.ContainsKey("mode"))
				return "Options must contain what mode is used (e.g. 'mode=rando')";

			// Mode value must be correct
			if(!Option.Options["mode"].Values.ContainsKey(Options["mode"]))
				return $"'{Options["mode"]}' is not a valid mode";

			Mode mode = Option.StringToMode(Options["mode"]);

			/*
			 * Open
			 */
			if(mode == Mode.Open)
			{
				// If 'Oops! All owls' is not selected, the 'But why owls?' option doesn't make sense
				if((!Options.ContainsKey("opEnemies") || Options["opEnemies"] != "oops") && Options.ContainsKey("oopsAllThis"))
					return "Selecting a different 'Oops! All owls' enemy is useless if you don't have 'Oops! All owls' enabled. This is probably a mistake.";

				// If 'Enemy stat growth' is set to 'None (vanilla), setting a difficulty doesn't make sense
				if(Options.ContainsKey("opStatGrowth") && Options["opStatGrowth"] == "vanilla" && Options.ContainsKey("opDifficulty"))
					return "Selecting a different difficulty is useless if you have vanilla enemy stat growth enabled. This is probably a mistake.";

				// Use sensible mana seed settings
				if((!Options.ContainsKey("opGoal") || Options["opGoal"] != "mtr") && (
						Options.ContainsKey("opNumSeeds") ||
						Options.ContainsKey("opMinSeeds") ||
						Options.ContainsKey("opMaxSeeds")))
					return "Using custom mana seed settings is useless if not enabling the Mana Tree Revival goal. This is probably a mistake.";
				if(!Options.ContainsKey("opNumSeeds") && (Options.ContainsKey("opMinSeeds") || Options.ContainsKey("opMaxSeeds")))
					return "Restricting the amount of mana seeds needed is useless if not using a random number of seeds needed. This is probably a mistake.";
				if(Options.ContainsKey("opMinSeeds") && Options.ContainsKey("opMaxSeeds"))
				{
					// Comparison of string numbers works as expected in this case
					int comp = String.Compare(Options["opMinSeeds"], Options["opMaxSeeds"]);
					if(comp > 0)
						return "Invalid 'mana seeds required' range.";
					if(comp == 0)
						return "'Mana seeds required' range is trivial. You should just set the 'Mana Tree Revival seeds required'.";
				}

				// Don't use the override christmas settings if already on a christmas goal
				if(Options.ContainsKey("opGoal") && (Options["opGoal"] == "gift" || Options["opGoal"] == "reindeer"))
				{
					if(Options.ContainsKey("opXmasMaps"))
						return "Setting the Christmas theme explicitly is unnecessary, as the goal is already Christmas-themed. This is probably a mistake.";
					if(Options.ContainsKey("opXmasItems"))
						return "Setting the random Christmas drops explicitly is unnecessary, as the goal is already Christmas-themed. This is probably a mistake.";
				}
			}

			/*
			 * Other
			 */
			foreach(var pair in Options)
			{
				// Skip if key doesn't exist
				if(!Option.Options.ContainsKey(pair.Key))
					continue;

				var pairMode = Option.Options[pair.Key].Mode;

				// Skip mode validation, as it's already been done
				if(pairMode == Mode.Mode)
					continue;

				// All options must be general options or belong to the selected mode
				if(pairMode != mode && pairMode != Mode.General)
					return $"'{pair.Key}' belongs to the {pairMode.ToString()} mode, but the selected mode is {mode.ToString()}. This is probably a mistake.";

				// Skip if value doesn't exist
				if(!Option.Options[pair.Key].Values.ContainsKey(pair.Value))
					continue;
			}

			return "";
		}

		/**
		 * Parses the set of flags in Options to make a human readable list
		 * of options, that can be filtered by Mode.
		 */
		public List<Tuple<Mode, string, string>> MakeDisplayable()
		{
			var err = ValidateOptions();
			if(err != "")
				throw new FormatException(err);

			var list = new List<Tuple<Mode, string, string>>();

			foreach(var pair in Options) {
				if(Option.Options.ContainsKey(pair.Key))
					list.Add(new Tuple<Mode, string, string>(
						Option.Options[pair.Key].Mode,
						Option.Options[pair.Key].Name,
						Option.Options[pair.Key].Values.ContainsKey(pair.Value)
								? Option.Options[pair.Key].Values[pair.Value]
								: pair.Value
					));
				else
					list.Add(new Tuple<Mode, string, string>(
						Mode.Other,
						pair.Key,
						pair.Value
					));
			}

			return list;
		}
    }
}
