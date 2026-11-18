using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    /// <summary>One alien in the grid: two sprite frames for the march, an explosion frame, and its place in the grid.</summary>
    public class Alien
    {
        Sprite _still;
        Sprite _moving;
        Sprite _current;
        Sprite _explosion;
        bool _isMoved;

        /// <summary>Creates an alien of <paramref name="type"/> at <paramref name="startPosition"/> (px), in grid position <paramref name="column"/> and <paramref name="row"/> (row 0 is the top).</summary>
        public Alien(AlienType type, Point startPosition, int column, int row)
        {
            CurrentPosition = startPosition;
            (_still, _moving) = type switch
            {
                AlienType.Crab =>
                    (new Sprite(0, 0, 20, 12), new Sprite(20, 0, 40, 14)),
                AlienType.Octopus =>
                    (new Sprite(20, 14, 40, 26), new Sprite(0, 12, 20, 27)),
                _ => (new Sprite(0, 27, 20, 41), new Sprite(20, 27, 40, 41)),
            };
            _current = _still;
            _explosion = new Sprite(0, 59, 17, 69);
            Column = column;
            Row = row;
        }

        /// <summary>Top-left corner in game px.</summary>
        public Point CurrentPosition { get; set; }

        /// <summary>The frame to draw: the explosion once hit, otherwise the current march frame.</summary>
        public Sprite Sprite => HasBeenHit ? _explosion : _current;

        /// <summary>Steps 20 px down.</summary>
        public void MoveDown() =>
            CurrentPosition = new Point { X = CurrentPosition.X, Y = CurrentPosition.Y + 20 };

        /// <summary>Steps 10 px sideways and swaps to the other march frame.</summary>
        public void MoveHorizontal(Direction direction)
        {
            var x = direction == Direction.Left ? CurrentPosition.X - 10 : CurrentPosition.X + 10;
            CurrentPosition = new Point { X = x, Y = CurrentPosition.Y };
            if (_isMoved)
            {
                _current = _still;
            }
            else
            {
                _current = _moving;
            }
            _isMoved = !_isMoved;
        }

        /// <summary>True once a shot has hit it; it then shows the explosion until it is destroyed.</summary>
        public bool HasBeenHit { get; set; }
        /// <summary>Game time in ms when it was hit, or -1 while unhit.</summary>
        public float HitTime { get; set; } = -1;

        /// <summary>Column in the grid, counted from the left.</summary>
        public int Column { get; private set; }

        /// <summary>Row in the grid, 0 at the top.</summary>
        public int Row { get; private set; }

        /// <summary>True once the explosion has been shown and the alien is gone for good.</summary>
        public bool Destroyed { get; set; }

        /// <summary>Tests the alien against a shot, sets <see cref="HasBeenHit"/> to the result and returns it.</summary>
        public bool Collision(Shot s)
        {
            HasBeenHit = new Rectangle(CurrentPosition, Sprite.RenderSize).IntersectsWith(new Rectangle(s.CurrentPosition, s.Sprite.RenderSize));
            return HasBeenHit;
        }
    }
}
