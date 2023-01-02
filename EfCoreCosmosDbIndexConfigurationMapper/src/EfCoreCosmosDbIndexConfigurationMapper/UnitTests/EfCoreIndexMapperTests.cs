using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.Mapper;

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
        _mapper.MapIndexes(assembly, contextPath);
    }
}
