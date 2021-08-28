using System;
using System.ComponentModel.DataAnnotations;
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

		/**
		 * Randomizer modes the options are categorized by.
		 */
		public enum Mode
		{
			[Display(Name = "General"     )] General,
			[Display(Name = "Rando"       )] Rando,
			[Display(Name = "Open World"  )] Open,
			[Display(Name = "Ancient Cave")] AncientCave,
			[Display(Name = "Boss Rush"   )] BossRush,
			[Display(Name = "Chaos"       )] Chaos
		}

		/**
		 * Information about a randomizer option.
		 */
		private struct Option
		{
			/**
			 * Name of the options, as seen in the randomizer.
			 */
			public readonly string Name;

			/**
			 * Mode it belongs in.
			 */
			public readonly Mode Mode;

			/**
			 * Possible values the option can have in the options JSON,
			 * together with the display name as seen in the randomizer.
			 */
			public readonly IDictionary<string, string> Values;

			public Option( string name, Mode mode, IDictionary<string, string> values )
			{
				Name   = name  ;
				Mode   = mode  ;
				Values = values;
			}
		}

		/**
		 * Parses the set of flags in Options to make a human readable list
		 * of options, that can be filtered by Mode.
		 */
		public List<Tuple<Mode, string, string>> MakeDisplayable()
		{
			var list = new List<Tuple<Mode, string, string>>();

			foreach(var pair in Options)
			{
				if(!AllOptions.ContainsKey(pair.Key))
					throw new System.FormatException("'" + pair.Key + "' is not a valid option");
				if(!AllOptions[pair.Key].Values.ContainsKey(pair.Value))
					throw new System.FormatException(
						"'" + pair.Value + "' is not a valid value for option '" + pair.Key + "'"
					);

				list.Add(new Tuple<Mode, string, string>(
					AllOptions[pair.Key].Mode,
					AllOptions[pair.Key].Name,
					AllOptions[pair.Key].Values[pair.Value]
				));
			}

			return list;
		}

		/**
		 * All options in the randomizer, indexable by the keys in the options JSON.
		 */
		private static readonly Dictionary<string, Option> AllOptions = new Dictionary<string, Option>()
		{
			{
				"aggBosses", new Option(
					"Aggressive bosses",
					Mode.General,
					new Dictionary<string, string>()
					{
						{ "yes", "Yes" }
					}
				)
			},
			{
				"aggEnemies", new Option(
					"Aggressive enemies",
					Mode.General,
					new Dictionary<string, string>()
					{
						{ "yes", "Yes" }
					}
				)
			}
		};
    }
}
