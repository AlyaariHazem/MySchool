/** Mirrors Backend.DTOS.Ai — keep aligned with C# models. */
export interface AiChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

export interface AiChatRequest {
  message?: string;
  messages?: AiChatMessage[];
}

export interface AiToolStep {
  toolName: string;
  argumentsJson?: string | null;
  resultJson: string;
}

export interface AiChatResponse {
  reply: string;
  toolSteps: AiToolStep[];
  error?: string | null;
}

/** UI-only message (includes optional tool trace for display). */
export interface AiUiMessage {
  role: 'user' | 'assistant';
  content: string;
  toolSteps?: AiToolStep[];
  error?: string;
}
