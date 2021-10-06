using System;
using System.Collections.Generic;
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
        public string Name;

        /// <summary>
        /// Description of the preset.
        /// </summary>
        public string Description;

        /// <summary>
        /// Version of the randomizer the preset is made for.
        /// </summary>
        public string Version;

        /// <summary>
        /// Author of the preset.
        /// </summary>
        public string Author;

        /// <summary>
        /// List of option flags as used in the randomizer options string.
        /// </summary>
        public Dictionary<string, string> Options;

        /// <summary>
        /// List of prettified general options contained in Options.
        /// </summary>
        [JsonIgnore] public Dictionary<string, string> GeneralOptions;

        /// <summary>
        /// List of prettified mode-specific options contained in Options.
        /// </summary>
        [JsonIgnore] public Dictionary<string, string> ModeOptions;

        /// <summary>
        /// List of prettified other options contained in Options.
        /// </summary>
        [JsonIgnore] public Dictionary<string, string> OtherOptions;

        /// <summary>
        /// Priority of the preset in determining the next weekly preset.
        /// </summary>
        public int Weight;

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
            GeneralOptions = new Dictionary<string, string>();
            ModeOptions    = new Dictionary<string, string>();
            OtherOptions   = new Dictionary<string, string>();

            foreach(var option in list)
            {
                if(option.Item1 == Mode.Mode)
                    continue;
                if(option.Item1 == mode)
                    ModeOptions.Add(option.Item2, option.Item3);
                else if(option.Item1 == Mode.General)
                    GeneralOptions.Add(option.Item2, option.Item3);
                else
                    OtherOptions.Add(option.Item2, option.Item3);
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
