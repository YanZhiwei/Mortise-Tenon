using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenon.AspNetCore.Abstractions.Application;

namespace Tenon.AspNetCore.Controllers;

public abstract class AbstractController : ControllerBase
{
    [NonAction]
    protected virtual ActionResult Problem(string error, int statusCode)
    {
        var problemDetails = new ProblemDetails
        {
            Detail = error,
            Instance = Request.Path.ToString(),
            Status = statusCode,
            Title = null,
            Type = null
        };
        return Problem(problemDetails.Detail
            , problemDetails.Instance
            , problemDetails.Status
            , problemDetails.Title
            , problemDetails.Type);
    }

    [NonAction]
    protected virtual ActionResult Problem(string error, HttpStatusCode statusCode)
    {
        return Problem(error, (int)statusCode);
    }

    [NonAction]
    protected virtual ActionResult<T> Result<T>(T data)
    {
        if (data == null) return NoContent();
        return data;
    }

    [NonAction]
    protected virtual ActionResult Result(ServiceResult serviceResult)
    {
        if (serviceResult.Succeeded)
            return NoContent();
        return Problem(serviceResult.ProblemDetails);
    }

    [NonAction]
    protected virtual ActionResult<T> Result<T>(ServiceResult<T> serviceResult)
    {
        if (serviceResult.Succeeded)
            return serviceResult.Content;
        return Problem(serviceResult.ProblemDetails);
    }

    [NonAction]
    protected virtual ActionResult<T> CreatedResult<T>(T data)
    {
        return Created(Request.Path, data);
    }

    [NonAction]
    protected virtual ActionResult<T> CreatedResult<T>(ServiceResult<T> serviceResult)
    {
        return serviceResult.Succeeded ? Created(Request.Path, serviceResult.Content) : Problem(serviceResult.ProblemDetails);
    }

    [NonAction]
    protected virtual ObjectResult Problem(ProblemDetails problemDetails)
    {
        problemDetails.Instance ??= Request.Path.ToString();
        if (problemDetails is HttpValidationProblemDetails)
            return new BadRequestObjectResult(problemDetails);
        return Problem(problemDetails.Detail
            , problemDetails.Instance
            , problemDetails.Status
            , problemDetails.Title
            , problemDetails.Type);
    }
}