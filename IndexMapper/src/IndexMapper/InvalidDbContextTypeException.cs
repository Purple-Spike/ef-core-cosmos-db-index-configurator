using System;
using System.Runtime.Serialization;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.Mapper;
[Serializable]
internal class InvalidDbContextTypeException : Exception
{
    public InvalidDbContextTypeException()
    {
    }

    public InvalidDbContextTypeException(string? message) : base(message)
    {
    }

    public InvalidDbContextTypeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected InvalidDbContextTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
