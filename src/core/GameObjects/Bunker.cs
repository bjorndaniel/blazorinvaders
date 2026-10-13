using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Bunker
    {
        public Bunker(Point position)
        {
            Position = position;
        }

        public Point Position { get; }
        public int Health { get; private set; } = 4;
        public bool Destroyed => Health <= 0;
        public static Size Size => new Size(60, 32);

        public void Hit() => Health = Math.Max(0, Health - 1);
    }
}
