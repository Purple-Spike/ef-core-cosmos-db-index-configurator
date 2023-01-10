using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Identity;

using Microsoft.EntityFrameworkCore;

using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ExampleLib.Entities;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ExampleLib;

public class MyDbContext : DbContext
{
    private readonly MyDatabaseConfig _databaseConfig;
    private readonly TokenCredential _azureTokenCredential;

    [NotNull]
    public DbSet<CustomerEntity>? Customers { get; set; }

    [NotNull]
    public DbSet<ProductEntity>? Products { get; set; }

    [NotNull]
    public DbSet<OrderEntity>? Orders { get; set; }
    
    public MyDbContext(DbContextOptions<MyDbContext> options, MyDatabaseConfig databaseConfig)
        : base(options)
    {
        _databaseConfig = databaseConfig;
        _azureTokenCredential = new DefaultAzureCredential();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseCosmos(_databaseConfig.AccountEndpoint, _azureTokenCredential, _databaseConfig.DatabaseName, options =>
        {
            //Limit to the given endpoint to reduce calls made on startup for multiple locations
            //  We use Serverless, so there's only 1 location
            _ = options.LimitToEndpoint(true);

            _ = options.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Direct);
        })
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<CustomerEntity>()
            .ToContainer(nameof(Customers))
            .HasPartitionKey(x => x.EntityId)
            .HasNoDiscriminator()
            .HasKey(x => x.EntityId);

        _ = modelBuilder.Entity<ProductEntity>()
            .ToContainer(nameof(Products))
            .HasPartitionKey(x => x.EntityId)
            .HasNoDiscriminator()
            .HasKey(x => x.EntityId);
    }
}
