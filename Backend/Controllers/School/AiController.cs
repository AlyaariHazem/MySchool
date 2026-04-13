using System.Threading;
using System.Threading.Tasks;
using Backend.DTOS.Ai;
using Backend.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>
/// In-app AI assistant — POST /api/Ai/chat (Angular baseUrl already includes /api).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly SchoolAiAssistantService _assistant;
    private readonly ILogger<AiController> _logger;

    public AiController(SchoolAiAssistantService assistant, ILogger<AiController> logger)
    {
        _assistant = assistant;
        _logger = logger;
    }

    /// <summary>Natural-language chat with OpenAI tool calling (server-side tools only).</summary>
    [Authorize(Roles = "ADMIN,MANAGER,TEACHER")]
    [HttpPost("chat")]
    public async Task<ActionResult<AiChatResponseDto>> Chat([FromBody] AiChatRequestDto? dto, CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest(new AiChatResponseDto { Error = "Request body is required." });

        _logger.LogInformation("AI chat request");
        var result = await _assistant.ChatAsync(dto, cancellationToken);

        if (!string.IsNullOrEmpty(result.Error) && string.IsNullOrEmpty(result.Reply))
            return Ok(result);

        return Ok(result);
    }
}
