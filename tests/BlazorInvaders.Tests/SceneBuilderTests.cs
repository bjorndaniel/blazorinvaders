using BlazorInvaders.GameObjects;
using BlazorInvaders.Rendering;
using Xunit;

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
    };

    private static GameEngine NewEngine() => new(870, 500, new Random(42));

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
}
