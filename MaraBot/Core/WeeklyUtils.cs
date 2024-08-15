using System;
using System.Globalization;
using System.Linq;

namespace MaraBot.Core
{
    /// <summary>
    /// Methods used to generate randomizer seeds.
    /// </summary>
    public static class WeeklyUtils
    {
        static Random s_TimeBasedRandom = new Random(DateTime.Now.GetHashCode());
        static DateTime s_FirstWeek = new DateTime(2021, 08, 13, 18, 0, 0, DateTimeKind.Utc);
        static readonly TimeSpan s_WeeklyDuration = TimeSpan.FromDays(7.0);
        //static readonly TimeSpan s_WeeklyDuration = TimeSpan.FromMinutes(1.0);

        enum ChallengeDuration
        {
            EveryMinute,
            EveryWeek,
            EveryMonth
        }

        static readonly ChallengeDuration s_ChallengeDuration = ChallengeDuration.EveryMonth;

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
        /// Retrieves the duration until next challenge reset.
        /// </summary>
        /// <returns>Returns a TimeSpan duration until next challenge reset.</returns>
        public static TimeSpan GetRemainingChallengeDuration(DateTime timeStamp)
        {
            DateTime timeOfReset;
            switch (s_ChallengeDuration)
            {
                case ChallengeDuration.EveryMinute:
                {
                    var nextMinute = timeStamp.AddMinutes(1);
                    timeOfReset = new DateTime(nextMinute.Year, nextMinute.Month, nextMinute.Day, nextMinute.Hour, nextMinute.Minute, 0);
                    break;
                }
                case ChallengeDuration.EveryWeek:
                {
                    var nextWeek = timeStamp.AddDays(7);
                    timeOfReset = new DateTime(nextWeek.Year, nextWeek.Month, nextWeek.Day);
                    break;
                }
                case ChallengeDuration.EveryMonth:
                default:
                {
                    var nextMonth = timeStamp.AddMonths(1);
                    timeOfReset = new DateTime(nextMonth.Year, nextMonth.Month, 1);
                    break;
                }
            }

            return timeOfReset - DateTime.UtcNow;
        }

        /// <summary>
        /// Returns whether challenge should be reset.
        /// </summary>
        /// <param name="timeStamp">TimeStamp of last </param>
        /// <returns></returns>
        public static bool ShouldResetChallenge(DateTime timeStamp)
        {
            switch (s_ChallengeDuration)
            {
                case ChallengeDuration.EveryMinute:
                    return timeStamp.Minute != DateTime.UtcNow.Minute;
                case ChallengeDuration.EveryWeek:
                    return ISOWeek.GetWeekOfYear(timeStamp) != ISOWeek.GetWeekOfYear(DateTime.UtcNow);
                case ChallengeDuration.EveryMonth:
                default:
                    return timeStamp.Month != DateTime.UtcNow.Month;
            }
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
            var divide = elapsed.Divide(s_WeeklyDuration);

            return (int)Math.Truncate(divide);
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
