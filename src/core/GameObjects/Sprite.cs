using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    /// <summary>A rectangle on the sprite sheet, in sheet px.</summary>
    public class Sprite
    {
        /// <summary>Creates a sprite from the sheet region with corners (<paramref name="x1"/>, <paramref name="y1"/>) and (<paramref name="x2"/>, <paramref name="y2"/>).</summary>
        public Sprite(int x1, int y1, int x2, int y2)
        {
            TopLeft = new Point(x1, y1);
            BottomRight = new Point(x2, y2);
        }
        /// <summary>Top-left corner on the sheet.</summary>
        public Point TopLeft { get; set; }
        /// <summary>Bottom-right corner on the sheet.</summary>
        public Point BottomRight { get; set; }
        /// <summary>Size on screen in px: twice the sheet size.</summary>
        public Size RenderSize => new Size((BottomRight.X - TopLeft.X) * 2, (BottomRight.Y - TopLeft.Y) * 2);
        /// <summary>Size on the sheet in px.</summary>
        public Size Size => new Size(BottomRight.X - TopLeft.X, BottomRight.Y - TopLeft.Y);
    }
}
