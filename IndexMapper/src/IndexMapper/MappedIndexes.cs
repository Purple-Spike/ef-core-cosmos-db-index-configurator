using System.Collections.Immutable;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.IndexMapper;

public record MappedIndexes(string ContainerName, string? PartitionKey, ImmutableArray<string> IncludedIndexes);
