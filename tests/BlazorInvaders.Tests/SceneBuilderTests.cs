using BlazorInvaders.GameObjects;
using BlazorInvaders.Rendering;
using Xunit;
using static BlazorInvaders.Tests.TestPlay;

namespace BlazorInvaders.Tests;

public class SceneBuilderTests
{
    // Argument count per op code; must match game.js
    private static readonly Dictionary<int, int> Arity = new()
    {
        [Scene.OpSprite] = 9,
        [Scene.OpRect] = 5,
        [Scene.OpText] = 6,
        [Scene.OpDiamond] = 4,
        [Scene.OpCircle] = 5,
        [Scene.OpEffects] = 0,
    };

    private static List<(int op, int[] args)> Decode(Scene scene)
    {
        var commands = new List<(int, int[])>();
        for (int i = 0; i < scene.Cmds.Length;)
        {
            var op = scene.Cmds[i];
            Assert.True(Arity.TryGetValue(op, out var arity), $"Unknown op {op} at {i}");
            Assert.True(i + arity < scene.Cmds.Length, $"Truncated op {op} at {i}");
            commands.Add((op, scene.Cmds[(i + 1)..(i + 1 + arity)]));
            i += arity + 1;
        }
        return commands;
    }

    private static List<string> Texts(Scene scene) =>
        Decode(scene).Where(c => c.op == Scene.OpText).Select(c => scene.Strings[c.args[0]]).ToList();

    private static List<(string Text, int X, int Y, string Font, bool Centred)> TextCommands(Scene scene) =>
        Decode(scene).Where(c => c.op == Scene.OpText)
            .Select(c => (scene.Strings[c.args[0]], c.args[1], c.args[2], scene.Strings[c.args[3]], c.args[5] == 1))
            .ToList();

    /// <summary>Asserts a line is drawn centred: its anchor is the canvas centre (width / 2).</summary>
    private static (string Text, int X, int Y, string Font, bool Centred) AssertCentred(Scene scene, int width, string text)
    {
        var line = Assert.Single(TextCommands(scene), t => t.Text == text);
        Assert.True(line.Centred, $"'{text}' is not drawn centred");
        Assert.Equal(width / 2, line.X);
        return line;
    }

    private static GameEngine EngineOfWidth(int width) => new(width, Height, new Random(42));

    /// <summary>Attract states cycle every 4 s; the first build at 1 ms starts the cycle.</summary>
    private static Scene AttractState(SceneBuilder builder, GameEngine engine, int state)
    {
        float[] times = { 1, 4002, 8003, 12004 };
        Scene? scene = null;
        for (var i = 0; i <= state; i++)
            scene = builder.Build(engine, times[i]);
        return scene!;
    }

    /// <summary>Lets the ship sit still, one 60 Hz frame (about 16.7 ms) at a time, until <paramref name="done"/>. Returns the game time reached.</summary>
    private static float IdleUntil(GameEngine engine, Func<bool> done)
    {
        var t = 0f;
        while (!done() && t < 180_000)
            t = Run(engine, t, 16);
        return t;
    }

    [Fact]
    public void AttractScene_ShowsHeaderAndScoreTable()
    {
        var scene = new SceneBuilder().Build(NewEngine(), 0);

        var texts = Texts(scene);
        Assert.Contains(texts, t => t.StartsWith("Score: 00000"));
        Assert.Contains("SCORE ADVANCE TABLE", texts);
    }

    [Fact]
    public void PlayingScene_DrawsEveryAlienTinted()
    {
        var engine = NewEngine();
        engine.Start(0);

        var scene = new SceneBuilder().Build(engine, 0);

        var tinted = Decode(scene).Count(c => c.op == Scene.OpSprite && c.args[8] >= 0);
        Assert.Equal(55, tinted);
    }

    [Fact]
    public void AllStringReferences_AreInRange()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.TogglePause();

        var scene = new SceneBuilder().Build(engine, 0);

