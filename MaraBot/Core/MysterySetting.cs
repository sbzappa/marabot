using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    /// <summary>
    /// Mystery setting for randomized races.
    /// </summary>
    public struct MysterySetting
    {
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
