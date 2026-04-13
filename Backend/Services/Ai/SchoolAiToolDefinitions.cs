using System.Text.Json.Nodes;

namespace Backend.Services.Ai;

/// <summary>
/// OpenAI Chat Completions "tools" payload — keep in sync with <see cref="SchoolAiToolsService"/>.
/// </summary>
public static class SchoolAiToolDefinitions
{
    public static JsonArray BuildToolsArray()
    {
        var arr = new JsonArray();
        arr.Add(Function(
            "search_student",
            "Search students by full name (partial) or numeric student ID. Returns a list; multiple matches are ambiguous.",
            """
            {
              "type": "object",
              "properties": {
                "query": { "type": "string", "description": "Name text or student ID digits." }
              },
              "required": ["query"]
            }
            """));

        arr.Add(Function(
            "get_student_by_id",
            "Load full student + guardian details for one student id.",
            """
            {
              "type": "object",
              "properties": {
                "studentId": { "type": "integer", "description": "Structured student ID (e.g. 2026110001)." }
              },
              "required": ["studentId"]
            }
            """));

        arr.Add(Function(
            "generate_student_registration_report",
            "Build HTML for the registration form using the same REGISTRATION_FORM template system as the school reports module.",
            """
            {
              "type": "object",
              "properties": {
                "studentId": { "type": "integer", "description": "Student ID." }
              },
              "required": ["studentId"]
            }
            """));

        arr.Add(Function(
            "summarize_student_profile",
            "Concise facts for academic/admin profile (student record + recent attendance counts).",
            """
            {
              "type": "object",
              "properties": {
                "studentId": { "type": "integer", "description": "Student ID." }
              },
              "required": ["studentId"]
            }
            """));

        arr.Add(Function(
            "draft_parent_message",
            "Draft a formal Arabic message to the guardian about the student.",
            """
            {
              "type": "object",
              "properties": {
                "studentId": { "type": "integer", "description": "Student ID." },
                "reason": { "type": "string", "description": "Topic, e.g. repeated absence, behavior, fees." }
              },
              "required": ["studentId", "reason"]
            }
            """));

        arr.Add(Function(
            "search_attendance",
            "Query attendance: by student and/or date range, filter by status, or list students with high absence counts.",
            """
            {
              "type": "object",
              "properties": {
                "studentId": { "type": "integer", "description": "Optional filter." },
                "from": { "type": "string", "description": "Optional start date yyyy-MM-dd." },
                "to": { "type": "string", "description": "Optional end date yyyy-MM-dd." },
                "status": { "type": "string", "description": "Present, Absent, Late, Excused, or All." },
                "highAbsenceOnly": { "type": "boolean", "description": "If true, aggregate absent counts per student." },
                "minAbsences": { "type": "integer", "description": "Threshold when highAbsenceOnly is true (default 5)." },
                "limit": { "type": "integer", "description": "Max rows (default 50)." }
              }
            }
            """));

        return arr;
    }

    private static JsonObject Function(string name, string description, string parametersJson)
    {
        var fn = new JsonObject
        {
            ["name"] = name,
            ["description"] = description,
            ["parameters"] = JsonNode.Parse(parametersJson)!
        };
        return new JsonObject
        {
            ["type"] = "function",
            ["function"] = fn
        };
    }
}
