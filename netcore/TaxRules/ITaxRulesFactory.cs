using Microsoft.Extensions.DependencyInjection;
using System;
using congestion.calculator.Repository;

namespace congestion.calculator.TaxRules
{
    public interface ITaxRulesFactory
    {
        TaxRules Create(string city, int year);
    }

    public class TaxRulesFactory : ITaxRulesFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TaxRulesFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TaxRules Create(string city, int year)
        {
            var configService = _serviceProvider.GetRequiredService<ITaxConfigRepository>();
            return new TaxRules(city, year, configService);
        }
    }
}
