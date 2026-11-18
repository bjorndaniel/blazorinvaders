using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    /// <summary>A power-up falling from a killed alien.</summary>
    public class PowerUp
    {
        /// <summary>Creates a power-up of <paramref name="type"/> with its top-left corner at <paramref name="position"/> (px).</summary>
        public PowerUp(Point position, PowerUpType type = PowerUpType.DoubleShot)
        {
            Position = position;
            Type = type;
        }

        /// <summary>Top-left corner in game px.</summary>
        public Point Position { get; private set; }
        /// <summary>Which power-up it is.</summary>
        public PowerUpType Type { get; }
        /// <summary>Set when the player catches it or it falls off the screen; it is then dropped.</summary>
        public bool Collected { get; set; }
        /// <summary>Size on screen in px.</summary>
        public static Size Size => new Size(16, 16);

        /// <summary>Falls 2 px.</summary>
        public void Move() =>
            Position = new Point(Position.X, Position.Y + 2);
    }
}
