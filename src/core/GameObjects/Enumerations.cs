namespace BlazorInvaders.GameObjects
{
    public enum AlienType
    {
        Squid,
        Crab,
        Octopus
    }

    public enum Direction
    {
        Right,
        Left,
    }

    public enum BombType
    {
        Straight,
        Zigzag,
        Rolling,
    }

    /// <summary>Controls held during a frame. Values must match the input bits in game.js.</summary>
    [Flags]
    public enum GameInput
    {
        None  = 0,
        Left  = 1,
        Right = 2,
        Fire  = 4,
    }

    /// <summary>Things that happened during a step, for cosmetic effects. Values must match game.js.</summary>
    public enum GameEventType
    {
        AlienKilled      = 1,
        PlayerHit        = 2,
        MotherShipKilled = 3,
        BunkerHit        = 4,
    }

    [Flags]
    public enum SoundEffect
    {
        None        = 0,
        Shoot       = 1,
        AlienDeath  = 2,
        PlayerDeath = 4,
        MotherShip  = 8,
        StopMarch   = 16,
    }
}
