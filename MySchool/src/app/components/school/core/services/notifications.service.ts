import { inject, Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { ApiResponse } from '../../../../core/models/response.model';
import {
  NotificationInboxDto,
  NotificationSendResultDto,
  PagedResultDto,
  SendNotificationRequest
} from '../models/notifications.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {
  private readonly API = inject(BackendAspService);

  /** Emits when inbox/unread state may have changed (header listens). */
  private readonly inboxChanged = new Subject<void>();
  readonly inboxChanged$ = this.inboxChanged.asObservable();

  notifyInboxChanged(): void {
    this.inboxChanged.next();
  }

  send(body: SendNotificationRequest): Observable<ApiResponse<NotificationSendResultDto>> {
    return this.API.postRequest<NotificationSendResultDto>('Notifications/send', body);
  }

  getInbox(pageNumber: number, pageSize: number): Observable<ApiResponse<PagedResultDto<NotificationInboxDto>>> {
    const q = `Notifications/inbox?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    return this.API.getRequest<PagedResultDto<NotificationInboxDto>>(q);
  }

  getUnreadCount(): Observable<ApiResponse<{ count: number }>> {
    return this.API.getRequest<{ count: number }>('Notifications/unread-count');
  }

  markRead(deliveryId: string): Observable<ApiResponse<unknown>> {
    return this.API.postRequest<unknown>('Notifications/mark-read', { deliveryId });
  }
}
