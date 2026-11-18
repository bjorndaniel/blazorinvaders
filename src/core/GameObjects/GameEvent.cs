namespace BlazorInvaders.GameObjects
{
    /// <summary>
    /// Something that happened at a point on screen during a step. Purely informational:
    /// the renderer turns these into particles and floating scores.
    /// </summary>
    /// <param name="Type">What happened.</param>
    /// <param name="X">Horizontal centre of the thing involved, in game px.</param>
    /// <param name="Y">Vertical centre of the thing involved, in game px.</param>
    /// <param name="Value">Points scored, if any.</param>
    /// <param name="Variant">Type-specific detail, e.g. the row of a killed alien.</param>
    public readonly record struct GameEvent(GameEventType Type, int X, int Y, int Value = 0, int Variant = 0);
}
