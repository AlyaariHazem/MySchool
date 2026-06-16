using Microsoft.AspNetCore.Mvc;

namespace MySchool.WebBff.Abstracts;

public abstract partial class RestfulBffControllerBase<TEntityDto, TCreateDto, TUpdateDto>
{
    [NonAction]
    public virtual async Task<IActionResult> Add(
        [FromBody] TCreateDto request,
        CancellationToken cancellationToken)
    {
        var created = await AddAsync(request, cancellationToken);
        return Created(string.Empty, created);
    }

    [NonAction]
    protected virtual Task<TEntityDto> AddAsync(TCreateDto request, CancellationToken cancellationToken) =>
        throw new NotSupportedException();
}
