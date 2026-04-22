using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Backend.Configuration;
using Backend.DTOS.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;

namespace Backend.Services.Ai;

/// <summary>
/// Orchestrates OpenAI chat completions with tool calling — executes tenant-scoped tools server-side only.
/// </summary>
public sealed class SchoolAiAssistantService
{
    private readonly OpenAiChatCompletionService _openAi;
    private readonly SchoolAiToolsService _tools;
    private readonly IOptions<OpenAiOptions> _options;
    private readonly ILogger<SchoolAiAssistantService> _logger;

    public SchoolAiAssistantService(
        OpenAiChatCompletionService openAi,
        SchoolAiToolsService tools,
        IOptions<OpenAiOptions> options,
        ILogger<SchoolAiAssistantService> logger)
    {
        _openAi = openAi;
        _tools = tools;
        _options = options;
        _logger = logger;
    }

    public async Task<AiChatResponseDto> ChatAsync(AiChatRequestDto request, CancellationToken cancellationToken)
    {
        var steps = new List<AiToolStepDto>();
        var messages = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "system",
                ["content"] = BuildSystemPrompt()
            }
        };

        foreach (var (role, content) in BuildHistory(request))
        {
            messages.Add(new JsonObject
            {
                ["role"] = role,
                ["content"] = content
            });
        }

        if (messages.Count <= 1)
            return new AiChatResponseDto { Error = "No user message provided." };

        var tools = SchoolAiToolDefinitions.BuildToolsArray();
        var model = _options.Value.Model;
        var maxLoops = Math.Clamp(_options.Value.MaxToolIterations, 1, 32);

        for (var i = 0; i < maxLoops; i++)
        {
            var body = new JsonObject
            {
                ["model"] = model,
                ["messages"] = messages.DeepClone(),
                ["tools"] = tools.DeepClone(),
                ["temperature"] = 0.2
            };

            string raw;
            try
            {
                raw = await _openAi.SendChatCompletionRawAsync(body.ToJsonString(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI request failed");
                return new AiChatResponseDto { Error = ex.Message, ToolSteps = steps };
            }

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errEl))
            {
                var msg = errEl.TryGetProperty("message", out var m) ? m.GetString() : "OpenAI error";
                return new AiChatResponseDto { Error = msg, ToolSteps = steps };
            }

            var choice = root.GetProperty("choices")[0];
            var finish = choice.TryGetProperty("finish_reason", out var fr)
                ? fr.GetString()
                : null;
            var message = choice.GetProperty("message");

            if (finish == "tool_calls" &&
                message.TryGetProperty("tool_calls", out var toolCallsEl) &&
                toolCallsEl.ValueKind == JsonValueKind.Array)
            {
                messages.Add(JsonNode.Parse(message.GetRawText())!);

                foreach (var tc in toolCallsEl.EnumerateArray())
                {
                    var id = tc.GetProperty("id").GetString() ?? "";
                    var fn = tc.GetProperty("function");
                    var name = fn.GetProperty("name").GetString() ?? "";
                    var args = fn.TryGetProperty("arguments", out var aEl) ? aEl.GetString() : "{}";

                    _logger.LogInformation("AI executing tool {Tool}", name);
                    var result = await _tools.ExecuteAsync(name, args ?? "{}", cancellationToken);
                    steps.Add(new AiToolStepDto
                    {
                        ToolName = name,
                        ArgumentsJson = args,
                        ResultJson = result
                    });

                    messages.Add(new JsonObject
                    {
                        ["role"] = "tool",
                        ["tool_call_id"] = id,
                        ["content"] = result
                    });
                }

                continue;
            }

            string? content = null;
            if (message.TryGetProperty("content", out var cEl) && cEl.ValueKind == JsonValueKind.String)
                content = cEl.GetString();

            return new AiChatResponseDto
            {
                Reply = content ?? string.Empty,
                ToolSteps = steps
            };
        }

        return new AiChatResponseDto
        {
            Error = "Maximum tool iterations reached — try a narrower question.",
            ToolSteps = steps
        };
    }

    private static string BuildSystemPrompt() =>
        """
        You are the in-school AI assistant for school staff and administrators.
        You MUST call the provided tools to read students or attendance; never invent student IDs, names, or attendance rows.
        If search_student returns multiple students, ask the user to pick a studentId or refine the name.
        If a tool reports ok:false or missing data, explain clearly and suggest next steps.
        Respond in concise Modern Standard Arabic when the user writes in Arabic; otherwise match the user's language.
        This is read-only: do not claim you created, updated, or deleted database records.
        """;

    private static IEnumerable<(string Role, string Content)> BuildHistory(AiChatRequestDto request)
    {
        if (request.Messages != null)
        {
            foreach (var m in request.Messages)
            {
                var role = (m.Role ?? "user").Trim().ToLowerInvariant();
                if (role is not ("user" or "assistant"))
                    continue;
                if (string.IsNullOrWhiteSpace(m.Content))
                    continue;
                yield return (role, m.Content.Trim());
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Message))
            yield return ("user", request.Message.Trim());
    }
}
