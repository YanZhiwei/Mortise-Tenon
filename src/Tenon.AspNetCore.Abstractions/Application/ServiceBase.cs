using System.Net;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Tenon.AspNetCore.Abstractions.Application;

public abstract class ServiceBase
{
    protected static ServiceResult CreateServiceResult()
    {
        return ServiceResult.Success;
    }

    protected ProblemDetails Problem(HttpStatusCode? statusCode = null, string? detail = null, string? title = null,
        string? instance = null, string? type = null)
    {
        return new ProblemDetails
        {
            Status = statusCode.HasValue ? (int)statusCode.Value : (int)HttpStatusCode.BadRequest,
            Detail = detail,
            Title = title,
            Instance = instance,
            Type = type
        };
    }
}