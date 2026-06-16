using Microsoft.AspNetCore.Mvc;
using MySchool.WebBff.Common.DTOs;

namespace MySchool.WebBff.Abstracts;

public abstract partial class RestfulBffControllerBase<TEntityDto, TCreateDto, TUpdateDto>
{
    [NonAction]
    public virtual async Task<IActionResult> GetPage(
        [FromBody] CommonGetPageRequestDto request,
        CancellationToken cancellationToken)
    {
        var page = await GetPageAsync(request, cancellationToken);
        return Ok(page);
    }

    [NonAction]
    protected virtual Task<CommonPageResponseDto<TEntityDto>> GetPageAsync(
        CommonGetPageRequestDto request,
        CancellationToken cancellationToken) =>
        Task.FromResult(new CommonPageResponseDto<TEntityDto>());
}
