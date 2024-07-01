using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using congestion.calculator.Vehicle;
using Newtonsoft.Json;

namespace congestion.calculator.TaxRules
{
    public class TaxRules : ICongestionTaxRules
    {
        private readonly int _maxDailyFee;
        private readonly int[] _tollFreeMonth;
        private readonly HashSet<DateTime> _tollFreeDates;
        private readonly List<TimeRange> _timeRanges;

        public TaxRules(string city)
        {
            var config = LoadConfig(city);
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

        private static Config LoadConfig(string city)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tax_rules.json");
            var configContent = File.ReadAllText(path);
            var allConfigs = JsonConvert.DeserializeObject<Dictionary<string, Config>>(configContent);
            if (allConfigs != null && allConfigs.TryGetValue(city.ToLower(), out var config))
            {
                return config;
            }
            throw new Exception($"Configuration for city '{city}' not found.");
        }
        private class Config
        {
            public int MaxDayFee { get; set; }
            public int[] TollFreeMonths { get; set; }
            public List<string> TollFreeDates { get; set; }
            public List<TimeRangeConfig> TimeSegments { get; set; }
        }

        private class TimeRangeConfig
        {
            public string Start { get; set; }
            public string End { get; set; }
            public int Fee { get; set; }
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