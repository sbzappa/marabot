using System;
using System.Collections.Generic;
using System.Linq;

namespace MaraBot.Core
{
    public class Weekly
    {
        public int WeekNumber;
        public string PresetName;
        public string Seed;
        public Dictionary<string, TimeSpan> Leaderboard;

        public static Weekly Invalid => new Weekly
        {
            WeekNumber = -1,
            PresetName = String.Empty,
            Seed = String.Empty,
            Leaderboard = null
        };

        public static Weekly Generate(IReadOnlyDictionary<string, Preset> presets)
        {
            var weekNumber = RandomUtils.GetWeekNumber();
            var seed = RandomUtils.GetRandomSeed();

            var weeklyPresets = presets.Where(preset => preset.Value.Weight > 0);
            var totalWeight = weeklyPresets.Sum(preset => preset.Value.Weight);

            if (totalWeight == 0)
                return Weekly.Invalid;

            var index = RandomUtils.GetRandomIndex(0, totalWeight - 1);

            var presetName = String.Empty;

            var sum = 0;
            foreach (var preset in weeklyPresets)
            {
                sum += preset.Value.Weight;
                if (sum > index)
                {
                    presetName = preset.Key;
                    break;
                }
            }

            return new Weekly
            {
                WeekNumber = weekNumber,
                PresetName = presetName,
                Seed = seed,
                Leaderboard = new Dictionary<string, TimeSpan>()
            };
        }
    }
}
