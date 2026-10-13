using BlazorInvaders.GameObjects;
using Xunit;

namespace BlazorInvaders.Tests;

public class GameEngineTests
{
    private const int Width = 870;
    private const int Height = 500;

    private static GameEngine NewEngine() => new(Width, Height, new Random(42));

    /// <summary>Feeds frame timestamps at <paramref name="hz"/> from <paramref name="fromMs"/> for <paramref name="durationMs"/>.</summary>
    private static float Run(GameEngine engine, float fromMs, float durationMs, float hz = 60)
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

    private static void RunUntilGameOver(GameEngine engine)
    {
        var t = 0f;
        while (!engine.GameOver && t < 180_000)
            t = Run(engine, t, 1000);
    }

    [Fact]
    public void Start_InitializesFirstWave()
    {
        var engine = NewEngine();
        engine.Start(0);

        Assert.True(engine.Started);
        Assert.False(engine.GameOver);
        Assert.Equal(1, engine.Wave);
        Assert.Equal(3, engine.Lives);
        Assert.Equal(0, engine.Points);
        Assert.Equal(55, engine.Aliens.Count);
        Assert.Equal(4, engine.Bunkers.Count);
        Assert.NotNull(engine.Player);
    }

    [Fact]
    public void Update_BeforeStart_DoesNotAdvance()
    {
        var engine = NewEngine();
        engine.Update(1000);

        Assert.False(engine.Started);
        Assert.Equal(0, engine.Time);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(144)]
    [InlineData(240)]
    public void GameClock_AdvancesInRealTime_AtAnyRefreshRate(int hz)
    {
        var engine = NewEngine();
        engine.Start(0);

        Run(engine, 0, 1000, hz);

        Assert.InRange(engine.Time, 1000 - GameEngine.StepMs - 0.01f, 1000 + 0.01f);
    }

    [Fact]
    public void Update_AfterLongGap_CatchesUpAtMostFiveSteps()
    {
        var engine = NewEngine();
        engine.Start(0);

        engine.Update(10_000);

        Assert.InRange(engine.Time, GameEngine.StepMs - 0.01f, 5 * GameEngine.StepMs + 0.01f);
    }

    [Fact]
    public void Pause_FreezesGameClock()
    {
        var engine = NewEngine();
        engine.Start(0);
        var t = Run(engine, 0, 100);
        var before = engine.Time;

        engine.TogglePause();
        engine.Update(t + 5000);
        Assert.Equal(before, engine.Time);

        engine.TogglePause();
        engine.Update(t + 5000 + GameEngine.StepMs);
        Assert.InRange(engine.Time, before, before + 2 * GameEngine.StepMs + 0.01f);
    }

    [Fact]
    public void Fire_AllowsOneShotInFlight()
    {
        var engine = NewEngine();
        engine.Start(0);

        engine.Fire();
        engine.Fire();

        Assert.Single(engine.Shots);
    }

    [Fact]
    public void HoldingRight_MovesPlayerAtConstantSpeed()
    {
        var engine = NewEngine();
        engine.Start(0);
        var startX = engine.Player!.CurrentPosition.X;

        engine.SetInput(GameInput.Right);
        Run(engine, 0, 500);

        // 30 steps at 6 px
        Assert.Equal(startX + 30 * GameEngine.PlayerSpeed, engine.Player.CurrentPosition.X);
    }

    [Fact]
    public void HoldingDirection_StopsAtScreenEdge()
    {
        var engine = NewEngine();
        engine.Start(0);

        engine.SetInput(GameInput.Right);
        var t = Run(engine, 0, 3000);
        Assert.InRange(engine.Player!.CurrentPosition.X, Width - 50 - GameEngine.PlayerSpeed, Width - 50);

        engine.SetInput(GameInput.Left);
        Run(engine, t, 3000);
        Assert.InRange(engine.Player.CurrentPosition.X, 10, 10 + GameEngine.PlayerSpeed);
    }

    [Fact]
    public void ReleasingInput_StopsPlayer()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.SetInput(GameInput.Left);
        var t = Run(engine, 0, 200);
        var x = engine.Player!.CurrentPosition.X;

        engine.SetInput(GameInput.None);
        Run(engine, t, 500);

