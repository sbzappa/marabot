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
        /// Version of the randomizer the preset is made for.
        /// </summary>
        [JsonProperty]
        public readonly string Version;

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
        /// List of prettified general options contained in Options.
        /// </summary>
        public IReadOnlyDictionary<string, string> GeneralOptions => m_GeneralOptions;
        /// <summary>
        /// List of prettified mode-specific options contained in Options.
        /// </summary>
        public IReadOnlyDictionary<string, string> ModeOptions => m_ModeOptions;
        /// <summary>
        /// List of prettified other options contained in Options.
        /// </summary>
        public IReadOnlyDictionary<string, string> OtherOptions => m_OtherOptions;

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
        public Preset(string name, string description, string version, string author, IReadOnlyDictionary<string, string> options)
        {
            Name = name;
            Description = description;
            Version = version;
            Author = author;
            Options = options.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Weight = 1;
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
            Version = preset.Version;
            Author = preset.Author;
            Options = preset.Options.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Weight = preset.Weight;

            m_GeneralOptions = preset.GeneralOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            m_ModeOptions = preset.ModeOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            m_OtherOptions = preset.OtherOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Parses the set of flags in Options, based on the options given,
        /// to populate the list of general, mode-specific and other options.
        /// </summary>
        public void MakeDisplayable(IReadOnlyDictionary<string, Option> options)
        {
            var list = new List<Tuple<Mode, string, string>>();

            foreach(var pair in Options) {
                if(options.ContainsKey(pair.Key)) { // Found key
                    var o = options[pair.Key];
                    if(o.Type == OptionType.List) // List type
                        list.Add(new Tuple<Mode, string, string>(
                            o.Mode,
                            o.Name,
                            String.Join(", ", Option.ParseList(pair.Value).ConvertAll(
                                v => o.Values.ContainsKey(v)
                                        ? o.Values[v] // Value found
                                        : v // No value found, use raw value
                            ))
                        ));
                    else // Enum type
                        list.Add(new Tuple<Mode, string, string>(
                            o.Mode,
                            o.Name,
                            o.Values.ContainsKey(pair.Value)
                                    ? o.Values[pair.Value] // Value found
                                    : pair.Value // No value found, use raw value
                        ));
                }
                else // No key found, use raw values
                    list.Add(new Tuple<Mode, string, string>(
                        Mode.Other,
                        pair.Key,
                        pair.Value
                    ));
            }

            Mode mode = Options.ContainsKey("mode") ? Option.OptionValueToMode(Options["mode"]) : Mode.Rando;
            m_GeneralOptions = new Dictionary<string, string>();
            m_ModeOptions    = new Dictionary<string, string>();
            m_OtherOptions   = new Dictionary<string, string>();

            foreach(var option in list)
            {
                if(option.Item1 == Mode.Mode)
                    continue;
                if(option.Item1 == mode)
                    m_ModeOptions.Add(option.Item2, option.Item3);
                else if(option.Item1 == Mode.General)
                    m_GeneralOptions.Add(option.Item2, option.Item3);
                else
                    m_OtherOptions.Add(option.Item2, option.Item3);
            }
        }

        public bool Equals(Preset other)
        {
            return Name == other.Name && Description == other.Description && Version == other.Version && Author == other.Author;
        }

        public override bool Equals(object obj)
        {
            return obj is Preset other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Description, Version, Author);
        }
    }
}