        foreach (var (op, args) in Decode(scene))
        {
            var refs = op switch
            {
                Scene.OpSprite => args[8] >= 0 ? new[] { args[8] } : Array.Empty<int>(),
                Scene.OpRect => new[] { args[4] },
                Scene.OpText => new[] { args[0], args[3], args[4] },
                Scene.OpDiamond => new[] { args[3] },
                Scene.OpCircle => new[] { args[3] },
                _ => Array.Empty<int>(),
            };
            Assert.All(refs, r => Assert.InRange(r, 0, scene.Strings.Length - 1));
        }
    }

    [Fact]
    public void PausedScene_ShowsPauseOverlay()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.TogglePause();

        var scene = new SceneBuilder().Build(engine, 0);

        Assert.Contains("PAUSED", Texts(scene));
    }

    [Theory]
    [InlineData(false, "Press P to resume", "Tap II to resume")]
    [InlineData(true, "Tap II to resume", "Press P to resume")]
    public void PausedScene_InstructionMatchesTheControls(bool touch, string expected, string other)
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.TogglePause();

        var scene = new SceneBuilder { TouchControls = touch }.Build(engine, 0);

        Assert.Contains(expected, Texts(scene));
        Assert.DoesNotContain(other, Texts(scene));
    }

    [Fact]
    public void Scene_MarksEffectsLayerOnce()
    {
        var engine = NewEngine();
        var builder = new SceneBuilder();

        Assert.Single(Decode(builder.Build(engine, 0)), c => c.op == Scene.OpEffects);
        engine.Start(0);
        Assert.Single(Decode(builder.Build(engine, 0)), c => c.op == Scene.OpEffects);
    }

    [Fact]
    public void AlienKill_IsEncodedWithScoreLabelAndRowColour()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.Fire();
        Run(engine, 0, 500);

        var scene = new SceneBuilder().Build(engine, 500);

        Assert.Equal(0, scene.Events.Length % Scene.EventStride);
        var kill = Enumerable.Range(0, scene.Events.Length / Scene.EventStride)
            .Select(i => scene.Events[(i * Scene.EventStride)..((i + 1) * Scene.EventStride)])
            .Single(e => e[0] == (int)GameEventType.AlienKilled);
        Assert.Equal("+10", scene.Strings[kill[3]]);
        Assert.Equal("#78ff3c", scene.Strings[kill[4]]);   // bottom row tint
    }

    [Fact]
    public void Events_AreSentOnlyOnce()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.Fire();
        Run(engine, 0, 500);
        var builder = new SceneBuilder();

        Assert.NotEmpty(builder.Build(engine, 500).Events);
        Assert.Empty(builder.Build(engine, 500).Events);
    }

    [Fact]
    public void Scene_ReportsPaused()
    {
        var engine = NewEngine();
        engine.Start(0);
        var builder = new SceneBuilder();

        Assert.False(builder.Build(engine, 0).Paused);
        engine.TogglePause();
        Assert.True(builder.Build(engine, 0).Paused);
    }

    [Fact]
    public void Hud_ShowsComboOnlyAboveX1()
    {
        var engine = NewEngine();
        engine.Start(0);
        var builder = new SceneBuilder();

        Assert.DoesNotContain(Texts(builder.Build(engine, 0)), t => t.StartsWith("COMBO"));
        engine.ComboTracker.Count = 10;
        Assert.Contains("COMBO x3", Texts(builder.Build(engine, 0)));
    }

    [Fact]
    public void Hud_ListsActivePowerUps_AndShieldRing()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.SpawnPowerUp(PowerUpType.RapidFire, engine.Player!.CurrentPosition);
        engine.SpawnPowerUp(PowerUpType.Shield, engine.Player.CurrentPosition);
        Run(engine, 0, 40);

        var scene = new SceneBuilder().Build(engine, 40);

        var texts = Texts(scene);
        Assert.Contains("RAPID 10", texts);
        Assert.Contains("SHIELD", texts);
        Assert.Single(Decode(scene), c => c.op == Scene.OpCircle);
    }

    [Fact]
    public void FallingPowerUp_IsDrawnInItsColourWithLetter()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.SpawnPowerUp(PowerUpType.Piercing, new System.Drawing.Point(100, 200));

        var scene = new SceneBuilder().Build(engine, 0);

        var diamond = Assert.Single(Decode(scene), c => c.op == Scene.OpDiamond);
        Assert.Equal("#FF40FF", scene.Strings[diamond.args[3]]);
        Assert.Contains("P", Texts(scene));
    }

    [Fact]
    public void BossWave_ShowsBannerBossAndHealthBar()
    {
        var engine = NewEngine();
        engine.Start(0, startWave: 5);
        engine.Boss!.Hp = 15;

        var scene = new SceneBuilder().Build(engine, 0);
        var commands = Decode(scene);

        Assert.Contains("BOSS WAVE", Texts(scene));
        Assert.Contains("BOSS", Texts(scene));
        Assert.Single(commands, c => c.op == Scene.OpSprite && c.args[6] == Boss.Size.Width);
        // Health bar: full-width background and a half-width fill
        Assert.Contains(commands, c => c.op == Scene.OpRect && c.args[2] == 300);
        Assert.Contains(commands, c => c.op == Scene.OpRect && c.args[2] == 150);
    }

    [Fact]
    public void BossKill_IsEncodedWithBonusLabel()
    {
        var engine = NewEngine();
        engine.Start(0, startWave: 5);
        engine.Boss!.Hp = 1;
        var t = TestPlay.ChaseAndShootBoss(engine);

        var scene = new SceneBuilder().Build(engine, t);

        var labels = Enumerable.Range(0, scene.Events.Length / Scene.EventStride)
            .Where(i => scene.Events[i * Scene.EventStride] == (int)GameEventType.BossKilled)
            .Select(i => scene.Strings[scene.Events[i * Scene.EventStride + 3]]);
        Assert.Equal(new[] { "BOSS DOWN +1000" }, labels);
    }

    [Theory]
    [InlineData(false, "Press space to start")]
    [InlineData(true, "Tap the screen to start")]
    public void AttractInstructions_MatchTheControls(bool touch, string expected)
    {
        var engine = NewEngine();
        var builder = new SceneBuilder { TouchControls = touch };

        // The controls card is the third attract state, 4 s apart
        builder.Build(engine, 1);
        builder.Build(engine, 4002);
        var scene = builder.Build(engine, 8003);

        Assert.Contains(expected, Texts(scene));
    }

    [Theory]
    [InlineData(false, "P / Escape to pause  ·  M toggles sound")]
    [InlineData(true, "II to pause  ·  M toggles sound")]
    public void AttractInstructions_NameTheMuteKey(bool touch, string expected)
    {
        var engine = NewEngine();
        var builder = new SceneBuilder { TouchControls = touch };

        builder.Build(engine, 1);
        builder.Build(engine, 4002);
        var scene = builder.Build(engine, 8003);

        Assert.Contains(expected, Texts(scene));
    }

    [Theory]
    [InlineData("attract")]
    [InlineData("playing")]
    [InlineData("paused")]
    public void MutedScene_ShowsSoundOffAndReportsMuted(string state)
    {
        var engine = NewEngine();
        if (state != "attract")
            engine.Start(0);
        if (state == "paused")
            engine.TogglePause();
        var builder = new SceneBuilder { Sound = SoundSetting.FromStored("true") };

        var scene = builder.Build(engine, 0);

        Assert.Contains("SOUND OFF", Texts(scene));
        Assert.True(scene.Muted);
    }

    [Fact]
    public void UnmutedScene_HasNoSoundOff()
    {
        var scene = new SceneBuilder().Build(NewEngine(), 0);

        Assert.DoesNotContain("SOUND OFF", Texts(scene));
        Assert.False(scene.Muted);
    }

    [Fact]
    public void DailyGame_ShowsChallengeBannerAndHudTag()
    {
        var engine = NewEngine();
        engine.Start(0, daily: new DailyChallenge(new DateOnly(2026, 10, 13), DailyModifier.NoBunkers));

        var texts = Texts(new SceneBuilder().Build(engine, 0));

        Assert.Contains("DAILY CHALLENGE", texts);
        Assert.Contains("2026-10-13  ·  NO BUNKERS", texts);
        Assert.Contains("DAILY  ·  NO BUNKERS", texts);
    }

    [Fact]
    public void Attract_CyclesToTodaysChallengeCard()
    {
        var engine = NewEngine();
        var builder = new SceneBuilder
        {
            TodaysChallenge = new DailyChallenge(new DateOnly(2026, 10, 13), DailyModifier.Glass),
        };

        // Fourth card, 4 s apart
        for (var t = 1; t < 12_010; t += 4001)
            builder.Build(engine, t);
        var texts = Texts(builder.Build(engine, 12_004));

        Assert.Contains("DAILY CHALLENGE", texts);
        Assert.Contains("ONE LIFE, DOUBLE POINTS", texts);
        Assert.Contains("Press D to play", texts);
    }

    [Fact]
    public void Scene_CarriesPendingSounds()
    {
        var engine = NewEngine();
        engine.Start(0);
        engine.Fire();

        var scene = new SceneBuilder().Build(engine, 0);

        Assert.Equal(55, scene.March);
        Assert.True(((SoundEffect)scene.Sounds).HasFlag(SoundEffect.Shoot));
    }

    // Centring: every centred message is drawn at width / 2 (BUG-001). Widths 870 and an odd 801.

    [Theory]
    [InlineData(870, false)]
    [InlineData(801, false)]
    [InlineData(870, true)]
    [InlineData(801, true)]
    public void StartScreen_LinesAreCentred_WithUnchangedTextFontAndY(int width, bool touch)
    {
        var lines = touch
            ? new[] { ("Use the arrow buttons to move, FIRE to shoot", 160, "26px Orbitron"), ("Tap the screen to start", 220, "28px Orbitron"), ("II to pause  ·  M toggles sound", 265, "18px Orbitron") }
            : new[] { ("Use arrow keys to move, space to fire", 160, "28px Orbitron"), ("Press space to start", 220, "28px Orbitron"), ("P / Escape to pause  ·  M toggles sound", 265, "18px Orbitron") };
        var scene = AttractState(new SceneBuilder { TouchControls = touch }, EngineOfWidth(width), 2);

        foreach (var (text, y, font) in lines)
        {
            var line = AssertCentred(scene, width, text);
            Assert.Equal(y, line.Y);
            Assert.Equal(font, line.Font);
        }
    }

    [Theory]
    [InlineData(870)]
    [InlineData(801)]
    public void ScoreTable_TitleIsCentred(int width)
    {
        var scene = AttractState(new SceneBuilder(), EngineOfWidth(width), 0);

        AssertCentred(scene, width, "SCORE ADVANCE TABLE");
    }

    [Theory]
    [InlineData(870)]
    [InlineData(801)]
    public void Leaderboard_LinesAreCentred(int width)
    {
        var engine = EngineOfWidth(width);
        engine.SetLeaderboard(new[] { new HighScore { Name = "ACE", Score = 1000 }, new HighScore { Name = "BOB", Score = 500 } });

        var scene = AttractState(new SceneBuilder(), engine, 1);

        AssertCentred(scene, width, "TOP SCORES");
        AssertCentred(scene, width, "1.  ACE   01000");
        AssertCentred(scene, width, "2.  BOB   00500");
    }

    [Theory]
    [InlineData(870)]
    [InlineData(801)]
    public void EmptyLeaderboard_SaysNoScoresYet_Centred(int width)
    {
        var scene = AttractState(new SceneBuilder(), EngineOfWidth(width), 1);

        AssertCentred(scene, width, "NO SCORES YET");
    }

    [Theory]
    [InlineData(870, false)]
    [InlineData(801, false)]
    [InlineData(870, true)]
    [InlineData(801, true)]
    public void DailyAttractCard_LinesAreCentred(int width, bool touch)
    {
        var builder = new SceneBuilder
        {
            TouchControls = touch,
            TodaysChallenge = new DailyChallenge(new DateOnly(2026, 10, 13), DailyModifier.Glass),
        };
        var scene = AttractState(builder, EngineOfWidth(width), 3);

        AssertCentred(scene, width, "DAILY CHALLENGE");
        AssertCentred(scene, width, "2026-10-13");
        AssertCentred(scene, width, "ONE LIFE, DOUBLE POINTS");
        AssertCentred(scene, width, "Same game for everyone today");
        AssertCentred(scene, width, touch ? "Tap D to play" : "Press D to play");
    }

    [Theory]
    [InlineData(870)]
    [InlineData(801)]
    public void WaveBanner_IsCentred(int width)
    {
        var engine = EngineOfWidth(width);
        engine.Start(0);

        AssertCentred(new SceneBuilder().Build(engine, 0), width, "WAVE 1");
    }

    [Theory]
    [InlineData(870)]
    [InlineData(801)]
    public void BossBanner_IsCentred(int width)
    {
        var engine = EngineOfWidth(width);
        engine.Start(0, startWave: 5);

        AssertCentred(new SceneBuilder().Build(engine, 0), width, "BOSS WAVE");
    }

    [Theory]
    [InlineData(870)]
    [InlineData(801)]
    public void DailyBanner_AndHudTag_AreCentred(int width)
    {
        var engine = EngineOfWidth(width);
        engine.Start(0, daily: new DailyChallenge(new DateOnly(2026, 10, 13), DailyModifier.NoBunkers));

        var scene = new SceneBuilder().Build(engine, 0);

        AssertCentred(scene, width, "DAILY CHALLENGE");
        AssertCentred(scene, width, "2026-10-13  ·  NO BUNKERS");
        AssertCentred(scene, width, "DAILY  ·  NO BUNKERS");
    }

    [Theory]
    [InlineData(870)]
    [InlineData(801)]
    public void PauseOverlay_IsCentred(int width)
    {
        var engine = EngineOfWidth(width);
        engine.Start(0);
        engine.TogglePause();

        var scene = new SceneBuilder().Build(engine, 0);

        AssertCentred(scene, width, "PAUSED");
        AssertCentred(scene, width, "Press P to resume");
    }

    [Theory]
    [InlineData(870, false)]
    [InlineData(801, false)]
    [InlineData(870, true)]
    [InlineData(801, true)]
    public void GameOverScreen_IsCentred(int width, bool touch)
    {
        var engine = EngineOfWidth(width);
        engine.Start(0);
        var t = IdleUntil(engine, () => engine.GameOver);
        Assert.True(engine.GameOver);

        var scene = new SceneBuilder { TouchControls = touch }.Build(engine, t);

        AssertCentred(scene, width, "GAME OVER");
        AssertCentred(scene, width, touch ? "Tap to start a new game" : "Press space to start a new game");
    }

    [Theory]
    [InlineData(870)]
    [InlineData(801)]
    public void GameOverScreen_DailyHint_IsCentred(int width)
    {
        var engine = EngineOfWidth(width);
        engine.Start(0);
        var t = IdleUntil(engine, () => engine.GameOver);
        Assert.True(engine.GameOver);

        var scene = new SceneBuilder { TodaysChallenge = new DailyChallenge(new DateOnly(2026, 10, 13), DailyModifier.Glass) }.Build(engine, t);

        AssertCentred(scene, width, "D for today's challenge");
    }

    [Theory]
    [InlineData(870)]
    [InlineData(801)]
    public void YouWereHit_IsCentred(int width)
    {
        var engine = EngineOfWidth(width);
        engine.Start(0);
        var t = IdleUntil(engine, () => engine.LostALife);
        Assert.True(engine.LostALife);

        AssertCentred(new SceneBuilder().Build(engine, t), width, "You were hit!");
    }

    [Theory]
    [InlineData(870)]
    [InlineData(801)]
    public void PerfectBonus_IsCentred(int width)
    {
        var engine = EngineOfWidth(width);
        engine.Start(0, startWave: 5);
        engine.Boss!.Hp = 1;
        var t = ChaseAndShootBoss(engine);
        Assert.True(engine.ShowPerfectBonus);

        AssertCentred(new SceneBuilder().Build(engine, t), width, $"PERFECT!  +{engine.PerfectWaveBonus}");
    }

    // ---- Larger phone canvas text (FEAT-002) ----

    private static readonly DailyChallenge TestDaily = new(new DateOnly(2026, 10, 13), DailyModifier.Glass);

    private record TextBox(string Text, int Size, double Left, double Top, double Right, double Bottom);

    private static List<TextBox> Boxes(Scene scene)
    {
        var boxes = new List<TextBox>();
        foreach (var (_, a) in Decode(scene).Where(c => c.op == Scene.OpText))
        {
            var text = scene.Strings[a[0]];
            var size = int.Parse(scene.Strings[a[3]].Split("px")[0]);
            var w = SceneBuilder.EstimateWidth(text, size);
            var left = a[5] == 1 ? a[1] - w / 2 : a[1];
            var top = a[2] - 0.77 * size;
            var bottom = a[2] + (text.IndexOfAny("gjpqy,;".ToCharArray()) >= 0 ? 0.243 * size : 0);
            boxes.Add(new TextBox(text, size, left, top, left + w, bottom));
        }
        return boxes;
    }

    private static bool Intersects(TextBox a, TextBox b) =>
        a.Left < b.Right && b.Left < a.Right && a.Top < b.Bottom && b.Top < a.Bottom;

    /// <summary>A started game with a 5-digit Hi score and a combo, the widest HUD text.</summary>
    private static GameEngine CrowdedHudEngine()
    {
        var hud = NewEngine();
        hud.SetLeaderboard(new[] { new HighScore { Name = "WWW", Score = 99999 } });
        hud.Start(0);
        hud.ComboTracker.Count = 20;
        return hud;
    }

    /// <summary>Scenes covering every screen and the crowded HUD states, built at <paramref name="scale"/>.</summary>
    private static List<(string Name, Scene Scene)> PhoneScenes(float scale, bool touch)
    {
        SceneBuilder NewBuilder(bool muted = false)
        {
            var b = new SceneBuilder { TouchControls = touch, DisplayScale = scale, TodaysChallenge = TestDaily };
            if (muted) b.Sound.Toggle();
            return b;
        }

        var scenes = new List<(string, Scene)>();

        var attractEngine = NewEngine();
        for (var i = 0; i < 4; i++)
            scenes.Add(($"attract {i}", AttractState(NewBuilder(), attractEngine, i)));
        scenes.Add(("attract muted", NewBuilder(muted: true).Build(attractEngine, 0)));

        var hud = CrowdedHudEngine();
        foreach (var alien in hud.Aliens.Skip(1))
            alien.HasBeenHit = true;
        Assert.True(hud.IsFrenzy);
        scenes.Add(("hud combo frenzy muted", NewBuilder(muted: true).Build(hud, 100)));

        var banner = NewEngine();
        banner.Start(0);
        scenes.Add(("wave banner", NewBuilder().Build(banner, 0)));

        var daily = NewEngine();
        daily.Start(0, daily: TestDaily);
        scenes.Add(("daily banner and tag", NewBuilder().Build(daily, 0)));

        var boss = NewEngine();
        boss.Start(0, startWave: 5);
        var t = IdleUntil(boss, () => boss.LostALife);
        Assert.True(boss.LostALife);
        scenes.Add(("boss wave lost a life muted", NewBuilder(muted: true).Build(boss, t)));

        var perfect = NewEngine();
        perfect.Start(0, startWave: 5);
        perfect.Boss!.Hp = 1;
        t = ChaseAndShootBoss(perfect);
        Assert.True(perfect.ShowPerfectBonus);
        scenes.Add(("perfect", NewBuilder().Build(perfect, t)));

        var over = NewEngine();
        over.Start(0, daily: TestDaily);
        t = IdleUntil(over, () => over.GameOver);
        Assert.True(over.GameOver);
        scenes.Add(("game over", NewBuilder().Build(over, t)));

        var paused = NewEngine();
        paused.Start(0);
        t = Run(paused, 0, 4000);
        paused.TogglePause();
        scenes.Add(("paused muted", NewBuilder(muted: true).Build(paused, t)));

        return scenes;
    }

    [Theory]
    [InlineData(18, 0.41f, 30)]
    [InlineData(18, 0.445f, 27)]
    [InlineData(18, 1.0f, 18)]
    [InlineData(36, 0.41f, 36)]
    [InlineData(14, 0.41f, 14)]
    [InlineData(10, 0.41f, 10)]
    [InlineData(18, 0f, 18)]
    [InlineData(18, -1f, 18)]
    [InlineData(18, float.NaN, 18)]
    [InlineData(18, 0.01f, 60)]
    public void PhoneFontSize_RaisesMustReadTextToTheFloor(int design, float scale, int expected) =>
        Assert.Equal(expected, SceneBuilder.PhoneFontSize(design, scale));

    [Theory]
    [InlineData(0.41f)]
    [InlineData(0.445f)]
    public void Phone_MustReadTextReachesTheFloor_AndFloorIsReported(float scale)
    {
        var floor = (int)Math.Ceiling(12 / scale);
        foreach (var (name, scene) in PhoneScenes(scale, touch: true))
        {
            Assert.Equal(floor, scene.MinTextSize);
            foreach (var box in Boxes(scene).Where(b => b.Size >= 18))
                Assert.True(box.Size >= floor, $"{name}: '{box.Text}' is {box.Size} px, floor {floor}");
        }
    }

    [Fact]
    public void EstimateWidth_MatchesTheMeasuredFont()
    {
        Assert.InRange(SceneBuilder.EstimateWidth("Use the arrow buttons to move, FIRE to shoot", 30), 753, 800);
        Assert.InRange(SceneBuilder.EstimateWidth("COMBO x4!", 30), 185, 196);
        Assert.InRange(SceneBuilder.EstimateWidth("Score: 99999   Hi: 99999 WWW", 30), 551, 580);
    }

    [Theory]
    [InlineData(0.41f)]
    [InlineData(0.445f)]
    public void Phone_TextStaysOnCanvas_AndDoesNotOverlap(float scale)
    {
        foreach (var (name, scene) in PhoneScenes(scale, touch: true))
        {
            var boxes = Boxes(scene);
            foreach (var b in boxes)
                Assert.True(b.Left >= 0 && b.Right <= Width && b.Top >= 0 && b.Bottom <= Height, $"{name}: '{b.Text}' leaves the canvas");

            for (var i = 0; i < boxes.Count; i++)
                for (var j = i + 1; j < boxes.Count; j++)
                    if (boxes[i].Size >= 18 || boxes[j].Size >= 18)
                        Assert.False(Intersects(boxes[i], boxes[j]), $"{name}: '{boxes[i].Text}' overlaps '{boxes[j].Text}'");

            // The boss bar (x 285..585, y 36..44) is a rect, not text
            if (name.StartsWith("boss"))
                foreach (var b in boxes.Where(b => b.Size >= 18))
                    Assert.False(b.Left < 585 && 285 < b.Right && b.Top < 44 && 36 < b.Bottom, $"{name}: '{b.Text}' covers the boss bar");

            if (name == "daily banner and tag")
            {
                // The subtitle sits inside its backdrop: y 200..290
                var sub = boxes.Single(b => b.Text.StartsWith("2026-10-13"));
                Assert.InRange(sub.Top, Height / 2 - 50, Height / 2 + 40);
                Assert.InRange(sub.Bottom, Height / 2 - 50, Height / 2 + 40);
            }
        }
    }

    [Fact]
    public void Phone_CentredLinesKeepTheCentre()
    {
        foreach (var (_, scene) in PhoneScenes(0.41f, touch: true))
            foreach (var line in TextCommands(scene).Where(t => t.Centred))
                Assert.Equal(Width / 2, line.X);
    }

    [Theory]
    [InlineData(30, 30, false, 61, 97)]
    [InlineData(30, 30, true, 72, 108)]
    [InlineData(27, 28, false, 58, 91)]
    [InlineData(27, 28, true, 69, 102)]
    public void HitAndPerfectBaselines_StackWhenEnlarged(int hitSize, int perfectSize, bool bossWave, int hit, int perfect)
    {
        var hitBaseline = SceneBuilder.HitBaseline(hitSize, bossWave);
        Assert.Equal(hit, hitBaseline);
        var perfectBaseline = SceneBuilder.PerfectBaseline(hitBaseline, hitSize, perfectSize);
        Assert.Equal(perfect, perfectBaseline);
        Assert.True(perfectBaseline - 0.77 * perfectSize > hitBaseline + 0.243 * hitSize);
    }

    [Fact]
    public void HitAndPerfectBaselines_KeepDesignPositionsAtDesignSize()
    {
        Assert.Equal(55, SceneBuilder.HitBaseline(20, false));
        Assert.Equal(55, SceneBuilder.HitBaseline(20, true));
        Assert.Equal(80, SceneBuilder.PerfectBaseline(55, 20, 28));
    }

    [Fact]
    public void Phone_FitCap_ShrinksLongLinesToTheCanvas()
    {
        var builder = new SceneBuilder { TouchControls = true, DisplayScale = 0.30f };
        var scene = AttractState(builder, EngineOfWidth(Width), 2);
        Assert.Equal(40, scene.MinTextSize);
        var line = Boxes(scene).Single(b => b.Text.StartsWith("Use the arrow"));
        Assert.InRange(line.Size, 26, 39);
        Assert.True(line.Right - line.Left <= Width - 20);

        var hud = CrowdedHudEngine();
        var muted = new SceneBuilder { TouchControls = true, DisplayScale = 0.30f };
        muted.Sound.Toggle();
        var boxes = Boxes(muted.Build(hud, 100));
        var header = boxes.Single(b => b.Text.StartsWith("Score:"));
        var combo = boxes.Single(b => b.Text.StartsWith("COMBO"));
        Assert.True(header.Right <= combo.Left);
        Assert.True(combo.Right <= Width);
        Assert.False(Intersects(combo, boxes.Single(b => b.Text == "SOUND OFF")));
    }

    private static void SetLostALife(GameEngine engine, bool value) =>
        typeof(GameEngine).GetProperty(nameof(GameEngine.LostALife))!.SetValue(engine, value);

    private static GameEngine PerfectEngine(out float time)
    {
        var engine = NewEngine();
        engine.Start(0, startWave: 5);
        engine.Boss!.Hp = 1;
        time = ChaseAndShootBoss(engine);
        Assert.True(engine.ShowPerfectBonus);
        return engine;
    }

    [Theory]
    [InlineData(0.41f)]
    [InlineData(0.445f)]
    public void Phone_PerfectSitsBelowTheEnlargedHitLine(float scale)
    {
        var engine = PerfectEngine(out var t);
        SetLostALife(engine, true);
        var boxes = Boxes(new SceneBuilder { TouchControls = true, DisplayScale = scale }.Build(engine, t));
        var hit = boxes.Single(b => b.Text == "You were hit!");
        var perfect = boxes.Single(b => b.Text.StartsWith("PERFECT!"));
        Assert.True(hit.Size > 20);
        Assert.True(perfect.Top > hit.Bottom, $"PERFECT top {perfect.Top} not below hit bottom {hit.Bottom}");
    }

    [Fact]
    public void Phone_HitSizeDoesNotLeakIntoTheNextFrame()
    {
        var engine = PerfectEngine(out var t);
        var builder = new SceneBuilder { TouchControls = true, DisplayScale = 0.41f };
        SetLostALife(engine, true);
        builder.Build(engine, t);
        SetLostALife(engine, false);
        var line = TextCommands(builder.Build(engine, t)).Single(c => c.Text.StartsWith("PERFECT!"));
        Assert.Equal(80, line.Y);
    }

    [Fact]
    public void Phone_FrenzyStaysBelowTheLoweredHeader()
    {
        var engine = NewEngine();
        engine.Start(0);
        foreach (var alien in engine.Aliens.Skip(1))
            alien.HasBeenHit = true;
        Assert.True(engine.IsFrenzy);
        var boxes = Boxes(new SceneBuilder { TouchControls = true, DisplayScale = 0.30f }.Build(engine, 100));
        var header = boxes.Single(b => b.Text.StartsWith("Score:"));
        var frenzy = boxes.Single(b => b.Text == "FRENZY!");
        Assert.False(Intersects(header, frenzy), $"header bottom {header.Bottom}, FRENZY top {frenzy.Top}");
    }

    [Fact]
    public void ShrinkToFit_NeverGoesBelowTheDesignSize()
    {
        var method = typeof(SceneBuilder).GetMethod("ShrinkToFit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        Assert.Equal(18, method.Invoke(null, new object[] { 30, 18, 2000f, 100f }));
        Assert.Equal(26, method.Invoke(null, new object[] { 30, 18, 1000f, 870f }));
        Assert.Equal(30, method.Invoke(null, new object[] { 30, 18, 500f, 870f }));
    }

    [Fact]
    public void Desktop_IgnoresTheScale()
    {
        var expected = PhoneScenes(1f, touch: false);
        var actual = PhoneScenes(0.41f, touch: false);

        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Scene.Cmds, actual[i].Scene.Cmds);
            Assert.Equal(expected[i].Scene.Strings, actual[i].Scene.Strings);
            Assert.Equal(0, actual[i].Scene.MinTextSize);
        }
    }
}
