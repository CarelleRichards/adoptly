namespace Adoptly.Web.Utilities;

public static class TimeSpanExtensions
{
    // Calculate the average TimeSpan in a list of TimeSpans.

    public static TimeSpan Average(this IEnumerable<TimeSpan> timeSpans)
    {
        IEnumerable<long> ticksPerTimeSpan = timeSpans.Select(x => x.Ticks);
        double averageTicks = ticksPerTimeSpan.Average();
        long averageTicksLong = Convert.ToInt64(averageTicks);
        TimeSpan averageTimeSpan = TimeSpan.FromTicks(averageTicksLong);
        return averageTimeSpan;
    }
}