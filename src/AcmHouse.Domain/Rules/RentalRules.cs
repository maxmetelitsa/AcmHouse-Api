namespace AcmHouse.Domain.Rules;

/// <summary>Business rules for renting a property: allowed period, price and agent profit.</summary>
public static class RentalRules
{
    public const int MinDays = 1;
    public const int MaxDays = 30;

    /// <summary>Agency commission taken from the rental price.</summary>
    public const decimal CommissionRate = 0.15m;

    /// <summary>Tax withheld from the agency commission.</summary>
    public const decimal TaxRate = 0.30m;

    /// <summary>Number of nights between check-in and check-out.</summary>
    public static int CountDays(DateOnly start, DateOnly end) => end.DayNumber - start.DayNumber;

    /// <summary>Why the period cannot be booked, or null if it can.</summary>
    public static string? ValidatePeriod(DateOnly start, DateOnly end, DateOnly today)
    {
        if (start < today)
        {
            return "The start date cannot be in the past.";
        }

        int days = CountDays(start, end);
        if (days < MinDays)
        {
            return $"The rent must last at least {MinDays} day.";
        }

        if (days > MaxDays)
        {
            return $"The rent cannot last more than {MaxDays} days.";
        }

        return null;
    }

    public static decimal CalculateTotal(DateOnly start, DateOnly end, decimal dayPrice)
    {
        if (dayPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dayPrice), "The price cannot be negative.");
        }

        return CountDays(start, end) * dayPrice;
    }

    /// <summary>Profit credited to the agent: the commission minus tax, rounded to kopecks.</summary>
    public static decimal CalculateAgentProfit(decimal total)
    {
        return Math.Round(total * CommissionRate * (1 - TaxRate), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Two stays overlap when one starts before the other ends. The end date is the check-out day,
    /// so a new stay may start on the day the previous one ends.
    /// </summary>
    public static bool Overlaps(DateOnly start, DateOnly end, DateOnly otherStart, DateOnly otherEnd)
    {
        return start < otherEnd && otherStart < end;
    }
}
