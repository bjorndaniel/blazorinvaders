namespace BlazorInvaders.Rendering
{
    /// <summary>
    /// One frame of drawing instructions for the JS canvas renderer (wwwroot/js/game.js).
    /// <see cref="Cmds"/> is a flat stream of op codes and integer arguments; text, colours
    /// and fonts are indexes into <see cref="Strings"/>.
    /// </summary>
    public sealed class Scene
    {
        // Op codes and arity must match game.js
        public const int OpSprite = 1;   // sx, sy, sw, sh, dx, dy, dw, dh, tint (string index or -1)
        public const int OpRect = 2;     // x, y, w, h, color
        public const int OpText = 3;     // text, x, y, font, color, centered (0/1)
        public const int OpDiamond = 4;  // x, y, size, color
        public const int OpCircle = 5;   // x, y, radius, color, lineWidth (0 = filled)
        public const int OpEffects = 6;  // (no args) draw the particle/floating-text layer here

        // Ints per entry in Events: type (GameEventType), x, y, label (string index or -1), color (string index or -1)
        public const int EventStride = 5;

        public required int[] Cmds { get; init; }
        public required string[] Strings { get; init; }

        /// <summary>Game events since the last frame, <see cref="EventStride"/> ints each.</summary>
        public int[] Events { get; init; } = [];

        /// <summary>True while the game is paused, so effects freeze too.</summary>
        public bool Paused { get; init; }

        /// <summary><see cref="GameObjects.SoundEffect"/> flags to play this frame.</summary>
        public int Sounds { get; init; }

        /// <summary>New alien count for the march tempo, or -1 if unchanged.</summary>
        public int March { get; init; } = -1;
    }
}
