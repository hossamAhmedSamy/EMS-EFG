namespace EMS.Services;

public readonly record struct TimeRange(TimeOnly Start, TimeOnly End)
{
    public static readonly TimeSpan MeetingLength = TimeSpan.FromHours(1);

    // Touching ranges (1-2 and 2-3) do not overlap.
    public bool Overlaps(TimeRange other) => Start < other.End && other.Start < End;

    public bool Contains(TimeRange other) => Start <= other.Start && other.End <= End;

    public override string ToString() => $"{Start:h:mm tt} – {End:h:mm tt}";

    public static string? Validate(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
            return "End time must be after start time.";
        if (end - start < MeetingLength)
            return "The time window must be at least one hour, since every meeting is one hour.";
        return null;
    }
}
