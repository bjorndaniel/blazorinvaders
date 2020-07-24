using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Game
    {
        List<Alien> _aliens;
        public void Start(ElementReference spriteSheet)
        {
            _aliens = new List<Alien>();
            for (int i = 0; i < 13; i++)
            {
                _aliens.Add(new Alien(AlienType.Red, new Point((i * 60 + 40), 40), spriteSheet));
                _aliens.Add(new Alien(AlienType.Yellow, new Point((i * 60 + 40), 80), spriteSheet));
                _aliens.Add(new Alien(AlienType.White, new Point((i * 60 + 40), 120), spriteSheet));
            }
            Started = true;
        }
        public bool Started { get; private set; }
        public IEnumerable<Alien> Aliens => _aliens;
    }
}
