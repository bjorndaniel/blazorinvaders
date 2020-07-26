using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Bomb
    {
        Sprite _bomb; //TODO: Get movement sprite
        Point _currentPosition;
        public Bomb(Point start)
        {
            _bomb = new Sprite(0, 69, 5, 11);
            _currentPosition = start;
        }
        public Sprite Sprite => _bomb;
        public bool Remove { get; set; }
        public Point CurrentPosition => _currentPosition;
        public void Move() =>    
            _currentPosition = new Point(_currentPosition.X, _currentPosition.Y + 20);
    }
}