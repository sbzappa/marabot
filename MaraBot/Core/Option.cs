using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MaraBot.Core
{
	/**
	 * Randomizer modes the options are categorized by.
	 */
	public enum Mode
	{
		Mode,
		General,
		Rando,
		Open,
		AncientCave,
		BossRush,
		Chaos,
		Other
	}

	/**
	 * Information about a randomizer option.
	 */
	public struct Option
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
		 * Possible values the option can have in the options JSON/string,
		 * together with the display name as seen in the randomizer.
		 * Default values will not be in this dictionary,
		 * because those values don't appear in the options string.
		 */
		public readonly IDictionary<string, string> Values;

		public Option( string name, Mode mode, IDictionary<string, string> values )
		{
			Name   = name  ;
			Mode   = mode  ;
			Values = values;
		}

		/**
		 * Contains a deserialization of the options config file,
		 * containing all randomizer options.
		 */
		public static Dictionary<string, Option> Options;

		public static string ModeToString(Mode mode)
		{
			switch(mode)
			{
				case Mode.Mode       : return "Game Mode"   ;
				case Mode.Rando      : return "Rando"       ;
				case Mode.Open       : return "Open World"  ;
				case Mode.AncientCave: return "Ancient Cave";
				case Mode.BossRush   : return "Boss Rush"   ;
				case Mode.Chaos      : return "Chaos"       ;
				default              : return "Other"       ;
			}
		}

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
	}
}
