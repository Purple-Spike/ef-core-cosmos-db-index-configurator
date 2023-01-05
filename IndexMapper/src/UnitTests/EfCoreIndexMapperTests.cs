using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.IndexMapper;

using Shouldly;

using Xunit;

namespace UnitTests;

public class EfCoreIndexMapperTests
{
    private readonly EfCoreIndexMapper _mapper = new EfCoreIndexMapper();

    [Fact]
    public void WhenLoadingTypes()
    {
        var releaseMode = "Release";
#if DEBUG
        releaseMode = "Debug";
#endif

        var pwd = Directory.GetCurrentDirectory();
        var resultPath = pwd + $"/../../../../ExampleLib/bin/{releaseMode}/net7.0/ExampleLib.dll";
        var assembly = Assembly.LoadFile(resultPath);
        var contextPath = "PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ExampleLib.MyDbContext";
        var mappedIndexes = _mapper.MapIndexes(assembly, contextPath);

        mappedIndexes.Length.ShouldBe(2);

        var customersContainer = mappedIndexes.Single(x => x.Container == "Customers");
        customersContainer.IncludedIndexes.Length.ShouldBe(12);

        customersContainer.IncludedIndexes.Any(x => x.Path == "/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/FirstName/?").ShouldBeTrue();

        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersList/[]/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersList/[]/OrderNumber/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersList/[]/ProductSummary/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersList/[]/ProductSummary/Name/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersList/[]/ProductSummary/Review/EntityId/?").ShouldBeTrue();

        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersArray/[]/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersArray/[]/OrderNumber/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersArray/[]/ProductSummary/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersArray/[]/ProductSummary/Name/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x.Path == "/OrdersArray/[]/ProductSummary/Review/EntityId/?").ShouldBeTrue();

        var productsContainer = mappedIndexes.Single(x => x.Container == "Products");

        productsContainer.IncludedIndexes.Length.ShouldBe(9);

        productsContainer.IncludedIndexes.Any(x => x.Path == "/EntityId/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x.Path == "/ProductName/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x.Path == "/IsEnabled/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x.Path == "/NonDecimalPrice/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x.Path == "/DisplayPrice/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x.Path == "/CreateDateTime/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x.Path == "/PopularityRating/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x.Path == "/DisplayOrder/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x.Path == "/UserPoints/?").ShouldBeTrue();
    }
}
