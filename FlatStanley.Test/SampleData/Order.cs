using System.Collections.Generic;

namespace FlatStanley.Test.SampleData
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public Address Address { get; set; }
        public IEnumerable<OrderLine> OrderLines { get; set; } = new OrderLine[0];
    }
}
