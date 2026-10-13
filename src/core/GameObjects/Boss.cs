using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public enum BossAttack
    {
        Spread,     // a fan of angled bombs
        Rain,       // a row of straight bombs across its width
        Aimed,      // bombs steered toward the player
    }

    /// <summary>
    /// The boss on every 5th wave: a giant mothership that sweeps across the top, fires
    /// rotating attack patterns and gets faster as it takes damage.
    /// </summary>
    public class Boss
    {
        public const int Scale = 8;
        public const int Top = 90;

        // Reuses the mothership sprite, drawn 8x
        public static readonly Sprite ShipSprite = new(12, 73, 24, 80);
        public static Size Size => new(ShipSprite.Size.Width * Scale, ShipSprite.Size.Height * Scale);

        private int _direction = 1;

        public Boss(int maxHp, int x)
        {
            MaxHp = maxHp;
            Hp = maxHp;
            Position = new Point(x, Top);
        }

        public int MaxHp { get; }
        public int Hp { get; internal set; }
        public Point Position { get; private set; }
        public bool Defeated => Hp <= 0;
        public float LastHitTime { get; private set; } = float.NegativeInfinity;
        public BossAttack NextAttack { get; private set; } = BossAttack.Spread;

        /// <summary>0 above two thirds health, 1 above one third, 2 below.</summary>
        public int Phase => Hp * 3 > MaxHp * 2 ? 0 : Hp * 3 > MaxHp ? 1 : 2;

        /// <summary>Time between attacks for the current phase.</summary>
        public float AttackIntervalMs => Phase switch { 0 => 1500, 1 => 1100, _ => 750 };

        public Rectangle Bounds => new(Position, Size);

        /// <summary>Sweeps side to side within the screen, faster in later phases.</summary>
        public void Move(int screenWidth)
        {
            var speed = 2 + Phase * 2;
            var x = Position.X + _direction * speed;
            if (x <= 10 || x + Size.Width >= screenWidth - 10)
                _direction = -_direction;
            Position = new Point(Math.Clamp(x, 10, screenWidth - 10 - Size.Width), Position.Y);
        }

        public bool Collision(Shot s) =>
            !Defeated && Bounds.IntersectsWith(new Rectangle(s.CurrentPosition, s.Sprite.RenderSize));

        public void Hit(float time)
        {
            Hp = Math.Max(0, Hp - 1);
            LastHitTime = time;
        }

        /// <summary>Returns the bombs for the next attack and moves on to the following pattern.</summary>
        public List<Bomb> Attack(int playerCenterX, int playerY)
        {
            var bottom = Position.Y + Size.Height;
            var center = Position.X + Size.Width / 2;
            var bombs = new List<Bomb>();

            switch (NextAttack)
            {
                case BossAttack.Spread:
                    for (int dx = -4; dx <= 4; dx += 2)
                        bombs.Add(new Bomb(new Point(center, bottom), BombType.Straight, dx));
                    break;

                case BossAttack.Rain:
                    for (int i = 0; i < 6; i++)
                        bombs.Add(new Bomb(new Point(Position.X + 8 + i * (Size.Width - 16) / 5, bottom)));
                    break;

                case BossAttack.Aimed:
                    // Bombs fall 10 px per step; pick a sideways speed that lands near the player
                    var steps = Math.Max(1, (playerY - bottom) / 10);
                    foreach (var offset in new[] { -30, 0, 30 })
                    {
                        var dx = Math.Clamp((playerCenterX + offset - center) / steps, -8, 8);
                        bombs.Add(new Bomb(new Point(center, bottom), BombType.Straight, dx));
                    }
                    break;
            }

            NextAttack = (BossAttack)(((int)NextAttack + 1) % Enum.GetValues<BossAttack>().Length);
            return bombs;
        }
    }
}
