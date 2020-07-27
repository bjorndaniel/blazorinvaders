using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Alien
    {
        Sprite _still;
        Sprite _moving;
        Sprite _current;
        Sprite _explosion;
        bool _isMoved;
        
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

        public Point CurrentPosition { get; set; }

        public Sprite Sprite => HasBeenHit ? _explosion : _current;

        public void MoveDown() =>
            CurrentPosition = new Point { X = CurrentPosition.X, Y = CurrentPosition.Y + 20 };

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

        public bool HasBeenHit { get; set; }

        public int Column { get; private set; }

        public int Row { get; private set; }

        public bool Destroyed { get; set; }

        public bool Collision(Shot s)
        {
            HasBeenHit = new Rectangle(CurrentPosition, Sprite.RenderSize).IntersectsWith(new Rectangle(s.CurrentPosition, s.Sprite.RenderSize));
            return HasBeenHit;
        }
    }
} 
  