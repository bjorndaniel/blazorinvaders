using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class PowerUp
    {
        public PowerUp(Point position, PowerUpType type = PowerUpType.DoubleShot)
        {
            Position = position;
            Type = type;
        }

        public Point Position { get; private set; }
        public PowerUpType Type { get; }
        public bool Collected { get; set; }
        public static Size Size => new Size(16, 16);

        public void Move() =>
            Position = new Point(Position.X, Position.Y + 2);
    }
}
