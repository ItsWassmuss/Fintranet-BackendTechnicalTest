using System;
using System.Linq;
using congestion.calculator.TaxRules;
using Xunit;
using congestion.calculator.Vehicle;

namespace TestProject
{
    public class CongestionTaxCalculatorTests
    {
        private readonly CongestionTaxCalculator _calculator;

        public CongestionTaxCalculatorTests()
        {
            _calculator = new CongestionTaxCalculator(new GothenburgTaxRules2013());
        }

        [Fact]
        public void GetTax_ShouldReturnZero_ForTollFreeVehicle()
        {
            // Arrange
            IVehicle vehicle = new MockTollFreeVehicle();
            DateTime[] dates = { new DateTime(2013, 7, 1, 8, 0, 0) };

            // Act
            int tax = _calculator.GetTax(vehicle, dates);

            // Assert
            Assert.Equal(0, tax);
        }

        [Fact]
        public void GetTax_ShouldReturnZero_ForTollFreeDate()
        {
            // Arrange
            IVehicle vehicle = new MockVehicle();
            DateTime[] dates = { new DateTime(2013, 7, 1, 8, 0, 0) }; // July is toll-free

            // Act
            int tax = _calculator.GetTax(vehicle, dates);

            // Assert
            Assert.Equal(0, tax);
        }

        [Fact]
        public void GetTax_ShouldReturnCorrectTax_ForSinglePass()
        {
            // Arrange
            IVehicle vehicle = new MockVehicle();
            DateTime[] dates = { new DateTime(2013, 3, 1, 6, 15, 0) }; // Should be 8

            // Act
            int tax = _calculator.GetTax(vehicle, dates);

            // Assert
            Assert.Equal(8, tax);
        }

        [Fact]
        public void GetTax_ShouldReturnCorrectTax_ForMultiplePassesWithinHour()
        {
            // Arrange
            IVehicle vehicle = new MockVehicle();
            DateTime[] dates = {
                new DateTime(2013, 3, 1, 6, 15, 0),
                new DateTime(2013, 3, 1, 6, 45, 0)
            };

            // Act
            int tax = _calculator.GetTax(vehicle, dates);

            // Assert
            Assert.Equal(13, tax); // Only the highest fee within the hour should apply
        }

        [Fact]
        public void GetTax_ShouldReturnCorrectTax_ForMultiplePassesExceedingHour()
        {
            // Arrange
            IVehicle vehicle = new MockVehicle();
            DateTime[] dates = {
                new DateTime(2013, 3, 1, 6, 15, 0),
                new DateTime(2013, 3, 1, 7, 30, 0)
            };

            // Act
            int tax = _calculator.GetTax(vehicle, dates);

            // Assert
            Assert.Equal(26, tax); // 8 + 18
        }

        [Fact]
        public void GetTax_ShouldCapAtMaximumDailyFee()
        {
            // Arrange
            IVehicle vehicle = new MockVehicle();
            DateTime[] dates = {
                new DateTime(2013, 3, 1, 6, 0, 0),
                new DateTime(2013, 3, 1, 7, 0, 0),
                new DateTime(2013, 3, 1, 8, 0, 0),
                new DateTime(2013, 3, 1, 15, 0, 0),
                new DateTime(2013, 3, 1, 17, 0, 0)
            };

            // Act
            int tax = _calculator.GetTax(vehicle, dates);

            // Assert
            Assert.Equal(57, tax); // Maximum cap
        }

        [Fact]
        public void GetTax_ShouldReturnCorrectTax_ForSampleTimeInScenario()
        {
            // Arrange
            IVehicle vehicle = new MockVehicle();
            string[] dateStrings = {
                "2013-01-14 21:00:00", // 0
                "2013-01-15 21:00:00", // 0

                "2013-02-07 06:23:27", // 8
                "2013-02-07 15:27:00", // 13         => 21
                "2013-02-07 23:27:00", // 0         

                "2013-02-08 00:20:00", // 0
                "2013-02-08 06:20:27", // 8 => |8
                "2013-02-08 06:27:00", // 8 => |0
                "2013-02-08 14:35:00", // 8 => |0
                "2013-02-08 15:29:00", // 13   |13
                "2013-02-08 15:47:00", // 18   |18
                "2013-02-08 16:01:00", // 18   |0
                "2013-02-08 16:48:00", // 18
                "2013-02-08 17:49:00", // 13   |13
                "2013-02-08 18:29:00", // 8    |0
                "2013-02-08 18:35:00", // 0          => 70 => 60

                "2013-03-26 14:25:00", // 8
                "2013-03-28 14:07:27"   // 0         => 16
                                        //           => 89
            };
            DateTime[] dates = dateStrings
                .Select(dateString => DateTime.Parse(dateString))
                .ToArray();

            // Act
            int tax = _calculator.GetTax(vehicle, dates);

            // Assert
            Assert.Equal(89, tax); // Maximum cap
        }

        // Mock classes to simulate IVehicle behavior
        private class MockVehicle : IVehicle
        {
            public string GetVehicleType() => "Car";
        }

        private class MockTollFreeVehicle : IVehicle
        {
            public string GetVehicleType() => "Motorcycle";
        }
    }
}
