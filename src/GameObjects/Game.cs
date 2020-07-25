using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlazorInvaders.GameObjects
{
    public class Game
    {
        List<Alien> _aliens;
        double _alienSpeed = 0.5;
        float _lastUpdate = 0.0F;
        Direction _currentMovement;
        public void Start(ElementReference spriteSheet, GameTime time)
        {
            _aliens = new List<Alien>();
            for (int i = 0; i < 11; i++)
            {
                _aliens.Add(new Alien(AlienType.Squid, new Point((i * 60 + 120), 40), spriteSheet));
                _aliens.Add(new Alien(AlienType.Crab, new Point((i * 60 + 120), 85), spriteSheet));
                _aliens.Add(new Alien(AlienType.Crab, new Point((i * 60 + 120), 130), spriteSheet));
                _aliens.Add(new Alien(AlienType.Octopus, new Point((i * 60 + 120), 175), spriteSheet));
                _aliens.Add(new Alien(AlienType.Octopus, new Point((i * 60 + 120), 220), spriteSheet));
            }
            Started = true;
            _lastUpdate = time.TotalTime;
        }
        public void Update(GameTime time)
        {
            if (time.TotalTime - _lastUpdate > 500) 
            {
                _lastUpdate = time.TotalTime;
                if(_aliens.Any(_ => (_.CurrentPosition.X + 80) >= 870) && _currentMovement == Direction.Right)
                {
                    _aliens.ForEach(_ => _.MoveDown());
                    _currentMovement = Direction.Left;
                }
                else if(_aliens.Any(_ => _.CurrentPosition.X <= 15) && _currentMovement == Direction.Left)
                {
                    _aliens.ForEach(_ => _.MoveDown());
                    _currentMovement = Direction.Right;
                }
                else
                {
                    _aliens.ForEach(_ => _.MoveHorizontal(_currentMovement));
                }
            }
        }
        public bool Started { get; private set; }
        public IEnumerable<Alien> Aliens => _aliens;
    }
}
