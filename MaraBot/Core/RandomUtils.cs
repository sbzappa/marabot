using System;
using System.Linq;

namespace MaraBot.Core
{
    public static class RandomUtils
    {
        static Random s_TimeBasedRandom = new Random(DateTime.Now.GetHashCode());
        static DateTime s_FirstWeek = new DateTime(2021, 08, 13, 0, 0, 0);

        public static int GetRandomIndex(int minIndex, int maxIndex)
        {
            return s_TimeBasedRandom.Next(minIndex, maxIndex);
        }

        public static string GetRandomSeed()
        {
            return GetSeed(s_TimeBasedRandom);
        }

        public static int GetWeekNumber()
        {
            var elapsed = DateTime.UtcNow.Subtract(s_FirstWeek);
            var elapsedWeeks = elapsed.Days / 7;

            return elapsedWeeks;
        }

        public static string GetWeeklySeed(int seedMultiplier)
        {
            var weekNumber = GetWeekNumber();
            var seed = weekNumber * seedMultiplier;

            var random = new Random(seed);
            return GetSeed(random);
        }

        private static string GetSeed(Random randomGenerator)
        {
            byte[] buffer = new byte[8];
            randomGenerator.NextBytes(buffer);
            return String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        }
    }
}
