using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    /// <summary>
    /// Holds information on an option flags preset.
    /// </summary>
    public struct Preset : IEquatable<Preset>
    {
        /// <summary>
        /// Name of the preset.
        /// </summary>
        [JsonProperty]
        public readonly string Name;

        /// <summary>
        /// Description of the preset.
        /// </summary>
        [JsonProperty]
        public readonly string Description;

        /// <summary>
        /// Author of the preset.
        /// </summary>
        [JsonProperty]
        public readonly string Author;

        /// <summary>
        /// List of option flags as used in the randomizer options string.
        /// </summary>
        [JsonProperty]
        public readonly Dictionary<string, string> Options;

        /// <summary>
        /// Priority of the preset in determining the next weekly preset.
        /// </summary>
        [JsonProperty]
        public readonly int Weight;

        /// <summary>
        /// Human-readable string describing the Open Mode Goal if applicable.
        /// </summary>
        [JsonIgnore]
        public string OpenModeGoal => m_OpenModeGoal;
        /// <summary>
        /// List of prettified general options contained in Options.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, string> GeneralOptions => m_GeneralOptions;
        /// <summary>
        /// List of prettified mode-specific options contained in Options.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, string> ModeOptions => m_ModeOptions;
        /// <summary>
        /// List of prettified other options contained in Options.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, string> OtherOptions => m_OtherOptions;

        [JsonIgnore] private string m_OpenModeGoal;
        [JsonIgnore] private Dictionary<string, string> m_GeneralOptions;
        [JsonIgnore] private Dictionary<string, string> m_ModeOptions;
        [JsonIgnore] private Dictionary<string, string> m_OtherOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Preset name.</param>
        /// <param name="description">Preset description.</param>
        /// <param name="version">Randomizer version for preset.</param>
        /// <param name="author">Preset author.</param>
        /// <param name="options">Collection of raw options.</param>
        public Preset(string name, string description, string author, IReadOnlyDictionary<string, string> options)
        {
            Name = name;
            Description = description;
            Author = author;
            Options = new Dictionary<string, string>(options);
            Weight = 1;
            m_OpenModeGoal = String.Empty;
            m_GeneralOptions = m_ModeOptions = m_OtherOptions = null;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="preset">Preset to copy.</param>
        public Preset(in Preset preset)
        {
            Name = preset.Name;
            Description = preset.Description;
            Author = preset.Author;
            Options = new Dictionary<string, string>(preset.Options);
            Weight = preset.Weight;

            m_OpenModeGoal = String.Empty;
            m_GeneralOptions = new Dictionary<string, string>(preset.GeneralOptions);
            m_ModeOptions = new Dictionary<string, string>(preset.ModeOptions);
            m_OtherOptions = new Dictionary<string, string>(preset.OtherOptions);
        }

        /// <summary>
        /// Parses the set of flags in Options, based on the options given,
        /// to populate the list of general, mode-specific and other options.
        /// </summary>
        public void MakeDisplayable(IReadOnlyDictionary<string, Option> options)
        {
            if (Options == null)
                return;

            var list = new List<Tuple<Category, string, string>>();

            foreach (var pair in Options)
            {
                // These options are shown separately.
                if (pair.Key == "mode" || pair.Key == "opGoal")
                    continue;

                if (options.ContainsKey(pair.Key)) // Found key
                {
                    var o = options[pair.Key];
                    list.Add(new Tuple<Category, string, string>(o.Category, o.Name, o.ParseValue(pair.Value)));
                }
                else // No key found, use raw values
                {
                    // Skip version...
                    if (pair.Key == "version")
                        continue;
                    list.Add(new Tuple<Category, string, string>(Category.Other, pair.Key, pair.Value));
                }
            }

            GameMode gameMode = Options.ContainsKey("mode") ? Option.OptionValueToGameMode(Options["mode"]) : GameMode.Rando;

            // Consider open mode goal separately.
            if (gameMode == GameMode.Open)
            {
                if (!Options.TryGetValue("opGoal", out var goalValue))
                    goalValue = "vlong";

                m_OpenModeGoal = options["opGoal"].ParseValue(goalValue);
            }

            m_GeneralOptions = new Dictionary<string, string>();
            m_ModeOptions    = new Dictionary<string, string>();
            m_OtherOptions   = new Dictionary<string, string>();

            foreach(var option in list)
            {
                if (option.Item1 == (Category)gameMode)
                    m_ModeOptions.Add(option.Item2, option.Item3);
                else if (option.Item1 == Category.General)
                    m_GeneralOptions.Add(option.Item2, option.Item3);
                else
                    m_OtherOptions.Add(option.Item2, option.Item3);
            }
        }

        public bool Equals(Preset other)
        {
            return Name == other.Name && Description == other.Description && Author == other.Author;
        }

        public override bool Equals(object obj)
        {
            return obj is Preset other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Description, Author);
        }
    }
}
