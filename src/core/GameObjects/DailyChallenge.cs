using System.Globalization;

namespace BlazorInvaders.GameObjects
{
    /// <summary>The twist applied to a daily challenge.</summary>
    public enum DailyModifier
    {
        /// <summary>No twist.</summary>
        None,
        /// <summary>No bunkers to hide behind.</summary>
        NoBunkers,      // no bunkers to hide behind
        /// <summary>Aliens start one wave faster.</summary>
        Speedrun,       // aliens start one wave faster
        /// <summary>Start with a shield.</summary>
        ShieldStart,    // start with a shield
        /// <summary>A boss every 3rd wave instead of every 5th.</summary>
        BossRush,       // a boss every 3rd wave instead of every 5th
        /// <summary>One life, double points.</summary>
        Glass,          // one life, double points
    }

    /// <summary>
    /// The game everyone plays on a given (UTC) day: the same random seed and the same
    /// modifier, with its own leaderboard.
    /// </summary>
    /// <param name="Date">The UTC day it is played on.</param>
    /// <param name="Modifier">The twist for that day.</param>
    public sealed record DailyChallenge(DateOnly Date, DailyModifier Modifier)
    {
        /// <summary>yyyyMMdd, used as the random seed and the leaderboard key.</summary>
        public int Seed => Date.Year * 10000 + Date.Month * 100 + Date.Day;

        /// <summary>The date as yyyyMMdd, used as the leaderboard key.</summary>
        public string Key => Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        /// <summary>The challenge for <paramref name="date"/>; the modifier is picked from the date, so it is the same for everyone.</summary>
        public static DailyChallenge For(DateOnly date)
        {
            var seed = date.Year * 10000 + date.Month * 100 + date.Day;
            var modifier = (DailyModifier)new Random(seed).Next(Enum.GetValues<DailyModifier>().Length);
            return new DailyChallenge(date, modifier);
        }

        /// <summary>The challenge for the current UTC day.</summary>
        public static DailyChallenge Today() => For(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
