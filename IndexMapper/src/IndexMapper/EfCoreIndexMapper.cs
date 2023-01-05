using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using Microsoft.EntityFrameworkCore;

using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ConfigurationLib;
using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.IndexMapper.PropertyMappers;

using static PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.IndexMapper.MappedIndexes;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.IndexMapper;

public interface IEfCoreIndexMapper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly">Assembly to load from</param>
    /// <param name="contextFullName">Full namespace path of the DBContext to interrogate</param>
    ImmutableArray<MappedIndexes> MapIndexes(Assembly assembly, string contextFullName);
}

public class EfCoreIndexMapper : IEfCoreIndexMapper
{
    private readonly IndexPropertyMapper _indexMapper = new IndexPropertyMapper();
    private readonly PartitionKeyPropertyMapper _partitionKeyMapper = new PartitionKeyPropertyMapper();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly">Assembly to load from</param>
    /// <param name="contextFullName">Full namespace path of the DBContext to interrogate</param>
    public ImmutableArray<MappedIndexes> MapIndexes(Assembly assembly, string contextFullName)
    {
        var customContextType = LoadDbContextTypeInfo(assembly, contextFullName);
        var dbSetProperties = LoadDbSetProperties(customContextType);
        return LoadMappedIndexesFromDbSetProperties(dbSetProperties);
    }

    private ImmutableArray<MappedIndexes> LoadMappedIndexesFromDbSetProperties(ImmutableArray<PropertyInfo> dbSetProperties)
    {
        var builder = ImmutableArray.CreateBuilder<MappedIndexes>(dbSetProperties.Length);

        foreach (var dbSetProperty in dbSetProperties)
        {
            var genericType = dbSetProperty.PropertyType.GenericTypeArguments.Single();
            var partitionKey = _partitionKeyMapper.MapPropertyWithAttribute(genericType, indexPath: "/");
            var indexes = _indexMapper.MapPropertiesWithAttribute(genericType, indexPath: "/");

            var mappedIndexes = new MappedIndexes(ContainerName: dbSetProperty.Name, partitionKey, indexes);
            builder.Add(mappedIndexes);
        }

        return builder.MoveToImmutable();
    }

    private ImmutableArray<PropertyInfo> LoadDbSetProperties(TypeInfo customContextType)
    {
        var dbSetProperties = customContextType.DeclaredProperties.Where(x =>
            x.CanRead
            && x.GetMethod is object
            && !x.GetMethod.IsStatic
            && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
            ).ToImmutableArray();
        return dbSetProperties;
    }

    private TypeInfo LoadDbContextTypeInfo(Assembly assembly, string contextFullName)
    {
        ArgumentNullException.ThrowIfNull(contextFullName);

        var customContextType = assembly.DefinedTypes.FirstOrDefault(x => x.FullName == contextFullName);
        if (customContextType is null)
        {
            throw new MissingContextTypeException($"Could not find define type named `{contextFullName}`");
        }

        if (!customContextType.IsAssignableTo(typeof(DbContext)))
        {
            throw new InvalidDbContextTypeException($"The loaded type `{contextFullName}` does not inherit from `{typeof(DbContext).FullName}`");
        }

        return customContextType;
    }
}
