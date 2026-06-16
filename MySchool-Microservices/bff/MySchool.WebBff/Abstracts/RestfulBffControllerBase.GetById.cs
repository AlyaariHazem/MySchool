using Microsoft.AspNetCore.Mvc;

namespace MySchool.WebBff.Abstracts;

public abstract partial class RestfulBffControllerBase<TEntityDto, TCreateDto, TUpdateDto>
{
    [NonAction]
    public virtual async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [NonAction]
    protected virtual Task<TEntityDto?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult<TEntityDto?>(null);
}
