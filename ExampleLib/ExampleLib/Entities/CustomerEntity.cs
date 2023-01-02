using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ConfigurationLib;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ExampleLib.Entities;

public class CustomerEntity
{
    [IncludeIndex]
    public required Guid EntityId { get; set; }

    [IncludeIndex]
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required int BirthYear { get; set; }

    public required List<OrderSubEntity> Orders { get; set; }

    public class OrderSubEntity
    {
        [IncludeIndex]
        public required Guid EntityId { get; set; }

        [IncludeIndex]
        public required Guid OrderNumber { get; set; }

        public required string ProductName { get; set; }
    }
}
