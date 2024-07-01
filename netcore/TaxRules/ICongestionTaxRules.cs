using congestion.calculator.Vehicle;
using System;

namespace congestion.calculator.TaxRules
{
    public interface ICongestionTaxRules
    {
        int GetMaxFee();
        bool IsTollFreeVehicle(IVehicle vehicle);
        int GetTollFee(DateTime date);
    }
}