namespace BlazorInvaders.GameObjects
{
    /// <summary>The alien designs; each sets which sprites an alien uses.</summary>
    public enum AlienType
    {
        /// <summary>Top row.</summary>
        Squid,
        /// <summary>Second and third rows.</summary>
        Crab,
        /// <summary>Fourth and fifth rows.</summary>
        Octopus
    }

    /// <summary>Horizontal direction of travel.</summary>
    public enum Direction
    {
        /// <summary>Towards the right edge of the screen.</summary>
        Right,
        /// <summary>Towards the left edge of the screen.</summary>
        Left,
    }

    /// <summary>How an alien bomb drifts sideways as it falls.</summary>
    public enum BombType
    {
        /// <summary>Falls straight down.</summary>
        Straight,
        /// <summary>Swings left and right with a sine wave of up to 8 px per step.</summary>
        Zigzag,
        /// <summary>Sways with a slower cosine wave of up to 5 px per step.</summary>
        Rolling,
    }

    /// <summary>Controls held during a frame. Values must match the input bits in game.js.</summary>
    [Flags]
    public enum GameInput
    {
        /// <summary>No controls held.</summary>
        None = 0,
        /// <summary>Move left.</summary>
        Left = 1,
        /// <summary>Move right.</summary>
        Right = 2,
        /// <summary>Fire a shot.</summary>
        Fire = 4,
    }

    /// <summary>Things that happened during a step, for cosmetic effects. Values must match game.js.</summary>
    public enum GameEventType
    {
        /// <summary>An alien was hit; Value is the points scored, Variant is its row.</summary>
        AlienKilled = 1,
        /// <summary>The player lost a life or the game.</summary>
        PlayerHit = 2,
        /// <summary>The mothership was hit; Value is the points scored.</summary>
        MotherShipKilled = 3,
        /// <summary>A shot or bomb damaged a bunker; Variant is the health it has left.</summary>
        BunkerHit = 4,
        /// <summary>The score multiplier went up; Value is the new multiplier.</summary>
        ComboUp = 5,
        /// <summary>The player caught a power-up; Variant is its <see cref="PowerUpType"/>.</summary>
        PowerUpCollected = 6,
        /// <summary>The shield absorbed a bomb.</summary>
        ShieldBlocked = 7,
        /// <summary>A shot hit the boss.</summary>
        BossHit = 8,
        /// <summary>The boss was defeated; Value is the points scored.</summary>
        BossKilled = 9,
    }

    /// <summary>Kinds of power-up that fall from killed aliens.</summary>
    public enum PowerUpType
    {
        /// <summary>Fires two shots per trigger pull for 10 s.</summary>
        DoubleShot,
        /// <summary>Allows three volleys in flight, at least 120 ms apart, for 10 s.</summary>
        RapidFire,
        /// <summary>Absorbs the next bomb that hits the player; no time limit.</summary>
        Shield,
        /// <summary>Shots pass through aliens for 8 s.</summary>
        Piercing,
        /// <summary>Adds a life, up to <see cref="GameEngine.MaxLives"/>. Instant, so never active.</summary>
        ExtraLife,
    }

    /// <summary>Sounds to play in one frame, combined as flags and passed to JS as <see cref="Rendering.Scene.Sounds"/>.</summary>
    [Flags]
    public enum SoundEffect
    {
        /// <summary>No sound.</summary>
        None = 0,
        /// <summary>The player fired.</summary>
        Shoot = 1,
        /// <summary>An alien or the mothership was hit.</summary>
        AlienDeath = 2,
        /// <summary>A bomb hit the player.</summary>
        PlayerDeath = 4,
        /// <summary>Ping while the mothership is crossing, at most every 500 ms.</summary>
        MotherShip = 8,
        /// <summary>Silences the alien march: a boss wave began or the game ended.</summary>
        StopMarch = 16,
        /// <summary>A power-up was collected.</summary>
        PowerUp = 32,
        /// <summary>The shield absorbed a bomb.</summary>
        ShieldBlock = 64,
        /// <summary>A shot hit the boss.</summary>
        BossHit = 128,
        /// <summary>The boss was defeated.</summary>
        BossDefeated = 256,
    }
}
