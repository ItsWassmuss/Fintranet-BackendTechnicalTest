using System;
using congestion.calculator.TaxRules;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;

namespace congestion.calculator.Repository
{

    public class TaxConfigRepository : ITaxConfigRepository
    {
        private readonly string _connectionString;

        public TaxConfigRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public Config LoadConfig(string city, int year)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT config FROM CityConfigs WHERE city = @city and year = @year", connection))
                {
                    command.Parameters.AddWithValue("city", city.ToLower());
                    command.Parameters.AddWithValue("year", year);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var configJson = reader.GetString(0);
                            return JsonConvert.DeserializeObject<Config>(configJson);
                        }
                    }
                }
            }

            throw new Exception($"Configuration for city '{city}' not found.");
        }

        public void Insert(string city, int year, string configJson)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("INSERT INTO CityConfigs (city, year, config) VALUES (@city, @year, @config::jsonb) ON CONFLICT (city, year) DO UPDATE SET config = EXCLUDED.config", connection))
                {
                    command.Parameters.AddWithValue("city", city.ToLower());
                    command.Parameters.AddWithValue("year", year);
                    command.Parameters.AddWithValue("config", configJson);
                    command.ExecuteNonQuery();
                }
            }
        }


    }

}
