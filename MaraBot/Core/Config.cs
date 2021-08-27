
namespace MaraBot.Core
{
    public interface IWeeklyConfig
    {
        int Seed { get; }
    }

    public struct Config : IWeeklyConfig
    {
        public string Token;
        public int WeeklySeed;

        int IWeeklyConfig.Seed => WeeklySeed;
    }
}