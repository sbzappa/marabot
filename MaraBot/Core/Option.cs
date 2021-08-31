using System.Collections.Generic;
using System.ComponentModel;

namespace MaraBot.Core
{
    /// <summary>
    /// Randomizer modes the options are categorized by.
    /// </summary>
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

    /// <summary>
    /// Type of value an Option has.
    /// </summary>
    public enum OptionType
    {
        Enum, // Select one value from Values
        List  // Select multiple values from Values, separated by a comma
    }

    /// <summary>
    /// Information about a randomizer option.
    /// </summary>
    public struct Option
    {
        /// <summary>
        /// Name of the options, as seen in the randomizer.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Mode it belongs in.
        /// </summary>
        public readonly Mode Mode;

        /// <summary>
        /// Type of value it supports.
        /// </summary>
        [DefaultValue(OptionType.Enum)]
        public readonly OptionType Type;

        /// <summary>
        /// Possible values the option can have in the options JSON/string,
        /// together with the display name as seen in the randomizer.
        /// Default values will not be in this dictionary,
        /// because those values don't appear in the options string.
        /// </summary>
        public readonly IDictionary<string, string> Values;

        /// <summary>
        /// Create an Option with specified properties.
        /// </summary>
        public Option(string name, Mode mode, OptionType type, IDictionary<string, string> values)
        {
            Name   = name  ;
            Mode   = mode  ;
            Values = values;
            Type   = type  ;
        }

        /// <summary>
        /// Translate a mode to a human-readable string.
        /// </summary>
        public static string ModeToPrettyString(Mode mode)
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

        /// <summary>
        /// Translate a 'mode' value to the correct Mode enum.
        /// </summary>
        public static Mode OptionValueToMode(string modeString)
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

        /// <summary>
        /// Transform a string list of values (OptionType.List) into a List.
        /// </summary>
        public static List<string> ParseList(string values) {
            var list = new List<string>(values.Split(','));
            list.RemoveAll(s => s == "");
            return list;
        }
    }
}
