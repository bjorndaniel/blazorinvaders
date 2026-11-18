namespace BlazorInvaders.GameObjects
{
    /// <summary>The shots fired by one trigger pull (two with double shot).</summary>
    public sealed class Volley
    {
        /// <summary>Shots of the volley that have not yet ended.</summary>
        public int InFlight { get; internal set; }
        /// <summary>True once any shot of the volley has hit something.</summary>
        public bool Hit { get; internal set; }
    }

    /// <summary>
    /// Counts consecutive hits and turns them into a score multiplier. A volley whose shots
    /// all end without hitting anything breaks the combo, so one stray shot of a double shot
    /// doesn't count as a miss.
    /// </summary>
    public sealed class ComboTracker
    {
        /// <summary>Consecutive hits needed to raise the multiplier by one.</summary>
        public const int HitsPerLevel = 5;
        /// <summary>Highest score multiplier.</summary>
        public const int MaxMultiplier = 4;

        /// <summary>Consecutive hits so far; 0 after a miss.</summary>
        public int Count { get; internal set; }
        /// <summary>Score multiplier: 1, plus one per <see cref="HitsPerLevel"/> hits, up to <see cref="MaxMultiplier"/>.</summary>
        public int Multiplier => Math.Min(1 + Count / HitsPerLevel, MaxMultiplier);

        /// <summary>Starts tracking a volley of <paramref name="shots"/> shots.</summary>
        public Volley StartVolley(int shots) => new() { InFlight = shots };

        /// <summary>Records a hit by a shot of <paramref name="volley"/>. Returns true if the multiplier went up.</summary>
        public bool AddHit(Volley volley)
        {
            volley.Hit = true;
            var before = Multiplier;
            Count++;
            return Multiplier > before;
        }

        /// <summary>Records that one shot of <paramref name="volley"/> is spent.</summary>
        public void ShotEnded(Volley volley)
        {
            volley.InFlight--;
            if (volley.InFlight == 0 && !volley.Hit)
                Count = 0;
        }

        /// <summary>Resets the combo to 0 without waiting for a miss.</summary>
        public void Break() => Count = 0;
    }
}
