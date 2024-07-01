using System;
using congestion.calculator.Vehicle;

namespace congestion.calculator.TaxRules
{
    public class GothenburgTaxRules2013 : ICongestionTaxRules
    {
        public int GetMaxFee()
        {
            return 60;
        }

        private bool IsTollFreeDate(DateTime date)
        {
            var month = date.Month;
            var day = date.Day;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

            return month == 1 && (day == 1) ||
                   month == 3 && (day == 28 || day == 29) ||
                   month == 4 && (day == 1 || day == 30) ||
                   month == 5 && (day == 1 || day == 8 || day == 9) ||
                   month == 6 && (day == 5 || day == 6 || day == 21) ||
                   month == 7 ||
                   month == 11 && day == 1 ||
                   month == 12 && (day == 24 || day == 25 || day == 26 || day == 31);
        }

        public bool IsTollFreeVehicle(IVehicle vehicle)
        {
            if (vehicle == null) return false;
            var vehicleType = vehicle.GetVehicleType();
            return Enum.TryParse<TollFreeVehicles>(vehicleType, true, out _);
        }

        public int GetTollFee(DateTime date)
        {
            if (IsTollFreeDate(date)) return 0;

            var hour = date.Hour;
            var minute = date.Minute;

            if (hour == 6 && minute <= 29) return 8;
            if (hour == 6 && minute >= 30) return 13;

            if (hour == 7) return 18;

            if (hour == 8 && minute <= 29) return 13;
            if (hour >= 8 && hour <= 14) return 8;

            if (hour == 15 && minute <= 29) return 13;
            if (hour == 15 && minute >= 30 || hour == 16) return 18;

            if (hour == 17) return 13;

            if (hour == 18 && minute <= 29) return 8;

            return 0;
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