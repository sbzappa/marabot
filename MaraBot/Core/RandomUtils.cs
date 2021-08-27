using System;
using System.Linq;

namespace MaraBot.Core
{
    public static class RandomUtils
    {
        static Random s_TimeBasedRandom = new Random(DateTime.Now.GetHashCode());
        static DateTime s_FirstWeek = new DateTime(2021, 08, 19, 20, 0, 0);
        
        public static string GetRandomSeed()
        {
            return GetSeed(s_TimeBasedRandom);
        }

        public static string GetWeeklySeed(int seedMultiplier)
        {
            var elapsed = DateTime.Now.Subtract(s_FirstWeek);
            var elapsedWeeks = elapsed.Days / 7;

            var seed = elapsedWeeks * seedMultiplier;
            
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