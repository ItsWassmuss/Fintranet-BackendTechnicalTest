using congestion.calculator.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using congestion.calculator.TaxRules;

namespace congestion.calculator
{
    class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Example of using the factory to create TaxRules
            //var factory = host.Services.GetRequiredService<ITaxRulesFactory>();
            //var taxRules = factory.Create("Gothenburg", 2013);

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Register the repository
                    services.AddScoped<TaxConfigRepository>();

                    // You can also register other services here
                    services.AddScoped<ITaxConfigRepository, TaxConfigRepository>();
                    services.AddScoped<ITaxRulesFactory, TaxRulesFactory>();

                });
    }
}
