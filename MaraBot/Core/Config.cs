
using System.Collections.Generic;

namespace MaraBot.Core
{
    /// <summary>
    /// Interface that provides a read-only view unto the bot configuration.
    /// </summary>
    public interface IReadOnlyConfig
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
    public class Config : IReadOnlyConfig
    {
        /// <inheritdoc cref="IReadOnlyConfig.Prefix"/>
        public string Prefix;
        /// <inheritdoc cref="IReadOnlyConfig.Token"/>
        public string Token;
        /// <inheritdoc cref="IReadOnlyConfig.OrganizerRoles"/>
        public string[] OrganizerRoles;
        /// <inheritdoc cref="IReadOnlyConfig.WeeklyCompletedRole"/>
        public string WeeklyCompletedRole;
        /// <inheritdoc cref="IReadOnlyConfig.WeeklyForfeitedRole"/>
        public string WeeklyForfeitedRole;
        /// <inheritdoc cref="IReadOnlyConfig.WeeklySpoilerChannel"/>
        public string WeeklySpoilerChannel;

        /// <inheritdoc cref="IReadOnlyConfig.Prefix"/>
        string IReadOnlyConfig.Prefix => Prefix;
        /// <inheritdoc cref="IReadOnlyConfig.Token"/>
        string IReadOnlyConfig.Token => Token;
        /// <inheritdoc cref="IReadOnlyConfig.OrganizerRoles"/>
        IReadOnlyCollection<string> IReadOnlyConfig.OrganizerRoles => OrganizerRoles;
        /// <inheritdoc cref="IReadOnlyConfig.WeeklyCompletedRole"/>
        string IReadOnlyConfig.WeeklyCompletedRole => WeeklyCompletedRole;
        /// <inheritdoc cref="IReadOnlyConfig.WeeklyForfeitedRole"/>
        string IReadOnlyConfig.WeeklyForfeitedRole => WeeklyForfeitedRole;
        /// <inheritdoc cref="IReadOnlyConfig.WeeklySpoilerChannel"/>
        string IReadOnlyConfig.WeeklySpoilerChannel => WeeklySpoilerChannel;
    }
}
