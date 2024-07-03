using System;
using System.Collections.Generic;
using System.Linq;
using congestion.calculator.TaxRules;
using congestion.calculator.Vehicle;

public class CongestionTaxCalculator
{

    private readonly ITaxRulesFactory _rulesFactory;
    private ICongestionTaxRules _rule;
    public CongestionTaxCalculator(ITaxRulesFactory rulesFactory)
    {
        this._rulesFactory = rulesFactory;
    }


    /**
         * Calculate the total toll fee for one day
         *
         * @param vehicle - the vehicle
         * @param dates   - date and time of all passes on one day
         * @return - the total congestion tax for that day
         */

    public int GetTax(IVehicle vehicle, DateTime[] dates, string city, int year)
    {
        // Create an instance of TaxRules using the factory
        _rule = _rulesFactory.Create(city, year);

        if (_rule.IsTollFreeVehicle(vehicle)) return 0;

        if (vehicle == null || dates == null || dates.Length == 0)
            return 0;

        if (dates.Length == 1)
            return _rule.GetTollFee(dates[0]); ;

        var timesDictionary = FillTimesTaxFee(vehicle, dates);

        var totalFee = 0;

        foreach (var month in GroupedByMonth(dates))
        {
            foreach (var day in GroupedByDay(month))
            {
                var dayFee = day.Value.Sum(current => timesDictionary[current]);

                dayFee = ApplyMaximumDayTaxFeeRule(dayFee);

                totalFee += dayFee;
            }
        }

        return totalFee;
    }

    private int ApplyMaximumDayTaxFeeRule(int dayFee)
    {
        if (dayFee > _rule.GetMaxFee())
            dayFee = _rule.GetMaxFee();
        return dayFee;
    }

    private static Dictionary<int, DateTime[]> GroupedByDay(KeyValuePair<int, DateTime[]> month)
    {
        var groupedByDay =
            month.Value
                .GroupBy(c => c.Day)
                .ToDictionary(group => group.Key, group => group
                    .OrderBy(c => c.Hour).ThenBy(c => c.Minute)
                    .ToArray());
        return groupedByDay;
    }

    private static Dictionary<int, DateTime[]> GroupedByMonth(DateTime[] dates)
    {
        var groupedByMonth =
            dates
                .GroupBy(c => c.Month)
                .ToDictionary(group => group.Key, group => group
                    .OrderBy(c => c.Day).ThenBy(c => c.Hour).ThenBy(c => c.Minute)
                    .ToArray());
        return groupedByMonth;
    }

    private Dictionary<DateTime, int> FillTimesTaxFee(IVehicle vehicle, DateTime[] dates)
    {
        Array.Sort(dates);

        var timesDictionary = dates.ToDictionary(group => group, group => 0);

        var earlier = dates[0];

        foreach (var current in dates)
        {
            var earlierFee = _rule.GetTollFee(earlier);
            var currentFee = _rule.GetTollFee(current);

            var minutes = (long)current.Subtract(earlier).TotalMinutes;
            if (minutes <= 60)
            {
                if (earlierFee <= currentFee)
                    currentFee -= earlierFee;

                SetTimeTaxFee(timesDictionary, earlier, earlierFee, current, currentFee);
            }
            else
            {
                SetTimeTaxFee(timesDictionary, earlier, earlierFee, current, currentFee);

                earlier = current;
            }
        }

        return timesDictionary;
    }

    private static void SetTimeTaxFee(Dictionary<DateTime, int> timesDictionary, DateTime earlier, int earlierFee, DateTime current,
        int currentFee)
    {
        timesDictionary[earlier] = earlierFee;
        timesDictionary[current] = currentFee;
    }
}