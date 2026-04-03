import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';
import { PaginatorState } from 'primeng/paginator';
import { map } from 'rxjs';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import {
  NotificationChannelFlags,
  NotificationInboxDto,
  NotificationTargetKind,
  SendNotificationRequest
} from '../core/models/notifications.model';
import { NotificationsService } from '../core/services/notifications.service';
import { ClassService } from '../core/services/class.service';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss']
})
export class NotificationsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private notifications = inject(NotificationsService);
  private classService = inject(ClassService);
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);
  private sanitizer = inject(DomSanitizer);
  private store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map(l => (l === 'ar' ? 'rtl' : 'ltr')));

  userType = '';
  canSend = false;
  activeTab = '0';

  inboxRows: NotificationInboxDto[] = [];
  pageNumber = 1;
  pageSize = 15;
  first = 0;
  totalRecords = 0;
  inboxLoading = false;

  unreadCount = 0;

  detailVisible = false;
  selected: NotificationInboxDto | null = null;

  targetOptions: { label: string; value: NotificationTargetKind }[] = [];

  sendForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(500)]],
    body: [''],
    targetKind: [NotificationTargetKind.ClassStudents, Validators.required],
    classId: [null as number | null],
    directUserIdsText: [''],
    chInApp: [true],
    chEmail: [false],
    chSms: [false],
    chPush: [false]
  });

  sendLoading = false;

  /** From `Classes/GetAllNameClasses` (includes classID + className). */
  classes: { classID: number; className: string }[] = [];

  constructor() {
    this.sendForm.get('targetKind')?.valueChanges.subscribe(() => this.onTargetKindChange());
  }

  ngOnInit(): void {
    this.userType = (typeof localStorage !== 'undefined' && localStorage.getItem('userType')) || '';
    this.canSend = ['ADMIN', 'MANAGER', 'TEACHER'].includes(this.userType);
    this.rebuildTargetOptions();
    this.translate.onLangChange.subscribe(() => this.rebuildTargetOptions());

    this.loadUnreadCount();
    this.loadInbox(1);

    if (this.canSend) {
      this.classService.GetAllNames().subscribe({
        next: res => {
          if (res.result) {
          this.classes = res.result as { classID: number; className: string }[];
        }
        },
        error: () => {}
      });
    }
  }

  get isPrivileged(): boolean {
    return this.userType === 'ADMIN' || this.userType === 'MANAGER';
  }

  needsClass(): boolean {
    const k = this.sendForm.get('targetKind')?.value;
    return (
      k === NotificationTargetKind.ClassStudents ||
      k === NotificationTargetKind.ClassGuardians ||
      k === NotificationTargetKind.ClassTeachers
    );
  }

  needsDirectIds(): boolean {
    return this.sendForm.get('targetKind')?.value === NotificationTargetKind.DirectUserIds;
  }

  private onTargetKindChange(): void {
    if (!this.needsClass()) {
      this.sendForm.patchValue({ classId: null }, { emitEvent: false });
    }
    if (!this.needsDirectIds()) {
      this.sendForm.patchValue({ directUserIdsText: '' }, { emitEvent: false });
    }
  }

  private rebuildTargetOptions(): void {
    const direct = {
      label: this.translate.instant('notifications.target.direct'),
      value: NotificationTargetKind.DirectUserIds
    };
    const classStudents = {
      label: this.translate.instant('notifications.target.classStudents'),
      value: NotificationTargetKind.ClassStudents
    };
    const classGuardians = {
      label: this.translate.instant('notifications.target.classGuardians'),
      value: NotificationTargetKind.ClassGuardians
    };
    const classTeachers = {
      label: this.translate.instant('notifications.target.classTeachers'),
      value: NotificationTargetKind.ClassTeachers
    };
    const tenant = this.isPrivileged
      ? [
          {
            label: this.translate.instant('notifications.target.allGuardians'),
            value: NotificationTargetKind.AllGuardiansInTenant
          },
          {
            label: this.translate.instant('notifications.target.allStudents'),
            value: NotificationTargetKind.AllStudentsInTenant
          },
          {
            label: this.translate.instant('notifications.target.allTeachers'),
            value: NotificationTargetKind.AllTeachersInTenant
          },
          {
            label: this.translate.instant('notifications.target.allUsers'),
            value: NotificationTargetKind.AllUsersInTenant
          }
        ]
      : [];

    this.targetOptions = [direct, classStudents, classGuardians, classTeachers, ...tenant];
  }

  loadUnreadCount(): void {
    this.notifications.getUnreadCount().subscribe({
      next: res => {
        const c = res.result?.count;
        this.unreadCount = typeof c === 'number' ? c : 0;
      },
      error: () => {}
    });
  }

  loadInbox(page: number): void {
    this.inboxLoading = true;
    this.pageNumber = page;
    this.notifications.getInbox(page, this.pageSize).subscribe({
      next: res => {
        this.inboxLoading = false;
        const paged = res.result;
        if (paged?.data) {
          this.inboxRows = paged.data;
          this.totalRecords = paged.totalCount ?? 0;
          this.first = ((paged.pageNumber ?? page) - 1) * (paged.pageSize ?? this.pageSize);
        } else {
          this.inboxRows = [];
          this.totalRecords = 0;
        }
      },
      error: () => {
        this.inboxLoading = false;
        this.toastr.error(this.translate.instant('notifications.toast.loadInboxError'));
      }
    });
  }

  onInboxPageChange(e: PaginatorState): void {
    const rows = e.rows ?? this.pageSize;
    const first = e.first ?? 0;
    const page = Math.floor(first / rows) + 1;
    this.pageSize = rows;
    this.loadInbox(page);
  }

  openDetail(row: NotificationInboxDto): void {
    this.selected = row;
    this.detailVisible = true;
  }

  closeDetail(): void {
    this.detailVisible = false;
    this.selected = null;
  }

  safeBody(html: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(html || '');
  }

  markSelectedRead(): void {
    if (!this.selected || this.selected.isRead) {
      this.closeDetail();
      return;
    }
    this.notifications.markRead(this.selected.deliveryId).subscribe({
      next: res => {
        if (res.isSuccess === false) {
          const msg = res.errorMasseges?.join(' ') || this.translate.instant('notifications.toast.markReadError');
          this.toastr.error(msg);
          return;
        }
        this.selected!.isRead = true;
        const row = this.inboxRows.find(r => r.deliveryId === this.selected!.deliveryId);
        if (row) row.isRead = true;
        this.loadUnreadCount();
        this.notifications.notifyInboxChanged();
        this.toastr.success(this.translate.instant('notifications.toast.markReadOk'));
      },
      error: () => this.toastr.error(this.translate.instant('notifications.toast.markReadError'))
    });
  }

  private parseUserIds(text: string): string[] {
    return text
      .split(/[\s,;]+/)
      .map(s => s.trim())
      .filter(Boolean);
  }

  private buildChannels(): NotificationChannelFlags {
    const v = this.sendForm.getRawValue();
    let f = NotificationChannelFlags.None;
    if (v.chInApp) f |= NotificationChannelFlags.InApp;
    if (v.chEmail) f |= NotificationChannelFlags.Email;
    if (v.chSms) f |= NotificationChannelFlags.Sms;
    if (v.chPush) f |= NotificationChannelFlags.Push;
    if (f === NotificationChannelFlags.None) f = NotificationChannelFlags.InApp;
    return f;
  }

  submitSend(): void {
    if (this.sendForm.invalid) {
      this.sendForm.markAllAsTouched();
      this.toastr.warning(this.translate.instant('notifications.toast.fixForm'));
      return;
    }

    const v = this.sendForm.getRawValue();
    const targetKind = v.targetKind as NotificationTargetKind;

    if (this.needsClass() && v.classId == null) {
      this.toastr.warning(this.translate.instant('notifications.validation.classRequired'));
      return;
    }
    if (this.needsDirectIds()) {
      const ids = this.parseUserIds(v.directUserIdsText || '');
      if (ids.length === 0) {
        this.toastr.warning(this.translate.instant('notifications.validation.directIdsRequired'));
        return;
      }
    }

    const body: SendNotificationRequest = {
      title: (v.title || '').trim(),
      body: v.body || '',
      targetKind,
      channels: this.buildChannels()
    };

    if (this.needsClass()) body.classId = Number(v.classId);
    if (this.needsDirectIds()) body.directUserIds = this.parseUserIds(v.directUserIdsText || '');

    this.sendLoading = true;
    this.notifications.send(body).subscribe({
      next: res => {
        this.sendLoading = false;
        if (res.isSuccess === false) {
          const msg = res.errorMasseges?.join(' ') || this.translate.instant('notifications.toast.sendError');
          this.toastr.error(msg);
          return;
        }
        const n = res.result?.recipientCount ?? 0;
        this.toastr.success(this.translate.instant('notifications.toast.sendOk', { count: n }));
        this.sendForm.reset({
          title: '',
          body: '',
          targetKind: NotificationTargetKind.ClassStudents,
          classId: null,
          directUserIdsText: '',
          chInApp: true,
          chEmail: false,
          chSms: false,
          chPush: false
        });
        this.activeTab = '0';
        this.loadInbox(1);
        this.loadUnreadCount();
      },
      error: () => {
        this.sendLoading = false;
        this.toastr.error(this.translate.instant('notifications.toast.sendError'));
      }
    });
  }
}
