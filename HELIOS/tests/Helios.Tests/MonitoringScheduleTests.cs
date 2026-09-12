using Helios.Core.Models;
using Helios.Rules;
using Xunit;

namespace Helios.Tests;

public class MonitoringScheduleTests
{
    [Fact]
    public void Evaluate_AlwaysActive_WhenNoScheduleConfigured()
    {
        var service = new MonitoringScheduleService();
        var schedule = new MonitoringSchedule { Enabled = false };

        var status = service.Evaluate(schedule);

        Assert.True(status.IsCurrentlyActive);
    }

    [Fact]
    public void Evaluate_ReportsInactive_OutsideConfiguredWindow()
    {
        var service = new MonitoringScheduleService();
        var schedule = new MonitoringSchedule
        {
            Enabled = true,
            TimeZoneId = "UTC",
            Windows = { new DailyTimeWindow { Day = DayOfWeek.Monday, OpenTime = TimeSpan.FromHours(9), CloseTime = TimeSpan.FromHours(17) } }
        };

        var mondayMorningBeforeOpen = new DateTimeOffset(2026, 9, 7, 6, 0, 0, TimeSpan.Zero); // a Monday
        var status = service.Evaluate(schedule, mondayMorningBeforeOpen);

        Assert.False(status.IsCurrentlyActive);
        Assert.NotNull(status.TimeUntilNextTransition);
    }

    [Fact]
    public void Evaluate_ReportsActive_InsideConfiguredWindow()
    {
        var service = new MonitoringScheduleService();
        var schedule = new MonitoringSchedule
        {
            Enabled = true,
            TimeZoneId = "UTC",
            Windows = { new DailyTimeWindow { Day = DayOfWeek.Monday, OpenTime = TimeSpan.FromHours(9), CloseTime = TimeSpan.FromHours(17) } }
        };

        var mondayNoon = new DateTimeOffset(2026, 9, 7, 12, 0, 0, TimeSpan.Zero);
        var status = service.Evaluate(schedule, mondayNoon);

        Assert.True(status.IsCurrentlyActive);
        Assert.NotNull(status.TimeUntilNextTransition);
    }
}
