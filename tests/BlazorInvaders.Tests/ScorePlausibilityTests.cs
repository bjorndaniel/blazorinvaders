using BlazorInvaders.Api;
using BlazorInvaders.GameObjects;
using Xunit;

namespace BlazorInvaders.Tests;

public class ScorePlausibilityTests
{
    private static readonly DateOnly Day = new(2026, 10, 13);

    private static GameEngine Started(int wave = 1, DailyModifier? modifier = null)
    {
        var engine = new GameEngine(870, 500, new Random(1));
        if (modifier is { } m)
            engine.Start(0, wave, new DailyChallenge(Day, m));
        else
            engine.Start(0, wave);
        return engine;
    }

    [Fact]
    public void ValidatorScoring_MatchesEngine()
    {
        var grid = Started().Aliens.Sum(a => GameEngine.GetAlienPoints(a.Row));
        Assert.Equal(HighScoreValidator.GridPoints, grid);
        Assert.Equal(HighScoreValidator.MaxComboMultiplier, ComboTracker.MaxMultiplier);
        Assert.Equal(HighScoreValidator.BossHitPoints, GameEngine.BossHitPoints);
        Assert.Equal(HighScoreValidator.BossKillBonus, GameEngine.BossKillBonus);
        Assert.Equal(HighScoreValidator.MotherShipMaxPoints, GameEngine.MotherShipScoreTable.Max());
        Assert.Equal(HighScoreValidator.BossEvery, Started().BossEvery);
        Assert.Equal(HighScoreValidator.BossRushEvery, Started(3, DailyModifier.BossRush).BossEvery);

        Assert.Equal(HighScoreValidator.BossBaseHp, Started(5).Boss!.MaxHp);
        Assert.Equal(HighScoreValidator.BossBaseHp + HighScoreValidator.BossHpPerBoss, Started(10).Boss!.MaxHp);

        Assert.Equal(HighScoreValidator.PerfectWaveBonus, Started().PerfectWaveBonus);
        Assert.Equal(HighScoreValidator.PerfectWaveBonus * HighScoreValidator.DailyScoreFactor,
            Started(1, DailyModifier.Glass).PerfectWaveBonus);
    }

    [Fact]
    public void MaxPlausibleScore_GrowsWithWave()
    {
        // Below MaxScore: past it the bound stops growing
        for (var wave = 1; wave < 6; wave++)
        {
            Assert.True(HighScoreValidator.MaxPlausibleScore(wave + 1, false) > HighScoreValidator.MaxPlausibleScore(wave, false));
            Assert.True(HighScoreValidator.MaxPlausibleScore(wave + 1, true) > HighScoreValidator.MaxPlausibleScore(wave, true));
        }
    }

    [Fact]
    public void MaxPlausibleScore_BossWaveAtLeastBossMax()
    {
        // A boss wave's allowance is the larger of the alien and boss terms, and the alien term wins at
        // these waves (the boss term is below 64,460 until boss 20), so the boss term is pinned on its own:
        // 4x hits on a 30 HP boss plus the 1000 kill bonus and the 500 perfect-wave bonus
        Assert.Equal(7_500, HighScoreValidator.BossWaveMax(1));
        Assert.Equal(10_500, HighScoreValidator.BossWaveMax(2));
    }
}
