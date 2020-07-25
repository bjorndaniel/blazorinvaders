using Microsoft.AspNetCore.Components;
using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Alien
    {
        Sprite _still;
        Sprite _moving;
        bool _isMoved;
        ElementReference _spriteSheet;
        
        public Alien(AlienType type, Point startPosition, ElementReference spriteSheet)
        {
            _spriteSheet = spriteSheet;
            CurrentPosition = startPosition;
            (_still, _moving) = type switch
            {
                AlienType.Crab =>
                    (new Sprite(0, 0, 20, 14), new Sprite(20, 0, 40, 14)),
                AlienType.Octopus =>
                    (new Sprite(20, 14, 40, 26), new Sprite(0, 14, 20, 26)),
                _ => (new Sprite(0, 27, 20, 40), new Sprite(20, 27, 40, 40)),
            };
            Sprite = _still;
        }

        public bool Destroyed { get; set; }

        public bool Hit { get; set; }

        public Point CurrentPosition { get; set; }

        public Sprite Sprite { get; private set; }

        internal void MoveDown() =>
            CurrentPosition = new Point { X = CurrentPosition.X, Y = CurrentPosition.Y + 20 };

        internal void MoveHorizontal(Direction direction)
        {
            var x = direction == Direction.Left ? CurrentPosition.X - 10 : CurrentPosition.X + 10;
            CurrentPosition = new Point { X = x, Y = CurrentPosition.Y };
            if (_isMoved)
            {
                Sprite = _still;
            }
            else
            {
                Sprite = _moving;
            }
            _isMoved = !_isMoved;
        }
    }
            
}
