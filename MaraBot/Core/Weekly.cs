using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    public interface IReadOnlyWeekly
    {
        /// <summary>Week number.</summary>
        public int WeekNumber { get; }
        /// <summary>Preset name.</summary>
        public string PresetName { get; }
        /// <summary>Preset.</summary>
        public Preset Preset { get; }
        /// <summary>Seed used in weekly. This is a string of 16 hexadecimal values.</summary>
        public string Seed { get; }
        /// <summary>Validation Hash. This is a string of 8 hexadecimal values (optional).</summary>
        public string ValidationHash { get; }
        /// <summary>Leaderboard for the weekly race.</summary>
        public IReadOnlyDictionary<string, TimeSpan> Leaderboard { get; }
        /// <summary>Timestamp at which weekly seed has been created.</summary>
        public DateTime Timestamp { get; }
    }


    /// <summary>
    /// Holds information on the weekly race settings.
    /// </summary>
    public class Weekly : IReadOnlyWeekly
    {
        /// <summary>Week number.</summary>
        public int WeekNumber;
        /// <summary>Preset name.</summary>
        public string PresetName;
        /// <summary>Preset.</summary>
        public Preset Preset;
        /// <summary>Seed used in weekly. This is a string of 16 hexadecimal values.</summary>
        public string Seed;
        /// <summary>Validation Hash. This is a string of 8 hexadecimal values (optional).</summary>
        public string ValidationHash;
        /// <summary>Leaderboard for the weekly race.</summary>
        public Dictionary<string, TimeSpan> Leaderboard;
        /// <summary>Timestamp at which weekly seed has been created.</summary>
        public DateTime Timestamp;

        public void AddToLeaderboard(string username, TimeSpan time)
        {
            if (Leaderboard == null)
                Leaderboard = new Dictionary<string, TimeSpan>();

            if (Leaderboard.ContainsKey(username))
                Leaderboard[username] = time;
            else
                Leaderboard.Add(username, time);
        }

        /// <summary>
        /// Load new weekly parameters into weely.
        /// </summary>
        /// <param name="weekly">Weekly instance.</param>
        public void Load(Weekly weekly)
        {
            WeekNumber = weekly.WeekNumber;
            PresetName = weekly.PresetName;
            Preset = weekly.Preset;
            Seed = weekly.Seed;
            Leaderboard = weekly.Leaderboard;
            Timestamp = weekly.Timestamp;
        }

        /// <summary>
        /// Retrieves invalid weekly settings.
        /// </summary>
        public static Weekly Invalid => new Weekly
        {
            WeekNumber = -1,
            PresetName = String.Empty,
            Seed = String.Empty,
            ValidationHash = String.Empty,
            Leaderboard = null,
            Timestamp = DateTime.MinValue
        };

        public static Weekly NotSet => new Weekly
        {
            WeekNumber = WeeklyUtils.GetWeekNumber(),
            PresetName = "not-set",
            Seed = "0",
            ValidationHash = String.Empty,
            Leaderboard = null,
            Timestamp = DateTime.Now
        };

        int IReadOnlyWeekly.WeekNumber => WeekNumber;
        string IReadOnlyWeekly.PresetName => PresetName;
        Preset IReadOnlyWeekly.Preset => Preset;
        string IReadOnlyWeekly.Seed => Seed;
        string IReadOnlyWeekly.ValidationHash => ValidationHash;
        IReadOnlyDictionary<string, TimeSpan> IReadOnlyWeekly.Leaderboard => Leaderboard;
        DateTime IReadOnlyWeekly.Timestamp => Timestamp;
    }
}
