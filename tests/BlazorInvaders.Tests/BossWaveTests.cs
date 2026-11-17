using System.Drawing;
using BlazorInvaders.GameObjects;
using Xunit;
using static BlazorInvaders.Tests.TestPlay;

namespace BlazorInvaders.Tests;

public class BossWaveTests
{
    [Fact]
    public void FifthWave_SpawnsBossInsteadOfAliens()
    {
        var engine = NewEngine();
        engine.Start(0, startWave: 5);

        Assert.True(engine.IsBossWave);
        Assert.Empty(engine.Aliens);
        var boss = Assert.IsType<Boss>(engine.Boss);
        Assert.Equal(30, boss.MaxHp);
        Assert.Equal(4, engine.Bunkers.Count);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    public void BossWaves_AreEveryFifth(int wave, bool boss)
    {
        var engine = NewEngine();
        engine.Start(0, startWave: wave);

        Assert.Equal(boss, engine.IsBossWave);
        Assert.Equal(boss, engine.Boss != null);
    }

    [Fact]
    public void LaterBosses_HaveMoreHealth()
    {
        var engine = NewEngine();
        engine.Start(0, startWave: 10);

        Assert.Equal(40, engine.Boss!.MaxHp);
    }

    [Fact]
    public void BossWave_IsNotClearedWhileBossLives()
    {
        var engine = NewEngine();
        engine.Start(0, startWave: 5);

        Run(engine, 0, 1000);

        Assert.Equal(5, engine.Wave);
        Assert.False(engine.Boss!.Defeated);
    }

    [Fact]
    public void ShotHittingBoss_DamagesItAndScores()
    {
        var engine = NewEngine();
        engine.Start(0, startWave: 5);

        engine.Fire();
        Run(engine, 0, 400);

        Assert.Equal(29, engine.Boss!.Hp);
        Assert.Equal(50, engine.Points);
        Assert.Contains(engine.TakeEvents(), e => e.Type == GameEventType.BossHit);
    }

    [Fact]
    public void DefeatingBoss_AwardsBonus_AndStartsNextWave()
    {
        var engine = NewEngine();
        engine.Start(0, startWave: 5);
        engine.Boss!.Hp = 1;

        TestPlay.ChaseAndShootBoss(engine);

        Assert.Equal(6, engine.Wave);
        Assert.False(engine.IsBossWave);
        Assert.Null(engine.Boss);
        Assert.Equal(55, engine.Aliens.Count);
        // 50 for the hit, 1000 for the first boss, 500 perfect-wave bonus
        Assert.Equal(1550, engine.Points);
        var killed = Assert.Single(engine.TakeEvents(), e => e.Type == GameEventType.BossKilled);
        Assert.Equal(1000, killed.Value);
    }

    [Fact]
    public void Boss_AttacksAfterBanner_AndRotatesPatterns()
    {
        var engine = NewEngine();
        engine.Start(0, startWave: 5);

        var t = Run(engine, 0, 1900);
        Assert.Empty(engine.Bombs);

        t = Run(engine, t, 150);
        Assert.Equal(5, engine.Bombs.Count);           // spread
        Assert.Equal(BossAttack.Rain, engine.Boss!.NextAttack);

        Run(engine, t, 1500);
        Assert.Equal(BossAttack.Aimed, engine.Boss.NextAttack);
    }

    [Fact]
    public void BossWave_HasNoMotherShip()
    {
        var engine = NewEngine();
        engine.Start(0, startWave: 5);

        Run(engine, 0, 3000);

        Assert.Null(engine.MotherShip);
    }
}

public class BossTests
{
    [Theory]
    [InlineData(30, 0)]
    [InlineData(21, 0)]
    [InlineData(20, 1)]
    [InlineData(11, 1)]
    [InlineData(10, 2)]
    [InlineData(1, 2)]
    public void Phase_DropsWithHealth(int hp, int phase)
    {
        var boss = new Boss(30, 400) { Hp = hp };

        Assert.Equal(phase, boss.Phase);
    }

    [Fact]
    public void LaterPhases_AttackMoreOften()
    {
        var boss = new Boss(30, 400);
        var calm = boss.AttackIntervalMs;

        boss.Hp = 5;

        Assert.True(boss.AttackIntervalMs < calm);
    }

    [Fact]
    public void Move_StaysOnScreen()
    {
        var boss = new Boss(30, 400);

        for (int i = 0; i < 2000; i++)
        {
            boss.Move(Width);
            Assert.InRange(boss.Position.X, 10, Width - 10 - Boss.Size.Width);
        }
    }

    [Fact]
    public void Hit_LowersHealth_AndDefeatsAtZero()
    {
        var boss = new Boss(2, 400);

        boss.Hit(100);
        Assert.Equal(1, boss.Hp);
        Assert.Equal(100, boss.LastHitTime);

        boss.Hit(200);
        Assert.True(boss.Defeated);
        Assert.False(boss.Collision(new Shot(boss.Position)));
    }

    [Fact]
    public void AimedAttack_SteersTowardPlayer()
    {
        var boss = new Boss(30, 100);
        boss.Attack(0, 450);    // spread
        boss.Attack(0, 450);    // rain

        var aimed = boss.Attack(playerCenterX: 800, playerY: 450);

        Assert.Equal(3, aimed.Count);
        foreach (var bomb in aimed)
        {
            var x = bomb.CurrentPosition.X;
            bomb.Move();
            Assert.True(bomb.CurrentPosition.X > x);
        }
    }

    [Fact]
    public void AngledBomb_MovesSideways()
    {
        var bomb = new Bomb(new Point(100, 100), BombType.Straight, dx: 3);

        bomb.Move();

        Assert.Equal(new Point(103, 110), bomb.CurrentPosition);
    }
}
