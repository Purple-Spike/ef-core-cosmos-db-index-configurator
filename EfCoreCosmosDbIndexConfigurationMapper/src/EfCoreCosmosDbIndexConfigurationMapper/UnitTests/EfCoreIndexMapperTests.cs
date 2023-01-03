using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.Mapper;

using Shouldly;

using Xunit;

namespace UnitTests;

public class EfCoreIndexMapperTests
{
    private readonly EfCoreIndexMapper _mapper = new EfCoreIndexMapper();

    [Fact]
    public void WhenLoadingTypes()
    {
        var assembly = Assembly.LoadFile("C:/GitHub/Purple-Spike/ef-core-cosmos-db-index-configurator/ExampleLib/ExampleLib/bin/Debug/net7.0/ExampleLib.dll");
        var contextPath = "PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ExampleLib.MyDbContext";
        var mappedIndexes = _mapper.MapIndexes(assembly, contextPath);
        
        mappedIndexes.Length.ShouldBe(1);
        
        var customersContainer = mappedIndexes.Single(x => x.Container == "Customers");
        customersContainer.IncludedIndexes.Length.ShouldBe(6);
        customersContainer.IncludedIndexes.Any(x => x.Path == "/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/FirstName/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersList/[]/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersList/[]/OrderNumber/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersArray/[]/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersArray/[]/OrderNumber/?").ShouldBeTrue();
    }
}
