
using System.Collections.Generic;

namespace MaraBot.Core
{
    /// <summary>
    /// Interface that provides a read-only view unto the bot configuration.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Prefix used for discord commands.
        /// </summary>
        public string Prefix { get; }
        /// <summary>
        /// Discord OAuth token.
        /// </summary>
        public string Token { get; }
        /// <summary>
        /// Roles with race organization privileges.
        /// </summary>
        public IReadOnlyCollection<string> OrganizerRoles { get; }
        /// <summary>
        /// Role to give a user whenever they completed a weekly.
        /// </summary>
        public string WeeklyCompletedRole { get; }
        /// <summary>
        /// Role to give a user whenever they forfeited a weekly.
        /// </summary>
        public string WeeklyForfeitedRole { get; }
        /// <summary>
        /// Channel with weekly spoilers.
        /// </summary>
        public string WeeklySpoilerChannel { get; }
    }

    /// <summary>
    /// Configuration of the bot.
    /// </summary>
    public class Config : IConfig
    {
        /// <inheritdoc cref="IConfig.Prefix"/>
        public string Prefix;
        /// <inheritdoc cref="IConfig.Token"/>
        public string Token;
        /// <inheritdoc cref="IConfig.OrganizerRoles"/>
        public string[] OrganizerRoles;
        /// <inheritdoc cref="IConfig.WeeklyCompletedRole"/>
        public string WeeklyCompletedRole;
        /// <inheritdoc cref="IConfig.WeeklyForfeitedRole"/>
        public string WeeklyForfeitedRole;
        /// <inheritdoc cref="IConfig.WeeklySpoilerChannel"/>
        public string WeeklySpoilerChannel;

        /// <inheritdoc cref="IConfig.Prefix"/>
        string IConfig.Prefix => Prefix;
        /// <inheritdoc cref="IConfig.Token"/>
        string IConfig.Token => Token;
        /// <inheritdoc cref="IConfig.OrganizerRoles"/>
        IReadOnlyCollection<string> IConfig.OrganizerRoles => OrganizerRoles;
        /// <inheritdoc cref="IConfig.WeeklyCompletedRole"/>
        string IConfig.WeeklyCompletedRole => WeeklyCompletedRole;
        /// <inheritdoc cref="IConfig.WeeklyForfeitedRole"/>
        string IConfig.WeeklyForfeitedRole => WeeklyForfeitedRole;
        /// <inheritdoc cref="IConfig.WeeklySpoilerChannel"/>
        string IConfig.WeeklySpoilerChannel => WeeklySpoilerChannel;
    }
}
