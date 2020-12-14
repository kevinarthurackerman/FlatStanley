using FlatStanley.DefaultMaps;
using FlatStanley.Test.SampleData;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace FlatStanley.Test
{
    public class RecordMapTests
    {
        [Fact]
        public void ShouldUseDefaultPath()
        {
            var todos = new[]
            {
                new ToDo{ Id = 1, Description = "Desc 1" },
                new ToDo{ Id = 2, Description = "Desc 2" },
                new ToDo{ Id = 3, Description = "Desc 3" }
            };

            var mapper = new MapperBuilder
            {
                Maps = new List<AbstractMap>()
                {
                    new ValueMap<int>(),
                    new ValueMap<string>(),
                    new RecordMap<ToDo>()
                }
            }
            .Build();

            var actualCsv = mapper.Serialize(todos);

            var expectedCsv =
@"ToDo.Id,ToDo.Description
1,Desc 1
2,Desc 2
3,Desc 3";

            actualCsv.ShouldBe(expectedCsv);

            var expectedOrders = JsonSerializer.Serialize(todos);
            var actualOrders = JsonSerializer.Serialize(mapper.Deserialize<ToDo>(actualCsv).ToArray());

            actualOrders.ShouldBe(expectedOrders);
        }
    }
}
