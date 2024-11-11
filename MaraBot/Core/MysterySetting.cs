using System.Collections.Generic;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    /// <summary>
    /// Mystery setting for randomized races.
    /// </summary>
    public struct MysterySetting
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requirement">Setting requirement</param>
        /// <param name="values">Setting values</param>
        public MysterySetting(string requirement = default, Dictionary<string, int> values = null)
        {
            Requirement = requirement;
            Values = values ?? new Dictionary<string, int>();
        }

        /// <summary>
        /// Optional requirement for that setting to be active.
        /// </summary>
        [JsonProperty]
        public readonly string Requirement;

        /// <summary>
        /// Settings values.
        /// </summary>
        [JsonProperty]
        public readonly Dictionary<string, int> Values;
    }
}
