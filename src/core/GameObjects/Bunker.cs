using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    /// <summary>A bunker the player can hide behind; wears away as it is hit.</summary>
    public class Bunker
    {
        /// <summary>Creates a bunker with its top-left corner at <paramref name="position"/> (px), at full health.</summary>
        public Bunker(Point position)
        {
            Position = position;
        }

        /// <summary>Top-left corner in game px.</summary>
        public Point Position { get; }
        /// <summary>Hits left, starting at 4.</summary>
        public int Health { get; private set; } = 4;
        /// <summary>True once its health is gone.</summary>
        public bool Destroyed => Health <= 0;
        /// <summary>Size on screen in px.</summary>
        public static Size Size => new Size(60, 32);

        /// <summary>Takes one hit; health never goes below 0.</summary>
        public void Hit() => Health = Math.Max(0, Health - 1);
    }
}
