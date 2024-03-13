using System;
using System.Linq;

namespace MaraBot.Core
{
    /// <summary>
    /// Methods used to generate randomizer seeds.
    /// </summary>
    public static class WeeklyUtils
    {
        static Random s_TimeBasedRandom = new Random(DateTime.Now.GetHashCode());
        static DateTime s_FirstWeek = new DateTime(2021, 08, 13, 0, 0, 0);
        static readonly TimeSpan s_WeeklyDuration = TimeSpan.FromDays(7.0);

        /// <summary>
        /// Retrieves the duration until next weekly reset.
        /// </summary>
        /// <returns>Returns a TimeSpan duration until next weekly reset.</returns>
        public static TimeSpan GetRemainingWeeklyDuration(int weekNumber)
        {
            var currentWeek = GetWeek(weekNumber);
            var elapsed = DateTime.UtcNow.Subtract(currentWeek);
            var divide = elapsed.Divide(s_WeeklyDuration);

            if (divide >= 1.0)
                return TimeSpan.Zero;

            return s_WeeklyDuration - elapsed;
        }

        /// <summary>
        /// Retrieves the duration until next challenge reset
        /// </summary>
        /// <returns>Returns a TimeSpan duration until next challenge reset.</returns>
        public static TimeSpan GetRemainingChallengeDuration()
        {
            var nextMonth = DateTime.UtcNow.AddMonths(1);
            var timeOfReset = new DateTime(nextMonth.Year, nextMonth.Month, 1);
            return timeOfReset - DateTime.UtcNow;
        }

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

        /// <summary>
        /// Retrieves the current week (starting date) for
        /// the current weekly.
        /// </summary>
        /// <returns>Returns a DateTime time stamp.</returns>
        public static DateTime GetWeek() =>
            GetWeek(GetWeekNumber());

        /// <summary>
        /// Retrieves the week (starting date) for
        /// specified week number.
        /// </summary>
        /// <returns>Returns a DateTime time stamp.</returns>
        public static DateTime GetWeek(int weekNumber)
        {
            var elapsed = s_WeeklyDuration.Multiply(weekNumber);
            return s_FirstWeek.Add(elapsed);
        }


        private static string GetSeed(Random randomGenerator)
        {
            byte[] buffer = new byte[8];
            randomGenerator.NextBytes(buffer);
            return String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        }
    }
}