        Assert.Equal(x, engine.Player.CurrentPosition.X);
    }

    [Fact]
    public void HoldingInput_WhilePaused_DoesNotMove()
    {
        var engine = NewEngine();
        engine.Start(0);
        var x = engine.Player!.CurrentPosition.X;

        engine.TogglePause();
        engine.SetInput(GameInput.Right | GameInput.Fire);
        Run(engine, 0, 1000);

        Assert.Equal(x, engine.Player.CurrentPosition.X);
        Assert.Empty(engine.Shots);
    }

    [Fact]
    public void HoldingFire_FiresAgainOnceShotIsSpent()
    {
        var engine = NewEngine();
        engine.Start(0);

        engine.SetInput(GameInput.Fire);
        var t = Run(engine, 0, 50);
        var first = Assert.Single(engine.Shots);

        // A shot is spent within half a second (top of screen or a hit)
        Run(engine, t, 1000);

        var current = Assert.Single(engine.Shots);
        Assert.NotSame(first, current);
    }

    [Fact]
    public void Shot_HitsBottomAlienInColumn_ScoresTenPoints()
    {
        var engine = NewEngine();
        engine.Start(0);

        engine.Fire();
        Run(engine, 0, 500);

        Assert.Equal(10, engine.Points);
        Assert.Equal(54, engine.AliveCount);
    }

    [Fact]
    public void ShotKillingAlien_RaisesAlienKilledEvent()
    {
        var engine = NewEngine();
        engine.Start(0);
        var shotX = engine.Player!.CurrentPosition.X + 13;

        engine.Fire();
        Run(engine, 0, 500);

        var kill = Assert.Single(engine.TakeEvents(), e => e.Type == GameEventType.AlienKilled);
        Assert.Equal(10, kill.Value);
        Assert.Equal(4, kill.Variant);                  // bottom row
        Assert.InRange(kill.X, shotX - 40, shotX + 40); // the alien above the ship
        Assert.Empty(engine.TakeEvents());
    }

    [Fact]
    public void LosingTheGame_RaisesPlayerHitEvent()
    {
        var engine = NewEngine();
        engine.Start(0);

        RunUntilGameOver(engine);

        Assert.Contains(engine.TakeEvents(), e => e.Type == GameEventType.PlayerHit);
    }

    [Fact]
    public void SpentShots_ArePruned()
    {
        var engine = NewEngine();
        engine.Start(0);

        engine.Fire();
        Run(engine, 0, 1000);

        Assert.Empty(engine.Shots);
    }

    [Theory]
    [InlineData(0, 0, 0, false)]     // nothing scored
    [InlineData(10, 0, 0, true)]     // empty board
    [InlineData(40, 9, 50, true)]    // board not full yet
    [InlineData(100, 10, 50, true)]  // beats the 10th place
    [InlineData(50, 10, 50, false)]  // ties the 10th place
    public void QualifiesForLeaderboard(int points, int entries, int lowest, bool expected)
    {
        var board = Enumerable.Range(0, entries)
            .Select(i => new HighScore { Name = "AAA", Score = lowest + (entries - 1 - i) * 10 })
            .ToList();

        Assert.Equal(expected, GameEngine.QualifiesForLeaderboard(points, board));
    }

    [Fact]
    public void GameOver_WithPoints_RaisesQualificationOnce()
    {
        var engine = NewEngine();
        var raised = 0;
        engine.QualifiedForLeaderboard += () => raised++;
        engine.Start(0);
        engine.Fire();

        RunUntilGameOver(engine);
        Run(engine, 180_000, 1000);

        Assert.True(engine.GameOver);
        Assert.False(engine.Started);
        Assert.Equal(1, raised);
        Assert.Equal(engine.Points, engine.HighScore);
    }

    [Fact]
    public void GameOver_WithoutPoints_DoesNotQualify()
    {
        var engine = NewEngine();
        var raised = 0;
        engine.QualifiedForLeaderboard += () => raised++;
        engine.Start(0);

        RunUntilGameOver(engine);

        Assert.True(engine.GameOver);
        Assert.Equal(0, raised);
    }

    [Fact]
    public void Start_AfterGameOver_ResetsGame()
    {
        var engine = NewEngine();
        engine.Start(0);
        RunUntilGameOver(engine);

        engine.Start(200_000);

        Assert.True(engine.Started);
        Assert.False(engine.GameOver);
        Assert.Equal(3, engine.Lives);
        Assert.Equal(0, engine.Points);
        Assert.Equal(55, engine.AliveCount);
    }

    [Fact]
    public void TakeSounds_ReturnsPendingSoundsAndClears()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.Fire();

        var sounds = engine.TakeSounds(out var march);
        Assert.True(sounds.HasFlag(SoundEffect.Shoot));
        Assert.Equal(55, march);

        Assert.Equal(SoundEffect.None, engine.TakeSounds(out march));
        Assert.Equal(-1, march);
    }

    [Fact]
    public void SetLeaderboard_TakesHighScoreFromTopEntry()
    {
        var engine = NewEngine();

        engine.SetLeaderboard(new[]
        {
            new HighScore { Name = "AAA", Score = 500 },
            new HighScore { Name = "BBB", Score = 100 },
        });

        Assert.Equal(500, engine.HighScore);
        Assert.Equal("AAA", engine.HighScoreName);
        Assert.Equal(2, engine.TopScores.Count);
    }
}
