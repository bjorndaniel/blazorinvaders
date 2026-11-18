namespace BlazorInvaders.GameObjects;

/// <summary>Whether the game's sound is muted, and how that choice is stored on the device.</summary>
public sealed class SoundSetting
{
    /// <summary>The localStorage key the setting is kept under.</summary>
    public const string StorageKey = "muted";

    private const string MutedValue = "true";
    private const string UnmutedValue = "false";

    /// <summary>True while all sound is off. Defaults to false (sound on).</summary>
    public bool Muted { get; private set; }

    /// <summary>Reads a stored value; only exactly "true" mutes, anything else (including null) leaves sound on.</summary>
    public static SoundSetting FromStored(string? stored) =>
        new() { Muted = string.Equals(stored, MutedValue, StringComparison.Ordinal) };

    /// <summary>Switches between muted and sound on.</summary>
    public void Toggle() => Muted = !Muted;

    /// <summary>The value to store for the current state, as read back by <see cref="FromStored"/>.</summary>
    public string ToStored() => Muted ? MutedValue : UnmutedValue;
}
