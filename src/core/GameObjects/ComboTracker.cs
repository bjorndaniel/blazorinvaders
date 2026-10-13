namespace BlazorInvaders.GameObjects
{
    /// <summary>The shots fired by one trigger pull (two with double shot).</summary>
    public sealed class Volley
    {
        public int InFlight { get; internal set; }
        public bool Hit { get; internal set; }
    }

    /// <summary>
    /// Counts consecutive hits and turns them into a score multiplier. A volley whose shots
    /// all end without hitting anything breaks the combo, so one stray shot of a double shot
    /// doesn't count as a miss.
    /// </summary>
    public sealed class ComboTracker
    {
        public const int HitsPerLevel = 5;
        public const int MaxMultiplier = 4;

        public int Count { get; internal set; }
        public int Multiplier => Math.Min(1 + Count / HitsPerLevel, MaxMultiplier);

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

        public void Break() => Count = 0;
    }
}
