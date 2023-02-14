﻿using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.Post;

public class Summary : ValueObject
{
    public const int MaxLength = 255;

    private Summary()
    {
    }

    private Summary(string value) => Value = value;

    public string Value { get; private set; }

    public static Result<Summary> Create(string summary) =>
        Result.Create(summary)
            .Ensure(s => !string.IsNullOrEmpty(s),
                DomainErrors.Summary.Empty)
            .Ensure(s => s.Length <= MaxLength,
                DomainErrors.Summary.TooLong)
            .Map(s => new Summary(s));

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}