using System.Globalization;

namespace BlazorInvaders.GameObjects
{
    public enum DailyModifier
    {
        None,
        NoBunkers,      // no bunkers to hide behind
        Speedrun,       // aliens start one wave faster
        ShieldStart,    // start with a shield
        BossRush,       // a boss every 3rd wave instead of every 5th
        Glass,          // one life, double points
    }

    /// <summary>
    /// The game everyone plays on a given (UTC) day: the same random seed and the same
    /// modifier, with its own leaderboard.
    /// </summary>
    public sealed record DailyChallenge(DateOnly Date, DailyModifier Modifier)
    {
        /// <summary>yyyyMMdd, used as the random seed and the leaderboard key.</summary>
        public int Seed => Date.Year * 10000 + Date.Month * 100 + Date.Day;

        public string Key => Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        public static DailyChallenge For(DateOnly date)
        {
            var seed = date.Year * 10000 + date.Month * 100 + date.Day;
            var modifier = (DailyModifier)new Random(seed).Next(Enum.GetValues<DailyModifier>().Length);
            return new DailyChallenge(date, modifier);
        }

        public static DailyChallenge Today() => For(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
