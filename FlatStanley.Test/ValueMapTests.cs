using FlatStanley.DefaultMaps;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace FlatStanley.Test
{
    public class ValueMapTests
    {
        [Fact]
        public void ShouldUseDefaultPath()
        {
            var numbers = new[] { 1, 2, 3 };

            var mapper = new MapperBuilder
            {
                Maps = new List<AbstractMap>()
                {
                    new ValueMap<int>()
                }
            }
            .Build();

            var actualCsv = mapper.Serialize(numbers);

            var expectedCsv =
@"Value
1
2
3";

            actualCsv.ShouldBe(expectedCsv);

            var expectedOrders = JsonSerializer.Serialize(numbers);
            var actualOrders = JsonSerializer.Serialize(mapper.Deserialize<int>(actualCsv).ToArray());

            actualOrders.ShouldBe(expectedOrders);
        }

        [Fact]
        public void ShouldUseDefaultValue()
        {
            var values = new[] { "Test", null, "", "Test" };

            var mapper = new MapperBuilder
            {
                Maps = new List<AbstractMap>()
                {
                    new ValueMap<string>{ DefaultValue = "-" }
                }
            }
            .Build();

            var actualCsv = mapper.Serialize(values);

            var expectedCsv =
@"Value
Test
-
-
Test";

            actualCsv.ShouldBe(expectedCsv);

            var expectedOrders = JsonSerializer.Serialize(values.Select(x => String.IsNullOrEmpty(x) ? null : x).ToArray());
            var actualOrders = JsonSerializer.Serialize(mapper.Deserialize<string>(actualCsv).ToArray());

            actualOrders.ShouldBe(expectedOrders);
        }
    }
}
