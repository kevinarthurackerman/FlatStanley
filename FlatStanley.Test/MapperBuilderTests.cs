using FlatStanley.DefaultMaps;
using FlatStanley.Test.SampleData;
using Shouldly;
using System.Linq;
using System.Reflection;
using Xunit;

namespace FlatStanley.Test
{
    public class MapperBuilderTests
    {
        [Fact]
        public void ShouldRegisterAllMaps()
        {
            var builder = new MapperBuilder().RegisterAssembly(Assembly.GetExecutingAssembly());

            var actual = builder.Maps.Select(x => x.GetType()).ToArray();

            var expected = new[]
                {
                    typeof(AddressMap),
                    typeof(OrderMap),
                    typeof(OrderLineMap)
                };

            actual.ShouldBeEquivalentTo(expected);
        }

        public class AddressMap : RecordMap<Address> { }
        public class OrderMap : RecordMap<Order> { }
        public class OrderLineMap : RecordMap<OrderLine> { }
    }
}
