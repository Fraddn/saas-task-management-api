using System;

namespace ProjectSaas.Api.Application.Exceptions;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message) : base(message) { }
}
