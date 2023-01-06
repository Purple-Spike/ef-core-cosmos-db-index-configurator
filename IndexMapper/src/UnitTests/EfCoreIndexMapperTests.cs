﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.IndexMapper;
using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.IndexMapper.PropertyMappers;
using Shouldly;

using Xunit;

namespace UnitTests;

public class EfCoreIndexMapperTests
{
    private readonly EfCoreIndexMapper _mapper = new EfCoreIndexMapper();

    [Fact]
    public void WhenLoadingTypes()
    {
        var pwd = Directory.GetCurrentDirectory();
        var resultPath = pwd + $"/ExampleLib.dll";
        var assembly = Assembly.LoadFrom(resultPath);
        var contextPath = "PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ExampleLib.MyDbContext";
        var mappedIndexes = _mapper.MapIndexes(assembly, contextPath);

        mappedIndexes.Length.ShouldBe(2);

        var customersContainer = mappedIndexes.Single(x => x.ContainerName == "Customers");
        customersContainer.PartitionKey.ShouldBe("/EntityId");

        customersContainer.IncludedIndexes.Length.ShouldBe(12);

        customersContainer.IncludedIndexes.Any(x => x == "/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x == "/FirstName/?").ShouldBeTrue();

        customersContainer.IncludedIndexes.Any(x => x == "/OrdersList/[]/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x == "/OrdersList/[]/OrderNumber/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x == "/OrdersList/[]/ProductSummary/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x == "/OrdersList/[]/ProductSummary/Name/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x == "/OrdersList/[]/ProductSummary/Review/EntityId/?").ShouldBeTrue();

        customersContainer.IncludedIndexes.Any(x => x == "/OrdersArray/[]/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x == "/OrdersArray/[]/OrderNumber/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x == "/OrdersArray/[]/ProductSummary/EntityId/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x == "/OrdersArray/[]/ProductSummary/Name/?").ShouldBeTrue();
        customersContainer.IncludedIndexes.Any(x => x == "/OrdersArray/[]/ProductSummary/Review/EntityId/?").ShouldBeTrue();

        var productsContainer = mappedIndexes.Single(x => x.ContainerName == "Products");

        productsContainer.PartitionKey.ShouldBeNull();

        productsContainer.IncludedIndexes.Length.ShouldBe(9);

        productsContainer.IncludedIndexes.Any(x => x == "/EntityId/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x == "/ProductName/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x == "/IsEnabled/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x == "/NonDecimalPrice/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x == "/DisplayPrice/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x == "/CreateDateTime/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x == "/PopularityRating/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x == "/DisplayOrder/?").ShouldBeTrue();
        productsContainer.IncludedIndexes.Any(x => x == "/UserPoints/?").ShouldBeTrue();
    }
}
