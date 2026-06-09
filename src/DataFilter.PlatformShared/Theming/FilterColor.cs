namespace DataFilter.PlatformShared.Theming;

/// <summary>
/// Platform-neutral RGBA color used by <see cref="FilterTheme"/>.
/// </summary>
public readonly struct FilterColor : IEquatable<FilterColor>
{
    public FilterColor(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }

    /// <summary>Parses <c>#RRGGBB</c> or <c>#AARRGGBB</c> (also accepts leading <c>#</c> omission).</summary>
    public static FilterColor Parse(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Color value is required.", nameof(hex));

        var span = hex.AsSpan().Trim();
        if (span[0] == '#')
            span = span[1..];

        if (span.Length is not (6 or 8))
            throw new FormatException($"Invalid color '{hex}'. Expected #RRGGBB or #AARRGGBB.");

        byte a = 255;
        ReadOnlySpan<char> rgb;
        if (span.Length == 8)
        {
            a = ParseByte(span[..2]);
            rgb = span[2..];
        }
        else
        {
            rgb = span;
        }

        return new FilterColor(ParseByte(rgb[..2]), ParseByte(rgb[2..4]), ParseByte(rgb[4..6]), a);
    }

    public string ToHex(bool includeAlpha = false) =>
        includeAlpha
            ? $"#{A:X2}{R:X2}{G:X2}{B:X2}"
            : $"#{R:X2}{G:X2}{B:X2}";

    public bool Equals(FilterColor other) =>
        R == other.R && G == other.G && B == other.B && A == other.A;

    public override bool Equals(object? obj) => obj is FilterColor other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(R, G, B, A);

    public static bool operator ==(FilterColor left, FilterColor right) => left.Equals(right);

    public static bool operator !=(FilterColor left, FilterColor right) => !left.Equals(right);

    private static byte ParseByte(ReadOnlySpan<char> chars) =>
        byte.Parse(chars, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
}
