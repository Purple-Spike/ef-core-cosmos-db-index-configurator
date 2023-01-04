using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using Microsoft.EntityFrameworkCore;

using PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.ConfigurationLib;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.Mapper;

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
            var includedIndexes = LoadIncludedIndexesForType(genericType, indexPath: "/");

            var mappedIndexes = new MappedIndexes(Container: dbSetProperty.Name, includedIndexes);
            builder.Add(mappedIndexes);
        }

        return builder.MoveToImmutable();
    }

    private ImmutableArray<MappedIndexes.MappedIndex> LoadIncludedIndexesForType(Type genericType, string indexPath)
    {
        var builder = ImmutableArray.CreateBuilder<MappedIndexes.MappedIndex>();

        foreach (var property in genericType.GetRuntimeProperties())
        {
            if (property.GetMethod is object
                && !property.GetMethod.IsStatic)
            {
                var propertyIndexPath = $"{indexPath}{property.Name}/";

                var includeIndexAttr = property.GetCustomAttribute<IncludeIndexAttribute>();
                if (includeIndexAttr is object)
                {
                    MappedIndexes.MappedIndex mappedIndex;
                    if (IsPropertyScalar(property))
                    {
                        mappedIndex = new MappedIndexes.MappedIndex($"{propertyIndexPath}?");
                    }
                    else
                    {
                        mappedIndex = new MappedIndexes.MappedIndex($"{propertyIndexPath}*");
                    }

                    builder.Add(mappedIndex);
                }
                else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable)))
                {
                    if (property.PropertyType.IsGenericType)
                    {
                        //List<> or Something like that
                        foreach (var genericArgumentType in property.PropertyType.GenericTypeArguments)
                        {
                            var collectionSubTypes = LoadIncludedIndexesForType(genericArgumentType, $"{propertyIndexPath}[]/");
                            builder.AddRange(collectionSubTypes);
                        }
                    }
                    else if (property.PropertyType.IsArray)
                    {
                        //An array
                        var elementType = property.PropertyType.GetElementType();
                        if (elementType is object)
                        {
                            var collectionSubTypes = LoadIncludedIndexesForType(elementType, $"{propertyIndexPath}[]/");
                            builder.AddRange(collectionSubTypes);
                        }
                    }
                }
                else if (property.PropertyType != typeof(object) 
                    && !IsPropertyScalar(property))
                {
                    var objectSubTypes = LoadIncludedIndexesForType(property.PropertyType, $"{propertyIndexPath}");
                    builder.AddRange(objectSubTypes);
                }
            }
        }

        return builder.ToImmutable();
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

    private bool IsPropertyScalar(PropertyInfo property)
    {
        return property.PropertyType.IsAssignableTo(typeof(string))
               || property.PropertyType.IsAssignableTo(typeof(int))
               || property.PropertyType.IsAssignableTo(typeof(decimal))
               || property.PropertyType.IsAssignableTo(typeof(double))
               || property.PropertyType.IsAssignableTo(typeof(float))
               || property.PropertyType.IsAssignableTo(typeof(long))
               || property.PropertyType.IsAssignableTo(typeof(short))
               || property.PropertyType.IsAssignableTo(typeof(bool))
               || property.PropertyType.IsAssignableTo(typeof(Guid))
               || property.PropertyType.IsAssignableTo(typeof(DateTime))
               || property.PropertyType.IsAssignableTo(typeof(DateOnly))
               || property.PropertyType.IsAssignableTo(typeof(TimeOnly));
    }
}
