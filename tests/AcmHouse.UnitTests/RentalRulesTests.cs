using AcmHouse.Domain.Rules;
using Xunit;

namespace AcmHouse.UnitTests;

public class RentalRulesTests
{
    private static readonly DateOnly Today = new(2026, 5, 1);

    [Fact]
    public void CountDays_WorksAcrossNewYear()
    {
        Assert.Equal(3, RentalRules.CountDays(new DateOnly(2026, 12, 30), new DateOnly(2027, 1, 2)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    public void ValidatePeriod_AcceptsPeriodsWithinLimits(int days)
    {
        Assert.Null(RentalRules.ValidatePeriod(Today, Today.AddDays(days), Today));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(31)]
    public void ValidatePeriod_RejectsTooShortOrTooLongPeriods(int days)
    {
        Assert.NotNull(RentalRules.ValidatePeriod(Today, Today.AddDays(days), Today));
    }

    [Fact]
    public void ValidatePeriod_RejectsStartInThePast()
    {
        Assert.Contains("past", RentalRules.ValidatePeriod(Today.AddDays(-1), Today.AddDays(2), Today));
    }

    [Fact]
    public void CalculateTotal_MultipliesDaysByDayPrice()
    {
        Assert.Equal(840m, RentalRules.CalculateTotal(Today, Today.AddDays(7), 120m));
    }

    [Fact]
    public void CalculateAgentProfit_IsCommissionMinusTax()
    {
        // 15% of 1000 = 150, minus 30% tax = 105.
        Assert.Equal(105m, RentalRules.CalculateAgentProfit(1000m));
    }

    [Fact]
    public void CalculateAgentProfit_RoundsToKopecks()
    {
        // 15% of 333.33 = 49.9995, minus 30% = 34.99965 -> 35.00
        Assert.Equal(35.00m, RentalRules.CalculateAgentProfit(333.33m));
    }

    [Theory]
    // existing stay: 10 May -> 15 May
    [InlineData(5, 10, false)]   // ends on the check-in day
    [InlineData(15, 20, false)]  // starts on the check-out day
    [InlineData(5, 11, true)]    // ends inside
    [InlineData(14, 20, true)]   // starts inside
    [InlineData(11, 13, true)]   // inside
    [InlineData(1, 30, true)]    // covers it
    public void Overlaps_TreatsEndDateAsCheckOutDay(int startDay, int endDay, bool expected)
    {
        var existingStart = new DateOnly(2026, 5, 10);
        var existingEnd = new DateOnly(2026, 5, 15);

        bool overlaps = RentalRules.Overlaps(new DateOnly(2026, 5, startDay), new DateOnly(2026, 5, endDay), existingStart, existingEnd);

        Assert.Equal(expected, overlaps);
    }
}
