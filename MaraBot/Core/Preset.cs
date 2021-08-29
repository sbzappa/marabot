using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    /// <summary>
    /// Holds information on an option flags preset.
    /// </summary>
    public class Preset
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
        [JsonConverter(typeof(OptionsConverter))] public Dictionary<string, string> Options;

        /// <summary>
        /// List of prettified general options contained in Options.
        /// </summary>
        [JsonIgnoreAttribute] public Dictionary<string, string> GeneralOptions;

        /// <summary>
        /// List of prettified mode-specific options contained in Options.
        /// </summary>
        [JsonIgnoreAttribute] public Dictionary<string, string> ModeOptions;

        /// <summary>
        /// List of prettified other options contained in Options.
        /// </summary>
        [JsonIgnoreAttribute] public Dictionary<string, string> OtherOptions;

        /// <summary>
        /// Parses the set of flags in Options, based on the options given,
        /// to populate the list of general, mode-specific and other options.
        /// </summary>
        public void MakeDisplayable(Dictionary<string, Option> options)
        {
            var list = new List<Tuple<Mode, string, string>>();

            foreach(var pair in Options) {
                if(options.ContainsKey(pair.Key))
                    list.Add(new Tuple<Mode, string, string>(
                        options[pair.Key].Mode,
                        options[pair.Key].Name,
                        options[pair.Key].Values.ContainsKey(pair.Value)
                                ? options[pair.Key].Values[pair.Value]
                                : pair.Value
                    ));
                else
                    list.Add(new Tuple<Mode, string, string>(
                        Mode.Other,
                        pair.Key,
                        pair.Value
                    ));
            }

            Mode mode = Option.OptionValueToMode(Options["mode"]);
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
    }
}
