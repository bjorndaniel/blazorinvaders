using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorInvaders.GameObjects
{
    public abstract class Collidable
    {
        public bool HasBeenHit { get; private set; }
    }
}
