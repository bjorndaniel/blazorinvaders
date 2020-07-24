using Microsoft.AspNetCore.Components;
using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Alien
    {
        Sprite _still;
        Sprite _moving;
        ElementReference _spriteSheet;

        public Alien(AlienType type, Point startPosition, ElementReference spriteSheet)
        {
            _spriteSheet = spriteSheet;
            CurrentPosition = startPosition;
            (_still, _moving) = type switch
            {
                AlienType.Red =>
                    (new Sprite(0, 0, 20, 14), new Sprite(20, 0, 40, 14)),
                AlienType.Yellow =>
                    (new Sprite(20, 14, 40, 26), new Sprite(0, 14, 20, 26)),
                _ => (new Sprite(0, 27, 20, 40), new Sprite(20, 27, 40, 40)),
            };
        }
        //        var alien2Sprite = image.Rect(0, 14, 20, 26)
        //var alien2aSprite = image.Rect(20, 14, 40, 26)
        //var alien3Sprite = image.Rect(0, 27, 20, 40)
        //var alien3aSprite = image.Rect(20, 27, 40, 40)
        public bool Destroyed { get; set; }
        public bool Hit { get; set; }
        public bool Moving { get; set; }
        public Point CurrentPosition { get; set; }
        public Sprite Sprite
        {
            get
            {
                Moving = !Moving;
                return Moving ? _moving : _still;
            }
        }

    }
}
