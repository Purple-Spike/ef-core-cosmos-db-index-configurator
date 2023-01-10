
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ConfigurationLib;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ExampleLib.Entities;

public record OrderEntity([property: IncludePartitionKey][property: IncludeIndex] Guid EntityId, [property: IncludeIndex] string ProductName);

