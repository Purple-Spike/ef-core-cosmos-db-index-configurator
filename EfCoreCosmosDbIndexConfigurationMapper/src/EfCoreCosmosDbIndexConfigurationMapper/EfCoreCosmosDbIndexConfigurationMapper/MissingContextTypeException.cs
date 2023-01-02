﻿using System;
using System.Runtime.Serialization;

namespace PurpleSpikeProductions.EfCoreCosmosDbIndexConfigurator.Mapper;
[Serializable]
internal class MissingContextTypeException : Exception
{
    public MissingContextTypeException()
    {
    }

    public MissingContextTypeException(string? message) : base(message)
    {
    }

    public MissingContextTypeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected MissingContextTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
