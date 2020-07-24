using Microsoft.AspNetCore.Components;
using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Sprite
    {
        public Size Size { get; set; }
        public ElementReference SpriteSheet{ get; set; }
    }
}
