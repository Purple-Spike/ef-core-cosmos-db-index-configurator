using System.Collections.Immutable;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.Mapper;

public record MappedIndexes(string Container, ImmutableArray<MappedIndexes.MappedIndex> IncludedIndexes)
{
    public record MappedIndex(string Path);
}
