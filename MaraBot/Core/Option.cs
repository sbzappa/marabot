using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace MaraBot.Core
{
    /// <summary>
    /// Randomizer modes the options are categorized by.
    /// </summary>
    public enum Category
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
    /// Information about a randomizer option.
    /// </summary>
    public abstract class Option
    {
        /// <summary>
        /// Name of the options, as seen in the randomizer.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Mode it belongs in.
        /// </summary>
        public readonly Category Category;

        /// <summary>
        /// Does this option accept a comma separated list of values?
        /// </summary>
        [DefaultValue(false)]
        public readonly bool List = false;

        /// <summary>
        /// Create an Option with specified properties.
        /// </summary>
        public Option(string name, Category category, bool list = false)
        {
            Name = name;
            Category = category;
            List = list;
        }

        /// <summary>
        /// Make a human-readable version of the given value.
        /// When in list mode, this function will be called for every list entry.
        /// </summary>
        protected abstract string ParseValueItem(string val);

        /// <summary>
        /// Translate a mode to a human-readable string.
        /// </summary>
        public static string CategoryToPrettyString(Category category)
        {
            switch(category)
            {
                case Category.Mode       : return "Game Mode"   ;
                case Category.General    : return "General"     ;
                case Category.Rando      : return "Rando"       ;
                case Category.Open       : return "Open World"  ;
                case Category.AncientCave: return "Ancient Cave";
                case Category.BossRush   : return "Boss Rush"   ;
                case Category.Chaos      : return "Chaos"       ;
                default              : return "Other"       ;
            }
        }

        /// <summary>
        /// Translate a 'mode' value to the correct Mode enum.
        /// </summary>
        public static Category OptionValueToCategory(string modeString)
        {
            switch(modeString)
            {
                case "rando"      : return Category.Rando      ;
                case "open"       : return Category.Open       ;
                case "ancientcave": return Category.AncientCave;
                case "bossrush"   : return Category.BossRush   ;
                case "chaos"      : return Category.Chaos      ;
                default           : return Category.Other      ;
            }
        }

        /// <summary>
        /// Transform a string list of values (OptionType.List) into a List.
        /// </summary>
        public static List<string> ParseList(string values)
        {
            var list = new List<string>(values.Split(','));
            list.RemoveAll(s => s == "");
            return list;
        }

        /// <summary>
        /// Parse a raw option value of this type into a human-readable value.
        /// </summary>
        public string ParseValue(string val)
        {
            return String.Join
            (
                ", ",
                (this.List ? Option.ParseList(val) : new List<string> { val }).Select(this.ParseValueItem)
            );
        }
    }

    /// <summary>
    /// Information about a randomizer enum option.
    /// </summary>
    public class EnumOption : Option
    {
        /// <summary>
        /// Possible values the option can have in the options JSON/string,
        /// together with the display name as seen in the randomizer.
        /// Default values will not be in this dictionary,
        /// because those values don't appear in the options string.
        /// Used when `Type == OptionType.Enum`.
        /// </summary>
        public readonly IDictionary<string, string> Values;

        /// <summary>
        /// Create an EnumOption with specified properties.
        /// </summary>
        public EnumOption(string name, Category category, IDictionary<string, string> values, bool list = false)
            : base(name, category, list)
        {
            Values = values;
        }

        protected override string ParseValueItem(string val)
        {
            return Values.ContainsKey(val) ? Values[val] : val;
        }
    }

    /// <summary>
    /// Information about a randomizer numeric option.
    /// </summary>
    public class NumericOption : Option
    {
        /// <summary>
        /// Maximum decimal precision allowed.
        /// * 0 means the number must be an integer.
        /// * X > 0 means the number can have at most X decimal digits.
        /// * 0 > X means the number must be an integer with at least X trailing zeroes.
        /// Used when `Type == OptionType.Numeric`.
        /// Precision must be between -10 and 10.
        /// </summary>
        [DefaultValue(10)]
        public readonly int Precision;

        /// <summary>
        /// Minimum value for numeric values.
        /// Used when `Type == OptionType.Numeric`.
        /// </summary>
        [DefaultValue(double.MinValue)]
        public readonly double Min;

        /// <summary>
        /// Maximum value for numeric values.
        /// Used when `Type == OptionType.Numeric`.
        /// </summary>
        [DefaultValue(double.MaxValue)]
        public readonly double Max;

        /// <summary>
        /// Create a NumericOption with specified properties.
        /// </summary>
        public NumericOption(string name, Category category, int precision, double min, double max, bool list = false)
            : base(name, category, list)
        {
            Precision = precision;
            Min       = min      ;
            Max       = max      ;
        }

        protected override string ParseValueItem(string val)
        {
            if (Precision > 0)
            {
                // double
                bool success = double.TryParse(val, out var v);
                if (!success)
                    return val;
                return v.ToString($"F{Precision.ToString()}");
            }
            else
            {
                // int
                bool success = int.TryParse(val, out var v);
                if (!success)
                    return val;
                return v.ToString(); // TODO: add rounding for Precision < 0
            }
        }
    }
}
