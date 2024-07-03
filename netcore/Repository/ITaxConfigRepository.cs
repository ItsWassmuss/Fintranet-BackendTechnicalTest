using congestion.calculator.TaxRules;

namespace congestion.calculator.Repository
{
    public interface ITaxConfigRepository
    {
        Config LoadConfig(string city, int year);
        void Insert(string city, int year, string configJson);
    }
}