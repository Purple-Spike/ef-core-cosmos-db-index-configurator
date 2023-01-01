using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ExampleLib.Entities;

public class CustomerEntity
{
    public required Guid EntityId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required int BirthYear { get; set; }

    public required List<OrderSubEntity> Orders { get; set; }

    public class OrderSubEntity
    {
        public required Guid EntityId { get; set; }
        public required Guid OrderNumber { get; set; }
        public required string ProductName { get; set; }
    }
}
