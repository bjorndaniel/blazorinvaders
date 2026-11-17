using BlazorInvaders.GameObjects;
using Xunit;
using static BlazorInvaders.Tests.TestPlay;

namespace BlazorInvaders.Tests;

public class DailyChallengeTests
{
    private static readonly DateOnly Day = new(2026, 10, 13);

    private static DailyChallenge With(DailyModifier modifier) => new(Day, modifier);

    /// <summary>Steps the engine, choosing the input for each step from its index.</summary>
    private static float Play(GameEngine engine, int steps, Func<int, GameInput> input, float fromMs = 0)
    {
        var t = fromMs;
        for (int i = 0; i < steps && !engine.GameOver; i++)
        {
            engine.SetInput(input(i));
            engine.Update(t += GameEngine.StepMs);
        }
        return t;
    }

    private static string Snapshot(GameEngine e) =>
        $"{e.Points}|{e.Lives}|{e.Wave}|{e.AliveCount}|{e.Player!.CurrentPosition}|{e.MotherShip?.CurrentPosition}|" +
        string.Join(",", e.Bombs.Select(b => b.CurrentPosition));

    [Fact]
    public void For_UsesTheDateAsSeedAndKey()
    {
        var daily = DailyChallenge.For(Day);

        Assert.Equal(20261013, daily.Seed);
        Assert.Equal("20261013", daily.Key);
        Assert.Equal(daily, DailyChallenge.For(Day));
    }

    [Fact]
    public void Modifiers_VaryFromDayToDay()
    {
        var modifiers = Enumerable.Range(0, 365)
            .Select(d => DailyChallenge.For(Day.AddDays(d)).Modifier)
            .Distinct();

        Assert.Equal(Enum.GetValues<DailyModifier>().Length, modifiers.Count());
    }

    [Fact]
    public void SameDailyAndSameInputs_PlayOutIdentically()
    {
        // Different engine seeds: only the daily seed may matter
        var a = new GameEngine(Width, Height, new Random(1));
        var b = new GameEngine(Width, Height, new Random(2));
        var daily = DailyChallenge.For(Day);
        a.Start(0, daily: daily);
        b.Start(0, daily: daily);

        // Sweep and fire for 20 seconds
        GameInput Script(int step) => GameInput.Fire | (step / 90 % 2 == 0 ? GameInput.Right : GameInput.Left);
        Play(a, 1200, Script);
        Play(b, 1200, Script);

        Assert.Equal(Snapshot(a), Snapshot(b));
        Assert.True(a.Points > 0);
    }

    [Fact]
    public void SameDaily_DifferentPlay_SharesTheMotherShipTimeline()
    {
        var idle = new GameEngine(Width, Height, new Random(1));
        var moving = new GameEngine(Width, Height, new Random(2));
        var daily = With(DailyModifier.None);
        idle.Start(0, daily: daily);
        moving.Start(0, daily: daily);
        // Keep both games alive; the bomb rolls are still drawn every step
        idle.AliensHoldFire = moving.AliensHoldFire = true;

        // Nobody shoots, so the mothership comes and goes the same way however the ship moves
        float ti = 0, tm = 0;
        for (int step = 0; step < 600; step++)
        {
            ti = Play(idle, 1, _ => GameInput.None, ti);
            tm = Play(moving, 1, _ => step / 40 % 2 == 0 ? GameInput.Left : GameInput.Right, tm);
            Assert.Equal(idle.MotherShip?.CurrentPosition, moving.MotherShip?.CurrentPosition);
        }
    }

    [Fact]
    public void DifferentDays_PlayDifferently()
    {
        var a = new GameEngine(Width, Height);
        var b = new GameEngine(Width, Height);
        a.Start(0, daily: With(DailyModifier.None));
        b.Start(0, daily: new DailyChallenge(Day.AddDays(1), DailyModifier.None));

        Play(a, 600, _ => GameInput.None);
        Play(b, 600, _ => GameInput.None);

        Assert.NotEqual(Snapshot(a), Snapshot(b));
    }

    [Fact]
    public void NormalGame_IsNotDaily()
    {
        var engine = new GameEngine(Width, Height);
        engine.Start(0, daily: With(DailyModifier.Glass));
        engine.Start(0);

        Assert.False(engine.IsDaily);
        Assert.Equal(3, engine.Lives);
        Assert.Equal(5, engine.BossEvery);
        Assert.Equal(4, engine.Bunkers.Count);
    }

    [Fact]
    public void NoBunkers_RemovesBunkers()
    {
        var engine = new GameEngine(Width, Height);
        engine.Start(0, daily: With(DailyModifier.NoBunkers));

        Assert.Empty(engine.Bunkers);
    }

    [Fact]
    public void ShieldStart_StartsWithShield()
    {
        var engine = new GameEngine(Width, Height);
        engine.Start(0, daily: With(DailyModifier.ShieldStart));

        Assert.True(engine.HasShield);
    }

    [Fact]
    public void BossRush_PutsABossOnEveryThirdWave()
    {
        var engine = new GameEngine(Width, Height);
        engine.Start(0, startWave: 3, daily: With(DailyModifier.BossRush));

        Assert.True(engine.IsBossWave);
        Assert.Equal(30, engine.Boss!.MaxHp);
    }

    [Fact]
    public void Glass_OneLifeDoublePoints()
    {
        var engine = NewEngine();
        engine.Start(0, daily: With(DailyModifier.Glass));
        engine.AliensHoldFire = true;

        engine.Fire();
        Play(engine, 30, _ => GameInput.None);

        Assert.Equal(1, engine.Lives);
        Assert.Equal(20, engine.Points);    // bottom-row alien, doubled
    }

    [Fact]
    public void Speedrun_AliensStartFaster()
    {
        var normal = new GameEngine(Width, Height);
        var fast = new GameEngine(Width, Height);
        normal.Start(0, daily: With(DailyModifier.None));
        fast.Start(0, daily: With(DailyModifier.Speedrun));
        normal.AliensHoldFire = fast.AliensHoldFire = true;

        // One second, before the grid reaches the edge and turns
        Play(normal, 60, _ => GameInput.None);
        Play(fast, 60, _ => GameInput.None);

        var travelled = (GameEngine e) => e.Aliens[0].CurrentPosition.X - 120;
        Assert.True(travelled(fast) > travelled(normal));
    }
}
