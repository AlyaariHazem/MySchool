using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Backend.DTOS.Ai;

public sealed class AiChatMessageDto
{
    /// <summary>user | assistant (system messages from the client are ignored server-side).</summary>
    public string Role { get; set; } = "user";

    public string Content { get; set; } = string.Empty;
}

public sealed class AiChatRequestDto
{
    /// <summary>Convenience: single user turn (combined with <see cref="Messages"/> if both sent).</summary>
    public string? Message { get; set; }

    /// <summary>Optional prior turns for multi-turn chat (roles: user, assistant).</summary>
    public List<AiChatMessageDto>? Messages { get; set; }
}

public sealed class AiToolStepDto
{
    public string ToolName { get; set; } = string.Empty;

    /// <summary>Raw JSON arguments passed to the tool.</summary>
    public string? ArgumentsJson { get; set; }

    /// <summary>JSON string returned to the model (bounded size).</summary>
    public string ResultJson { get; set; } = "{}";
}

public sealed class AiChatResponseDto
{
    /// <summary>Final assistant reply for the UI.</summary>
    public string Reply { get; set; } = string.Empty;

    /// <summary>Tool executions performed during this request (for transparency / debugging).</summary>
    public List<AiToolStepDto> ToolSteps { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }
}
