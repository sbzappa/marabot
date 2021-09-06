using System;
using System.Linq;

namespace MaraBot.Core
{
    /// <summary>
    /// Methods used to generate randomizer seeds.
    /// </summary>
    public static class RandomUtils
    {
        static Random s_TimeBasedRandom = new Random(DateTime.Now.GetHashCode());
        static DateTime s_FirstWeek = new DateTime(2021, 08, 13, 0, 0, 0);

        /// <summary>
        /// Retrieves a random integer index in between minIndex and maxIndex.
        /// </summary>
        /// <param name="minIndex">Min Index value.</param>
        /// <param name="maxIndex">Max Index value.</param>
        /// <returns>Returns a random integer index.</returns>
        public static int GetRandomIndex(int minIndex, int maxIndex)
        {
            return s_TimeBasedRandom.Next(minIndex, maxIndex);
        }

        /// <summary>
        /// Retrieves a random randomizer seed string.
        /// A randomizer seed is a string of 16 hexadecimal values.
        /// </summary>
        /// <returns>Returns a random seed value.</returns>
        public static string GetRandomSeed()
        {
            return GetSeed(s_TimeBasedRandom);
        }

        /// <summary>
        /// Retrieves the current week number for weeklies.
        /// Week zero started on 2021-08-13.
        /// </summary>
        /// <returns>Returns a week number.</returns>
        public static int GetWeekNumber()
        {
            var elapsed = DateTime.UtcNow.Subtract(s_FirstWeek);
            var elapsedWeeks = elapsed.Days / 7;

            return elapsedWeeks;
        }

        private static string GetSeed(Random randomGenerator)
        {
            byte[] buffer = new byte[8];
            randomGenerator.NextBytes(buffer);
            return String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        }
    }
}
