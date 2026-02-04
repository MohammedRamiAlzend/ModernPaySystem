using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace ModernPaySystem.Domain.Commons;

public sealed class Result<TValue> : IResult<TValue>
{
    private readonly TValue? _value = default;
    private readonly List<Error>? _errors = null;
    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;

    public List<Error> Errors => IsError ? _errors! : new List<Error>();
    public TValue? Value => IsSuccess ? _value! : default;

    public Error TopError => (_errors?.Count > 0) ? _errors[0] : default;

    [JsonConstructor]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Result(TValue? value, List<Error>? errors, bool isSuccess)
    {
        if (isSuccess)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
            _errors = new List<Error>();
            IsSuccess = true;
        }
        else
        {
            if (errors == null || errors.Count == 0)
            {
                throw new ArgumentNullException("provide at least one error", nameof(errors));
            }

            _errors = errors;
            _value = default;
            IsSuccess = false;
        }
    }

    private Result(Error error)
    {
        _errors = new List<Error> { error };
        IsSuccess = false;
    }

    private Result(List<Error> errors)
    {
        if (errors == null || errors.Count == 0)
        {
            throw new ArgumentException(
                "Cannot create an ErrorOr<TValue> from an empty collection of errors. provide at least one error.",
                nameof(errors));
        }

        _errors = errors;
        IsSuccess = false;
    }

    private Result(TValue value)
    {
        if (value is null)
        {
            throw new ArgumentException(null, nameof(value));
        }

        _value = value;
        IsSuccess = true;
    }

    public static implicit operator Result<TValue>(TValue value)
        => new(value);
    public static implicit operator Result<TValue>(Error error)
        => new(error);
    public static implicit operator Result<TValue>(List<Error> errors)
        => new(errors);
}
