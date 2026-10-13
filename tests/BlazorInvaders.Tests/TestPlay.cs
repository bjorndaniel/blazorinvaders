using BlazorInvaders.GameObjects;

namespace BlazorInvaders.Tests;

/// <summary>Scripted play shared by tests.</summary>
internal static class TestPlay
{
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
