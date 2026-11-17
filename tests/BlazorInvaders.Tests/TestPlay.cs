using BlazorInvaders.GameObjects;

namespace BlazorInvaders.Tests;

/// <summary>Engine setup and scripted play shared by tests.</summary>
internal static class TestPlay
{
    /// <summary>Play-field width used by the tests.</summary>
    public const int Width = 870;

    /// <summary>Play-field height used by the tests.</summary>
    public const int Height = 500;

    /// <summary>Creates an engine on the test play field with a fixed seed.</summary>
    public static GameEngine NewEngine() => new(Width, Height, new Random(42));

    /// <summary>Feeds frame timestamps at <paramref name="hz"/> from <paramref name="fromMs"/> for <paramref name="durationMs"/>. Returns the last timestamp.</summary>
    public static float Run(GameEngine engine, float fromMs, float durationMs, float hz = 60)
    {
        var frameMs = 1000.0 / hz;
        var frames = (int)Math.Round(durationMs / frameMs);
        var t = fromMs;
        for (int i = 1; i <= frames; i++)
        {
            t = (float)(fromMs + i * frameMs);
            engine.Update(t);
        }
        return t;
    }

    /// <summary>Holds fire and steers under the boss until it is defeated. Returns the game time reached.</summary>
    public static float ChaseAndShootBoss(GameEngine engine, float fromMs = 0, float timeoutMs = 10_000)
    {
        var t = fromMs;
        while (engine.Boss is { Defeated: false } boss && t < fromMs + timeoutMs)
        {
            var target = boss.Position.X + Boss.Size.Width / 2 - 13;
            var x = engine.Player!.CurrentPosition.X;
            var steer = x < target - 6 ? GameInput.Right : x > target + 6 ? GameInput.Left : GameInput.None;
            engine.SetInput(steer | GameInput.Fire);
            engine.Update(t += GameEngine.StepMs);
        }
        engine.SetInput(GameInput.None);
        return t;
    }
}
