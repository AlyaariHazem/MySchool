import { Component, OnInit, inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';

import { AiAssistantService } from './ai-assistant.service';
import type { AiUiMessage } from './ai-assistant.models';

/**
 * Floating FAB + slide-up panel (not sidebar) — hosts the same chat flow as the former full page.
 */
@Component({
  selector: 'app-ai-assistant-floating',
  templateUrl: './ai-assistant-floating.component.html',
  styleUrls: ['./ai-assistant-floating.component.scss'],
  standalone: false,
})
export class AiAssistantFloatingComponent implements OnInit {
  private readonly ai = inject(AiAssistantService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);

  panelOpen = false;

  /** minimal-agent conversation id (server-side memory). */
  supportConversationId: string | null = null;

  uiMessages: AiUiMessage[] = [];
  loading = false;

  readonly quickPrompts: { key: string }[] = [
    { key: 'aiAssistant.examples.scheduleToday' },
    { key: 'aiAssistant.examples.assignmentsThisWeek' },
    { key: 'aiAssistant.examples.nextMathTest' },
    { key: 'aiAssistant.examples.currentSemesterGrades' },
    { key: 'aiAssistant.examples.absenceDaysThisMonth' },
    { key: 'aiAssistant.examples.parentAbsenceMessage' },
    { key: 'aiAssistant.examples.studentsHighAbsence' },
    { key: 'aiAssistant.examples.grade10PerformanceReport' },
  ];

  inputPlaceholder = '';
  sendLabel = '';
  title = '';

  ngOnInit(): void {
    this.translate
      .get([
        'aiAssistant.inputPlaceholder',
        'aiAssistant.send',
        'aiAssistant.title',
      ])
      .subscribe((t) => {
        this.inputPlaceholder = t['aiAssistant.inputPlaceholder'];
        this.sendLabel = t['aiAssistant.send'];
        this.title = t['aiAssistant.title'];
      });
  }

  togglePanel(): void {
    this.panelOpen = !this.panelOpen;
  }

  closePanel(): void {
    this.panelOpen = false;
  }

  onSend(text: string): void {
    const trimmed = text.trim();
    if (!trimmed || this.loading) return;

    this.uiMessages = [...this.uiMessages, { role: 'user', content: trimmed }];
    this.loading = true;

    this.ai.chat(trimmed, this.supportConversationId).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.conversationId) this.supportConversationId = res.conversationId;
        if (res.error && !res.reply) {
          this.toastr.error(res.error, this.translate.instant('aiAssistant.errorTitle'));
          this.uiMessages = [
            ...this.uiMessages,
            { role: 'assistant', content: '', error: res.error, toolSteps: res.toolSteps },
          ];
          return;
        }

        const reply = res.reply?.trim() ?? '';
        this.uiMessages = [
          ...this.uiMessages,
          {
            role: 'assistant',
            content: reply || this.translate.instant('aiAssistant.emptyReply'),
            toolSteps: res.toolSteps?.length ? res.toolSteps : undefined,
            error: res.error ?? undefined,
          },
        ];
      },
      error: (err) => {
        this.loading = false;
        const msg =
          err?.error?.error ??
          err?.error?.message ??
          err?.message ??
          this.translate.instant('aiAssistant.networkError');
        this.toastr.error(msg, this.translate.instant('aiAssistant.errorTitle'));
        this.uiMessages = [
          ...this.uiMessages,
          { role: 'assistant', content: '', error: String(msg) },
        ];
      },
    });
  }

  onQuick(key: string): void {
    this.translate.get(key).subscribe((text) => this.onSend(text));
  }
}
