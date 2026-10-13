using BlazorInvaders.GameObjects;
using Xunit;

namespace BlazorInvaders.Tests;

public class ComboTrackerTests
{
    private static void Hit(ComboTracker combo)
    {
        var volley = combo.StartVolley(1);
        combo.AddHit(volley);
        combo.ShotEnded(volley);
    }

    private static void Miss(ComboTracker combo)
    {
        var volley = combo.StartVolley(1);
        combo.ShotEnded(volley);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(4, 1)]
    [InlineData(5, 2)]
    [InlineData(10, 3)]
    [InlineData(15, 4)]
    [InlineData(40, 4)]
    public void Multiplier_RisesEveryFiveHits_UpToFour(int hits, int expected)
    {
        var combo = new ComboTracker();
        for (int i = 0; i < hits; i++) Hit(combo);

        Assert.Equal(hits, combo.Count);
        Assert.Equal(expected, combo.Multiplier);
    }

    [Fact]
    public void AddHit_ReportsOnlyTheHitsThatRaiseTheMultiplier()
    {
        var combo = new ComboTracker();
        var raised = new List<int>();

        for (int i = 1; i <= 25; i++)
        {
            var volley = combo.StartVolley(1);
            if (combo.AddHit(volley)) raised.Add(i);
            combo.ShotEnded(volley);
        }

        Assert.Equal(new[] { 5, 10, 15 }, raised);
    }

    [Fact]
    public void MissedVolley_ResetsCombo()
    {
        var combo = new ComboTracker();
        for (int i = 0; i < 7; i++) Hit(combo);

        Miss(combo);

        Assert.Equal(0, combo.Count);
        Assert.Equal(1, combo.Multiplier);
    }

    [Fact]
    public void DoubleShot_WithOneHit_KeepsCombo()
    {
        var combo = new ComboTracker();
        for (int i = 0; i < 7; i++) Hit(combo);

        var volley = combo.StartVolley(2);
        combo.ShotEnded(volley);    // the stray shot ends first
        combo.AddHit(volley);
        combo.ShotEnded(volley);

        Assert.Equal(8, combo.Count);
    }

    [Fact]
    public void DoubleShot_WithBothMissing_ResetsComboOnlyWhenBothAreSpent()
    {
        var combo = new ComboTracker();
        for (int i = 0; i < 7; i++) Hit(combo);

        var volley = combo.StartVolley(2);
        combo.ShotEnded(volley);
        Assert.Equal(7, combo.Count);

        combo.ShotEnded(volley);
        Assert.Equal(0, combo.Count);
    }

    [Fact]
    public void Break_ResetsCombo()
    {
        var combo = new ComboTracker();
        for (int i = 0; i < 12; i++) Hit(combo);

        combo.Break();

        Assert.Equal(0, combo.Count);
    }
}
