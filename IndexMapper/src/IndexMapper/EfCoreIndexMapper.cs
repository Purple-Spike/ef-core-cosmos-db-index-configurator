using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

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
    ImmutableArray<MappedIndexes> MapIndexes(string assemblyPath, string contextNamespace, string contextClass);
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
    public ImmutableArray<MappedIndexes> MapIndexes(string assemblyPath, string contextNamespace, string contextClass)
    {
        ArgumentNullException.ThrowIfNull(assemblyPath);
        ArgumentNullException.ThrowIfNull(contextNamespace);
        ArgumentNullException.ThrowIfNull(contextClass);

        using var fs = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var peReader = new PEReader(fs);

        MetadataReader reader = peReader.GetMetadataReader();

        var customContextType = LoadDbContextTypeInfo(reader, contextNamespace, contextClass);
        var dbSetProperties = LoadDbSetProperties(reader, customContextType);
        throw new Exception("Testing");
        //return LoadMappedIndexesFromDbSetProperties(dbSetProperties);
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

    private ImmutableArray<PropertyInfo> LoadDbSetProperties(MetadataReader reader, TypeDefinition contextType)
    {
        var builder = ImmutableArray.CreateBuilder<PropertyInfo>();

        foreach (var propertyHandle in contextType.GetProperties())
        {
            var property = reader.GetPropertyDefinition(propertyHandle);
            property.DecodeSignature(new EcmaSignatureTypeProviderForToString.Instance)
        }

        var dbSetProperties = customContextType.DeclaredProperties.Where(x =>
            x.CanRead
            && x.GetMethod is object
            && !x.GetMethod.IsStatic
            //&& x.PropertyType.FullName.StartsWith.GetGenericTypeDefinition() == typeof(DbSet<>)
            && x.PropertyType.FullName?.StartsWith("Microsoft.EntityFrameworkCore.DbSet'") is true
            ).ToImmutableArray();
        return builder.ToImmutable();
    }

    private TypeDefinition LoadDbContextTypeInfo(MetadataReader reader, string contextNamespace, string contextClass)
    {
        foreach (var defHandle in reader.TypeDefinitions)
        {
            var typeDef = reader.GetTypeDefinition(defHandle);
            var typeName = reader.GetString(typeDef.Name);
            var typeNamespace = reader.GetString(typeDef.Namespace);

            if (typeName == contextClass
                && typeNamespace == contextNamespace)
            {
                //var baseType = reader.GetTypeReference((TypeReferenceHandle)typeDef.BaseType);
                return typeDef;
            }
        }

        throw new MissingContextTypeException($"Could not find define type named `{contextNamespace}.{contextClass}`");
    }
}
