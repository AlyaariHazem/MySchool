/** UI chat message (and optional tool trace from legacy backend). */
export interface AiChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

/** minimal-agent POST /api/chat body (camelCase). */
export type SchoolAiSupportUserRole = 'student' | 'teacher' | 'parent' | 'admin';

export interface SchoolAiSupportChatRequest {
  message: string;
  userRole: SchoolAiSupportUserRole;
  conversationId?: string | null;
  userName?: string | null;
}

export interface SchoolAiSupportChatResponse {
  conversationId: string;
  response: string;
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
  /** Returned by minimal-agent for follow-up turns (server-side memory). */
  conversationId?: string;
}

/** UI-only message (includes optional tool trace for display). */
export interface AiUiMessage {
  role: 'user' | 'assistant';
  content: string;
  toolSteps?: AiToolStep[];
  error?: string;
}
