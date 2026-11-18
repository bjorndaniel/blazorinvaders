using BlazorInvaders.GameObjects;
using Xunit;

namespace BlazorInvaders.Tests;

public class SoundSettingTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void FromStored_Missing_SoundOn(string? stored)
    {
        Assert.False(SoundSetting.FromStored(stored).Muted);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("TRUE", false)]
    [InlineData("1", false)]
    [InlineData("garbage", false)]
    public void FromStored_Value_ParsesOnlyTrue(string stored, bool expected)
    {
        Assert.Equal(expected, SoundSetting.FromStored(stored).Muted);
    }

    [Fact]
    public void Toggle_RoundTripsThroughStorage()
    {
        var setting = new SoundSetting();

        setting.Toggle();

        Assert.True(setting.Muted);
        Assert.Equal("true", setting.ToStored());
        Assert.True(SoundSetting.FromStored(setting.ToStored()).Muted);

        setting.Toggle();

        Assert.False(setting.Muted);
        Assert.Equal("false", setting.ToStored());
    }
}
