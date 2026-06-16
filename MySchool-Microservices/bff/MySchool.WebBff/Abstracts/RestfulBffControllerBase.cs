using Microsoft.AspNetCore.Mvc;

namespace MySchool.WebBff.Abstracts;

/// <summary>
/// Reusable REST surface for BFF resources backed by gRPC.
/// Derived controllers override actions and enable routes with HTTP verb attributes.
/// </summary>
public abstract partial class RestfulBffControllerBase<TEntityDto, TCreateDto, TUpdateDto> : ControllerBase
    where TEntityDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    protected CancellationToken RequestCancellation => HttpContext.RequestAborted;

    protected IActionResult NotImplementedAction(string actionName) =>
        StatusCode(StatusCodes.Status501NotImplemented, new { message = $"{actionName} is not implemented for this resource." });
}
