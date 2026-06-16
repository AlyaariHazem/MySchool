using Microsoft.AspNetCore.Mvc;

namespace MySchool.WebBff.Abstracts;

public abstract partial class RestfulBffControllerBase<TEntityDto, TCreateDto, TUpdateDto>
{
    [NonAction]
    public virtual async Task<IActionResult> Update(
        string id,
        [FromBody] TUpdateDto request,
        CancellationToken cancellationToken)
    {
        var updated = await UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [NonAction]
    protected virtual Task<TEntityDto?> UpdateAsync(
        string id,
        TUpdateDto request,
        CancellationToken cancellationToken) =>
        Task.FromResult<TEntityDto?>(null);
}
