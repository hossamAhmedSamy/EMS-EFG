using EMS.Services;
using static EMS.Tests.TestDb;

namespace EMS.Tests;

public class TimeRangeTests
{
    [Fact]
    public void Touching_ranges_do_not_overlap()
    {
        Assert.False(new TimeRange(T(13), T(14)).Overlaps(new TimeRange(T(14), T(15))));
    }

    [Fact]
    public void Partially_shared_ranges_overlap()
    {
        Assert.True(new TimeRange(T(13), T(14)).Overlaps(new TimeRange(T(13, 30), T(14, 30))));
    }

    [Fact]
    public void Window_contains_a_meeting_that_touches_its_edges()
    {
        var window = new TimeRange(T(13), T(16));
        Assert.True(window.Contains(new TimeRange(T(13), T(14))));
        Assert.True(window.Contains(new TimeRange(T(15), T(16))));
        Assert.False(window.Contains(new TimeRange(T(15, 30), T(16, 30))));
    }

    [Theory]
    [InlineData(13, 0, 16, 0, true)]
    [InlineData(13, 0, 14, 0, true)]
    [InlineData(13, 0, 13, 30, false)]
    [InlineData(16, 0, 13, 0, false)]
    [InlineData(13, 0, 13, 0, false)]
    public void Validate_requires_a_window_of_at_least_one_hour(int fromH, int fromM, int toH, int toM, bool valid)
    {
        Assert.Equal(valid, TimeRange.Validate(T(fromH, fromM), T(toH, toM)) == null);
    }
}
