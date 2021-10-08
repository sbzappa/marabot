
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    /// <summary>
    /// Configuration of the bot.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Prefix used for discord commands.
        /// </summary>
        [JsonProperty]
        public readonly string Prefix;
        /// <summary>
        /// Discord OAuth token.
        /// </summary>
        [JsonProperty]
        public readonly string Token;
        /// <summary>
        /// Roles with race organization privileges.
        /// </summary>
        [JsonProperty]
        public readonly string[] OrganizerRoles;
        /// <summary>
        /// Role to give a user whenever they completed a weekly.
        /// </summary>
        [JsonProperty]
        public readonly string WeeklyCompletedRole;
        /// <summary>
        /// Role to give a user whenever they forfeited a weekly.
        /// </summary>
        [JsonProperty]
        public readonly string WeeklyForfeitedRole;
        /// <summary>
        /// Channel with weekly spoilers.
        /// </summary>
        [JsonProperty]
        public readonly string WeeklySpoilerChannel;
    }
}
