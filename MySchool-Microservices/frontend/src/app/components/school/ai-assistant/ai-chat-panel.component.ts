import { Component, EventEmitter, Input, Output } from '@angular/core';

import type { AiUiMessage } from './ai-assistant.models';

/**
 * Reusable chat shell: message list + input (used by AI Assistant page; extend for other bots later).
 */
@Component({
  selector: 'app-ai-chat-panel',
  templateUrl: './ai-chat-panel.component.html',
  styleUrls: ['./ai-chat-panel.component.scss'],
  standalone: false,
})
export class AiChatPanelComponent {
  @Input({ required: true }) messages: AiUiMessage[] = [];
  @Input() loading = false;
  @Input() inputPlaceholder = '';
  @Input() sendLabel = '';

  @Output() send = new EventEmitter<string>();
  @Output() quickAction = new EventEmitter<string>();

  draft = '';

  onSend(): void {
    const t = this.draft.trim();
    if (!t || this.loading) return;
    this.draft = '';
    this.send.emit(t);
  }

  onQuick(text: string): void {
    this.quickAction.emit(text);
  }
}
