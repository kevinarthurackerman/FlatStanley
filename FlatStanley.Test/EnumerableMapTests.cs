using FlatStanley.DefaultMaps;
using FlatStanley.Test.SampleData;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace FlatStanley.Test
{
    public class EnumerableMapTests
    {
        [Fact]
        public void ShouldUseDefaultPath()
        {
            var todos = new[]
            {
                new[]
                {
                    new ToDo{ Id = 1, Description = "Desc 1" },
                    new ToDo{ Id = 2, Description = "Desc 2" },
                    new ToDo{ Id = 3, Description = "Desc 3" }
                },
                new[]
                {
                    new ToDo{ Id = 4, Description = "Desc 4" },
                    new ToDo{ Id = 5, Description = "Desc 5" },
                    new ToDo{ Id = 6, Description = "Desc 6" }
                }
            };

            var mapper = new MapperBuilder
            {
                Maps = new List<AbstractMap>()
                {
                    new ValueMap<int>(),
                    new ValueMap<string>(),
                    new RecordMap<ToDo>(),
                    new EnumerableMap<ToDo>()
                }
            }
            .Build();

            var actualCsv = mapper.Serialize(todos);

            var expectedCsv =
@"Items[0].Id,Items[0].Description,Items[1].Id,Items[1].Description,Items[2].Id,Items[2].Description
1,Desc 1,2,Desc 2,3,Desc 3
4,Desc 4,5,Desc 5,6,Desc 6";

            actualCsv.ShouldBe(expectedCsv);

            var expectedOrders = JsonSerializer.Serialize(todos);
            var actualOrders = JsonSerializer.Serialize(mapper.Deserialize<ToDo[]>(actualCsv).ToArray());

            actualOrders.ShouldBe(expectedOrders);
        }

        [Fact]
        public void ShouldRemoveDefaultItems()
        {
            var todos = new[]
            {
                new[]
                {
                    new ToDo{ Id = 1, Description = "Desc 1" },
                    new ToDo{ Id = 2, Description = "Desc 2" },
                    null
                },
                new[]
                {
                    new ToDo{ Id = 4, Description = "Desc 4" }
                }
            };

            var mapper = new MapperBuilder
            {
                Maps = new List<AbstractMap>()
                {
                    new ValueMap<int>(),
                    new ValueMap<string>(),
                    new RecordMap<ToDo>(),
                    new EnumerableMap<ToDo>()
                }
            }
            .Build();

            var actualCsv = mapper.Serialize(todos);

            var expectedCsv =
@"Items[0].Id,Items[0].Description,Items[1].Id,Items[1].Description,Items[2].Id,Items[2].Description
1,Desc 1,2,Desc 2,,
4,Desc 4,,,,";

            actualCsv.ShouldBe(expectedCsv);

            var expectedOrders = JsonSerializer.Serialize(todos.Select(x => x.Where(y => y != null).ToArray()).ToArray());
            var actualOrders = JsonSerializer.Serialize(mapper.Deserialize<ToDo[]>(actualCsv).ToArray());

            actualOrders.ShouldBe(expectedOrders);
        }
    }
}
