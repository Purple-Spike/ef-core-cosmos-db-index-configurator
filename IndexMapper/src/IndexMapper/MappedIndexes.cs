using System.Collections.Immutable;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.IndexMapper;

public record MappedIndexes(string Container, ImmutableArray<MappedIndexes.MappedIndex> IncludedIndexes)
{
    public record MappedIndex(string Path);
}
