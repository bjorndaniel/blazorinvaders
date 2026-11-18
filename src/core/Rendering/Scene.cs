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
        /// <summary>Draws a region of the sprite sheet, optionally tinted.</summary>
        public const int OpSprite = 1;   // sx, sy, sw, sh, dx, dy, dw, dh, tint (string index or -1)
        /// <summary>Fills a rectangle.</summary>
        public const int OpRect = 2;     // x, y, w, h, color
        /// <summary>Draws a line of text.</summary>
        public const int OpText = 3;     // text, x, y, font, color, centered (0/1)
        /// <summary>Draws a filled diamond.</summary>
        public const int OpDiamond = 4;  // x, y, size, color
        /// <summary>Draws a circle, filled or as an outline.</summary>
        public const int OpCircle = 5;   // x, y, radius, color, lineWidth (0 = filled)
        /// <summary>Draws the particle and floating text layer at this point in the stream.</summary>
        public const int OpEffects = 6;  // (no args) draw the particle/floating-text layer here

        // Ints per entry in Events: type (GameEventType), x, y, label (string index or -1), color (string index or -1)
        /// <summary>Number of ints each entry in <see cref="Events"/> takes.</summary>
        public const int EventStride = 5;

        /// <summary>Op codes and their arguments, back to back.</summary>
        public required int[] Cmds { get; init; }
        /// <summary>Text, colours and fonts that <see cref="Cmds"/> refer to by index.</summary>
        public required string[] Strings { get; init; }

        /// <summary>Game events since the last frame, <see cref="EventStride"/> ints each.</summary>
        public int[] Events { get; init; } = [];

        /// <summary>True while the game is paused, so effects freeze too.</summary>
        public bool Paused { get; init; }

        /// <summary>All sound off; game.js silences its master gain. Reaches JS as muted.</summary>
        public bool Muted { get; init; }

        /// <summary>
        /// Smallest font size in logical px for floating labels of 18 px design size and up; 0 when not on a touch screen.
        /// Reaches JS as minTextSize; must match effects.js.
        /// </summary>
        public int MinTextSize { get; init; }

        /// <summary><see cref="GameObjects.SoundEffect"/> flags to play this frame.</summary>
        public int Sounds { get; init; }

        /// <summary>New alien count for the march tempo, or -1 if unchanged.</summary>
        public int March { get; init; } = -1;
    }
}
