using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    /// <summary>A shot fired by the player; moves 20 px up per step.</summary>
    public class Shot
    {
        Sprite _beam;
        Point _currentPosition;
        /// <summary>Creates a shot with its top-left corner at <paramref name="start"/> (px).</summary>
        public Shot(Point start)
        {
            _beam = new Sprite(20, 60, 22, 66);
            _currentPosition = start;
        }
        /// <summary>The sprite to draw.</summary>
        public Sprite Sprite => _beam;
        /// <summary>Set when the shot should be dropped from the game.</summary>
        public bool Remove { get; set; }
        /// <summary>The trigger pull this shot belongs to, for combo tracking.</summary>
        public Volley Volley { get; init; } = new();
        /// <summary>Passes through aliens instead of stopping at the first one.</summary>
        public bool Piercing { get; init; }
        /// <summary>Top-left corner in game px.</summary>
        public Point CurrentPosition => _currentPosition;
        /// <summary>Moves 20 px up.</summary>
        public void Move() =>
            _currentPosition = new Point(_currentPosition.X, _currentPosition.Y - 20);
    }
}
