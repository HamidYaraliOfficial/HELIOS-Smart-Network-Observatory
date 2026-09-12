using Helios.Core.Models;

namespace Helios.Rules;

/// <summary>
/// Evaluates the fully user-defined weekly "open hours" schedule for a scope
/// (global, a segment, or a single node) and reports whether monitoring /
/// maintenance is currently active plus a live countdown to the next
/// transition, e.g. "Active - closes in 2h 14m" or "Inactive - opens in 6h 40m".
/// The user supplies every window; HELIOS applies no implicit default besides
/// "always on" when no schedule has been configured.
/// </summary>
public sealed class MonitoringScheduleService
{
    public ScheduleStatus Evaluate(MonitoringSchedule schedule, DateTimeOffset? nowUtc = null)
    {
        var now = nowUtc ?? DateTimeOffset.UtcNow;

        if (!schedule.Enabled || schedule.Windows.Count == 0)
        {
            return new ScheduleStatus
            {
                IsCurrentlyActive = true,
                EvaluatedAtUtc = now,
                StatusLabel = "Active (no schedule configured - always on)"
            };
        }

        var tz = SafeFindTimeZone(schedule.TimeZoneId);
        var localNow = TimeZoneInfo.ConvertTime(now, tz);

        var todayWindow = schedule.Windows.FirstOrDefault(w => w.Day == localNow.DayOfWeek && w.IsActive);
        bool isActiveNow = false;
        DateTimeOffset? nextTransitionLocalAsUtc = null;

        if (todayWindow is not null)
        {
            var windowStart = localNow.Date + todayWindow.OpenTime;
            var windowEnd = localNow.Date + todayWindow.CloseTime;
            if (localNow >= windowStart && localNow <= windowEnd)
            {
                isActiveNow = true;
                nextTransitionLocalAsUtc = TimeZoneInfo.ConvertTimeToUtc(windowEnd, tz);
            }
        }

        if (!isActiveNow)
        {
            nextTransitionLocalAsUtc = FindNextWindowStart(schedule, localNow, tz);
        }

        var status = new ScheduleStatus
        {
            IsCurrentlyActive = isActiveNow,
            EvaluatedAtUtc = now,
            StatusLabel = isActiveNow ? "Active" : "Inactive",
            NextTransitionUtc = nextTransitionLocalAsUtc
        };

        if (nextTransitionLocalAsUtc is { } next)
            status.TimeUntilNextTransition = next - now;

        return status;
    }

    private static DateTimeOffset? FindNextWindowStart(MonitoringSchedule schedule, DateTimeOffset localNow, TimeZoneInfo tz)
    {
        for (int dayOffset = 0; dayOffset <= 7; dayOffset++)
        {
            var candidateDate = localNow.Date.AddDays(dayOffset);
            var candidateDay = candidateDate.DayOfWeek;
            var window = schedule.Windows.FirstOrDefault(w => w.Day == candidateDay && w.IsActive);
            if (window is null) continue;

            var candidateStart = candidateDate + window.OpenTime;
            if (dayOffset == 0 && candidateStart <= localNow) continue;

            return TimeZoneInfo.ConvertTimeToUtc(candidateStart, tz);
        }
        return null;
    }

    private static TimeZoneInfo SafeFindTimeZone(string id)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch { return TimeZoneInfo.Local; }
    }
}
