using Microsoft.AspNetCore.Mvc;

namespace MySchool.WebBff.Abstracts;

public abstract partial class RestfulBffControllerBase<TEntityDto, TCreateDto, TUpdateDto>
{
    [NonAction]
    public virtual async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [NonAction]
    protected virtual Task<IEnumerable<TEntityDto>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Enumerable.Empty<TEntityDto>());
}
