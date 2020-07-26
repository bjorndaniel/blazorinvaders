using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Sprite
    {
        public Sprite(int x1, int y1, int x2, int y2)
        {
            TopLeft = new Point(x1, y1);
            BottomRight = new Point(x2, y2);
        }
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }
        public Size RenderSize => new Size((BottomRight.X - TopLeft.X)  * 2, (BottomRight.Y - TopLeft.Y) * 2);
        public Size Size => new Size(BottomRight.X - TopLeft.X, BottomRight.Y - TopLeft.Y);
    }
}
