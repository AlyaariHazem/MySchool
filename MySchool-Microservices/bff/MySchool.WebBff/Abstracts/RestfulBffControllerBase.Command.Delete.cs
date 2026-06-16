using Microsoft.AspNetCore.Mvc;

namespace MySchool.WebBff.Abstracts;

public abstract partial class RestfulBffControllerBase<TEntityDto, TCreateDto, TUpdateDto>
{
    [NonAction]
    public virtual async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [NonAction]
    protected virtual Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(false);
}
