using Microsoft.AspNetCore.Mvc;

namespace MySchool.WebBff.Common.Results;

public static class BffResults
{
    public static IActionResult BadRequestMessage(string message) =>
        new BadRequestObjectResult(new { message });

    public static IActionResult NotFoundMessage(string message) =>
        new NotFoundObjectResult(new { message });
}
