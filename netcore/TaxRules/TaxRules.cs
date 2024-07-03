using System;
using System.Collections.Generic;
using System.Linq;
using congestion.calculator.Repository;
using congestion.calculator.Vehicle;

namespace congestion.calculator.TaxRules
{
    public class TaxRules : ICongestionTaxRules
    {
        private readonly ITaxConfigRepository _configRepository;

        private readonly int _maxDailyFee;
        private readonly int[] _tollFreeMonth;
        private readonly HashSet<DateTime> _tollFreeDates;
        private readonly List<TimeRange> _timeRanges;

        public TaxRules(string city, int year, ITaxConfigRepository configRepository)
        {
            _configRepository = configRepository;

            var config = LoadConfig(city, year);
            _maxDailyFee = config.MaxDayFee;
            _tollFreeMonth = config.TollFreeMonths;
            _tollFreeDates = new HashSet<DateTime>(config.TollFreeDates.Select(DateTime.Parse));
            _timeRanges = config.TimeSegments.Select(tr => new TimeRange(tr.Start, tr.End, tr.Fee)).ToList();

        }


        public int GetMaxFee()
        {
            return _maxDailyFee;
        }

        public bool IsTollFreeVehicle(IVehicle vehicle)
        {
            if (vehicle == null) return false;
            var vehicleType = vehicle.GetVehicleType();
            return Enum.TryParse<TollFreeVehicles>(vehicleType, true, out _);
        }

        public int GetTollFee(DateTime date)
        {
            if (IsTollFreeDate(date))
                return 0;

            foreach (var timeRange in _timeRanges)
            {
                if (timeRange.IsInRange(date))
                {
                    return timeRange.Fee;
                }
            }

            return 0;
        }

        private bool IsTollFreeDate(DateTime date)
        {
            // It could be added to config
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return true;

            if (_tollFreeMonth.Contains(date.Month)) return true;

            return _tollFreeDates.Contains(date.Date);
        }

        private Config LoadConfig(string city, int year)
        {
            return _configRepository.LoadConfig(city, year);
        }

        private class TimeRange
        {
            public TimeSpan Start { get; }
            public TimeSpan End { get; }
            public int Fee { get; }

            public TimeRange(string start, string end, int fee)
            {
                Start = TimeSpan.Parse(start);
                End = TimeSpan.Parse(end);
                Fee = fee;
            }

            public bool IsInRange(DateTime dateTime)
            {
                var timeOfDay = dateTime.TimeOfDay;
                return timeOfDay >= Start && timeOfDay <= End;
            }
        }

        private enum TollFreeVehicles
        {
            Motorcycle = 0,
            Tractor = 1,
            Emergency = 2,
            Diplomat = 3,
            Foreign = 4,
            Military = 5
        }

    }
}