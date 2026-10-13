using BlazorInvaders.GameObjects;
using Xunit;

namespace BlazorInvaders.Tests;

public class PowerUpEngineTests
{
    private const int Width = 870;
    private const int Height = 500;

    private static GameEngine NewEngine() => new(Width, Height, new Random(42));

    /// <summary>Advances the engine in 60 Hz frames for <paramref name="durationMs"/> and returns the new time.</summary>
    private static float Run(GameEngine engine, float fromMs, float durationMs)
    {
        var frames = (int)Math.Round(durationMs / GameEngine.StepMs);
        for (int i = 1; i <= frames; i++)
            engine.Update(fromMs + i * GameEngine.StepMs);
        return fromMs + frames * GameEngine.StepMs;
    }

    /// <summary>Drops a power-up on the ship and lets it be collected.</summary>
    private static float Collect(GameEngine engine, PowerUpType type, float fromMs)
    {
        engine.SpawnPowerUp(type, engine.Player!.CurrentPosition);
        return Run(engine, fromMs, 2 * GameEngine.StepMs);
    }

    [Fact]
    public void Collecting_RaisesEventAndRemovesPowerUp()
    {
        var engine = NewEngine();
        engine.Start(0);

        Collect(engine, PowerUpType.RapidFire, 0);

        Assert.Empty(engine.PowerUps);
        var e = Assert.Single(engine.TakeEvents(), e => e.Type == GameEventType.PowerUpCollected);
        Assert.Equal((int)PowerUpType.RapidFire, e.Variant);
        Assert.True(engine.TakeSounds(out _).HasFlag(SoundEffect.PowerUp));
    }

    [Fact]
    public void DoubleShot_FiresTwoShots()
    {
        var engine = NewEngine();
        engine.Start(0);
        Collect(engine, PowerUpType.DoubleShot, 0);

        engine.Fire();

        Assert.Equal(2, engine.Shots.Count);
    }

    [Fact]
    public void RapidFire_KeepsThreeShotsInFlight()
    {
        var engine = NewEngine();
        engine.Start(0);
        // Right edge: no aliens above, so shots stay in flight
        engine.SetInput(GameInput.Right);
        var t = Run(engine, 0, 2000);
        engine.SetInput(GameInput.None);
        t = Collect(engine, PowerUpType.RapidFire, t);

        engine.Fire();
        engine.Fire();                      // too soon after the first
        Assert.Single(engine.Shots);

        t = Run(engine, t, 8 * GameEngine.StepMs);
        engine.Fire();
        t = Run(engine, t, 8 * GameEngine.StepMs);
        engine.Fire();

        Assert.Equal(3, engine.Shots.Count);
    }

    [Fact]
    public void Piercing_ShotPassesThroughTheWholeColumn()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.AliensHoldFire = true;       // a bomb hit would break the combo
        var t = Collect(engine, PowerUpType.Piercing, 0);

        engine.Fire();
        Run(engine, t, 600);

        Assert.Equal(50, engine.AliveCount);
        Assert.Equal(10 + 10 + 20 + 20 + 30, engine.Points);
        Assert.Equal(5, engine.Combo);      // one volley, five hits
    }

    [Fact]
    public void Shield_AbsorbsExactlyOneBombHit()
    {
        var engine = NewEngine();
        engine.Start(0);
        var t = Collect(engine, PowerUpType.Shield, 0);
        Assert.True(engine.HasShield);

        while (engine.HasShield && t < 60_000)
            t = Run(engine, t, 100);

        Assert.False(engine.HasShield);
        Assert.Equal(3, engine.Lives);
        Assert.False(engine.Player!.HasBeenHit);
        Assert.Contains(engine.TakeEvents(), e => e.Type == GameEventType.ShieldBlocked);

        while (engine.Lives == 3 && t < 120_000)
            t = Run(engine, t, 100);
        Assert.Equal(2, engine.Lives);
    }

    [Fact]
    public void ExtraLife_AddsALife_UpToTheCap()
    {
        var engine = NewEngine();
        engine.Start(0);

        var t = Collect(engine, PowerUpType.ExtraLife, 0);
        Assert.Equal(4, engine.Lives);

        t = Collect(engine, PowerUpType.ExtraLife, t);
        Collect(engine, PowerUpType.ExtraLife, t);
        Assert.Equal(GameEngine.MaxLives, engine.Lives);
    }

    [Fact]
    public void TimedPowerUp_RunsOutAfterTenSeconds()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.AliensHoldFire = true;       // the game must still be running after 10 s
        var t = Collect(engine, PowerUpType.RapidFire, 0);

        Assert.True(engine.IsActive(PowerUpType.RapidFire));
        Assert.InRange(engine.RemainingMs(PowerUpType.RapidFire), 9900, 10000);

        Run(engine, t, 10_000);
        Assert.False(engine.IsActive(PowerUpType.RapidFire));
        Assert.Equal(0, engine.RemainingMs(PowerUpType.RapidFire));
    }

    [Fact]
    public void Drops_IncludeEveryType()
    {
        var engine = NewEngine();
        engine.Start(0);

        var rolled = Enumerable.Range(0, 1000).Select(_ => engine.RollPowerUpType()).ToList();

        Assert.Equal(Enum.GetValues<PowerUpType>().OrderBy(x => x), rolled.Distinct().OrderBy(x => x));
        // Weighted: double shot (30) is far more common than extra life (5)
        Assert.True(rolled.Count(r => r == PowerUpType.DoubleShot) > 3 * rolled.Count(r => r == PowerUpType.ExtraLife));
    }

    [Fact]
    public void Drops_SkipExtraLife_AtMaxLives()
    {
        var engine = NewEngine();
        engine.Start(0);
        var t = Collect(engine, PowerUpType.ExtraLife, 0);
        Collect(engine, PowerUpType.ExtraLife, t);

        var rolled = Enumerable.Range(0, 1000).Select(_ => engine.RollPowerUpType());

        Assert.DoesNotContain(PowerUpType.ExtraLife, rolled);
    }
}
