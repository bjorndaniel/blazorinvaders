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
