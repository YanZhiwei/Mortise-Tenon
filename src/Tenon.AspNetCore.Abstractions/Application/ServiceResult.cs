using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Tenon.AspNetCore.Abstractions.Application;

[Serializable]
public sealed class ServiceResult
{
    public ServiceResult(ProblemDetails problemDetails)
    {
        ProblemDetails = problemDetails;
    }

    public ServiceResult()
    {
    }

    public static ServiceResult Success { get; } = new();
    public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public ProblemDetails? ProblemDetails { get; set; }

    public bool Succeeded => ProblemDetails == null;

    public static implicit operator ServiceResult(ProblemDetails problemDetails)
    {
        return new ServiceResult
        {
            ProblemDetails = problemDetails
        };
    }

    public static ServiceResult Error(HttpStatusCode statusCode, string title, string? detail = null)
    {
        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail
        };
        return new ServiceResult(problem);
    }

    public static ServiceResult<T> Error<T>(HttpStatusCode statusCode, string title, string? detail = null)
    {
        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail
        };
        return new ServiceResult<T>(problem);
    }

    public static ServiceResult NotFound(string title, string? detail = null)
        => Error(HttpStatusCode.NotFound, title, detail);

    public static ServiceResult BadRequest(string title, string? detail = null)
        => Error(HttpStatusCode.BadRequest, title, detail);

    public static ServiceResult Conflict(string title, string? detail = null)
        => Error(HttpStatusCode.Conflict, title, detail);

    public static ServiceResult<T> SuccessResult<T>(T value) => new(value);
    public static ServiceResult<T> Empty<T>() => new();

    public static ServiceResult<T> NotFound<T>(string title, string? detail = null)
        => Error<T>(HttpStatusCode.NotFound, title, detail);

    public static ServiceResult<T> BadRequest<T>(string title, string? detail = null)
        => Error<T>(HttpStatusCode.BadRequest, title, detail);

    public static ServiceResult<T> Conflict<T>(string title, string? detail = null)
        => Error<T>(HttpStatusCode.Conflict, title, detail);
}

[Serializable]
public sealed class ServiceResult<TValue>
{
    public ServiceResult()
    {
    }

    public ServiceResult(TValue value)
    {
        Content = value;
    }

    public ServiceResult(ProblemDetails problemDetails)
    {
        ProblemDetails = problemDetails;
    }

    public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public bool Succeeded => ProblemDetails == null && Content != null;

    public TValue Content { get; set; } = default!;

    public ProblemDetails? ProblemDetails { get; set; }

    public static implicit operator ServiceResult<TValue>(ServiceResult result)
    {
        return new ServiceResult<TValue>
        {
            ProblemDetails = result.ProblemDetails
        };
    }

    public static implicit operator ServiceResult<TValue>(ProblemDetails problemDetails)
    {
        return new ServiceResult<TValue>
        {
            ProblemDetails = problemDetails
        };
    }

    public static implicit operator ServiceResult<TValue>(TValue value)
    {
        return new ServiceResult<TValue>(value);
    }
}