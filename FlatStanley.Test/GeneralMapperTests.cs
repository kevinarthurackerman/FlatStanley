using FlatStanley.DefaultMaps;
using FlatStanley.Test.SampleData;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace FlatStanley.Test
{
    public class GeneralMapperTests
    {
        [Fact]
        public void ShouldFlattenObjectTree()
        {
            var orders = new[]
            {
                new Order
                {
                    Id = 1,
                    CustomerName = "Test Customer 1",
                    OrderLines = new[]
                    {
                        new OrderLine
                        {
                            ItemName = "Test Item 1",
                            Description = "Description",
                            Quantity = 2
                        },
                        new OrderLine
                        {
                            ItemName = "Test Item 2",
                            Quantity = 3
                        }
                    }
                },
                new Order
                {
                    Id = 2,
                    CustomerName = "Test Customer 2",
                    Address = new Address
                    {
                        Line1 = "Address Line 1",
                        City = "City 1",
                        State = "State 1",
                        Zipcode = "12345"
                    },
                    OrderLines = new[]
                    {
                        new OrderLine
                        {
                            ItemName = "Test Item 2",
                            Quantity = 4
                        }
                    }
                }
            };

            var mapper = new MapperBuilder
            {
                Maps = new List<AbstractMap>()
                {
                    new ValueMap<int>(),
                    new ValueMap<string>(),
                    new RecordMap<Order>(),
                    new RecordMap<Address>(),
                    new RecordMap<OrderLine>(),
                    new EnumerableMap<OrderLine>()
                }
            }
            .Build();

            var actualCsv = mapper.Serialize(orders);

            var expectedCsv =
@"Order.Id,Order.CustomerName,Order.Address.Line1,Order.Address.Line2,Order.Address.City,Order.Address.State,Order.Address.Zipcode,Order.OrderLines[0].ItemName,Order.OrderLines[0].Description,Order.OrderLines[0].Quantity,Order.OrderLines[1].ItemName,Order.OrderLines[1].Description,Order.OrderLines[1].Quantity
1,Test Customer 1,,,,,,Test Item 1,Description,2,Test Item 2,,3
2,Test Customer 2,Address Line 1,,City 1,State 1,12345,Test Item 2,,4,,,";

            actualCsv.ShouldBe(expectedCsv);

            var expectedOrders = JsonSerializer.Serialize(orders);
            var actualOrders = JsonSerializer.Serialize(mapper.Deserialize<Order>(actualCsv).ToArray());

            actualOrders.ShouldBe(expectedOrders);
        }

        [Fact]
        public void ShouldEscapeString()
        {
            var values = new[]
            {
                "\"the quick brown fox ran fast\"",
                "here, or there?"
            };

            var mapper = new MapperBuilder
            {
                Maps = new List<AbstractMap>()
                {
                    new ValueMap<string>{ DefaultPathSegment = "value" }
                }
            }
            .Build();

            var actualCsv = mapper.Serialize(values);

            var expectedCsv =
@"value
""""""the quick brown fox ran fast""""""
""here, or there?""";

            actualCsv.ShouldBe(expectedCsv);

            var expectedOrders = JsonSerializer.Serialize(values);
            var actualOrders = JsonSerializer.Serialize(mapper.Deserialize<string>(actualCsv).ToArray());

            actualOrders.ShouldBe(expectedOrders);
        }
    }
}
