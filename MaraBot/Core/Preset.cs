using System.Collections.Generic;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    public class Preset
    {
        public string Name;
        public string Version;
        [JsonConverter(typeof(OptionsConverter))] public Dictionary<string, string> Options;
        public string Author;
        public string Description;

		/**
		 * Randomizer modes the options are categorized by.
		 */
		private enum Mode
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
			readonly string Name;

			/**
			 * Mode it belongs in.
			 */
			readonly Mode Mode;

			/**
			 * Possible values the option can have in the options JSON,
			 * together with the display name as seen in the randomizer.
			 */
			readonly IDictionary<string, string> Values;

			public Option( string name, Mode mode, IDictionary<string, string> values )
			{
				Name   = name  ;
				Mode   = mode  ;
				Values = values;
			}
		}

		public List<Tuple<Mode, string, string>> MakeDisplayable()
		{
			var list = new List<Tuple<Mode, string, string>>();

			foreach(var pair in Options) {
				if(!AllOptions.ContainsKey(pair.Key))
					throw new System.FormatException("'" + pair.Key + "' is not a valid option");
				if(!AllOptions[pair.Key].ContainsKey(pair.Value))
					throw new System.FormatException("'" + pair.Value + "' is not a valid value for option '" + pair.Key + "'");

				list.Add(new Tuple<Mode, string, string>(AllOptions[pair.Key].Mode, AllOptions[pair.Key].Name, AllOptions[pair.Key].Values[pair.Value]));
			}

			return list;
		}

		/**
		 * All options in the randomizer, indexable by the keys in the options JSON.
		 */
		private static const var AllOptions = new Dictionary<string, Option>(
			// General
			new KeyValuePair("aggBosses", new Option("Aggressive bosses", Mode.General, new Dictionary(
				new KeyValuePair("yes", "Yes")
			))),
			new KeyValuePair("aggEnemies", new Option("Aggressive enemies", Mode.General, new Dictionary(
				new KeyValuePair("yes", "Yes")
			)))
		);
    }
}
