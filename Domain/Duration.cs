namespace ExamApp.Domain;

using System.Text.RegularExpressions;

public readonly partial struct Duration : IEquatable<Duration>
{
    private static readonly Regex FormatRegex = DurationRegex();

    public TimeSpan Value { get; }

	private Duration(TimeSpan value) => Value = value;

	public static Duration FromTimeSpan(TimeSpan timeSpan) => new(timeSpan);

    public static Duration Parse(string durationString)
    {
        var match = FormatRegex.Match(durationString);
        if (!match.Success)
            throw new ArgumentException($"Invalid duration format: '{durationString}'. Expected: '1h', '30m', '45s'");

        var amount = int.Parse(match.Groups[1].Value);
        return match.Groups[2].Value switch
        {
            "h" => FromTimeSpan(TimeSpan.FromHours(amount)),
            "m" => FromTimeSpan(TimeSpan.FromMinutes(amount)),
            "s" => FromTimeSpan(TimeSpan.FromSeconds(amount)),
            _ => throw new ArgumentException($"Invalid unit: '{match.Groups[2].Value}'")
        };
    }

    public static bool TryParse(string durationString, out Duration duration)
    {
        try
        {
            duration = Parse(durationString);
            return true;
        }
        catch
        {
            duration = default;
            return false;
        }
    }

    public string ToStringFormat() => Value switch
    {
        var ts when ts.TotalHours >= 1 => $"{(int) ts.TotalHours}h",
        var ts when ts.TotalMinutes >= 1 => $"{(int) ts.TotalMinutes}m",
        var ts => $"{(int) ts.TotalSeconds}s"
    };

    public override string ToString() => ToStringFormat();

    public bool Equals(Duration other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is Duration other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Duration left, Duration right) => left.Equals(right);
    public static bool operator !=(Duration left, Duration right) => !left.Equals(right);

	[GeneratedRegex(@"^(\d+)(h|m|s)$", RegexOptions.Compiled)]
	private static partial Regex DurationRegex();
}
