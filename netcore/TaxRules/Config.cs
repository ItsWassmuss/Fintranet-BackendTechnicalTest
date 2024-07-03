using System.Collections.Generic;

namespace congestion.calculator.TaxRules
{
    public class Config
    {
        public int MaxDayFee { get; set; }
        public int[] TollFreeMonths { get; set; }
        public List<string> TollFreeDates { get; set; }
        public List<TimeRangeConfig> TimeSegments { get; set; }
    }

    public class TimeRangeConfig
    {
        public string Start { get; set; }
        public string End { get; set; }
        public int Fee { get; set; }
    }
}
