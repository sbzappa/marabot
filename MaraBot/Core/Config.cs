
using System.Collections.Generic;

namespace MaraBot.Core
{
    public interface IConfig
    {
        public string Prefix { get; }
        public string Token { get; }
        public IReadOnlyCollection<string> OrganizerRoles { get; }
    }

    public class Config : IConfig
    {
        public string Prefix;
        public string Token;
        public string[] OrganizerRoles;

        string IConfig.Prefix => Prefix;
        string IConfig.Token => Token;
        IReadOnlyCollection<string> IConfig.OrganizerRoles => OrganizerRoles;
    }
}
