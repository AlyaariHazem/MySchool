/* header.component.ts */
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Store } from '@ngrx/store';
import { TranslateService } from '@ngx-translate/core';
import { filter, interval, Subject, takeUntil, forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthAPIService } from '../../../auth/authAPI.service';
import { languageAction } from '../../../core/store/language/language.action';
import { NotificationsService } from '../core/services/notifications.service';
import { NotificationInboxDto } from '../core/models/notifications.model';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent implements OnInit, OnDestroy {
  /* one signal drives everything */
  isSidebarOpen = signal(false);

  /* ------------ language / user -------------------- */
  dir = 'ltr';
  langName = 'English';
  languageImage = 'Amarica.jpg';
  userName = localStorage.getItem('userName') ?? '';
  currentUserName = localStorage.getItem('managerName') ?? '';

  /** In-app notifications (header dropdown). */
  headerNotifications: NotificationInboxDto[] = [];
  unreadCount = 0;
  notifLoading = false;

  private readonly destroy$ = new Subject<void>();

  /* ------------ injected services ------------------ */
  private store = inject(Store);
  private translate = inject(TranslateService);
  private auth = inject(AuthAPIService);
  private notifications = inject(NotificationsService);
  private router = inject(Router);

  constructor() {
    /* keep language reactive */
    this.store.select('language').subscribe(lang => {
      this.dir = lang === 'en' ? 'ltr' : 'rtl';
      this.langName = lang === 'en' ? 'English' : 'العربية';
      this.languageImage = lang === 'en' ? 'Amarica.jpg' : 'SudiAribia.jpg';
      this.translate.use(lang);
    });
  }

  ngOnInit(): void {
    this.refreshHeaderNotifications();

    interval(120_000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.refreshHeaderNotifications());

    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        if (!this.router.url.includes('/school/notifications')) {
          this.refreshHeaderNotifications();
        }
      });

    this.notifications.inboxChanged$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.refreshHeaderNotifications();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** Call when opening the notifications dropdown (keeps list fresh). */
  onNotificationsBellClick(): void {
    this.refreshHeaderNotifications();
  }

  refreshHeaderNotifications(): void {
    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('token') : null;
    if (!token) {
      this.headerNotifications = [];
      this.unreadCount = 0;
      return;
    }

    this.notifLoading = true;
    forkJoin({
      count: this.notifications.getUnreadCount().pipe(
        catchError(() => of({ isSuccess: true, result: { count: 0 } } as const))
      ),
      page: this.notifications.getInbox(1, 8).pipe(
        catchError(() => of({ isSuccess: true, result: { data: [] } } as const))
      ),
    }).subscribe({
      next: ({ count, page }) => {
        this.notifLoading = false;
        const c = count.result?.count;
        this.unreadCount = typeof c === 'number' ? c : 0;
        const rows = page.result?.data;
        this.headerNotifications = Array.isArray(rows) ? rows : [];
      },
      error: () => {
        this.notifLoading = false;
      },
    });
  }

  previewBody(html: string, maxLen = 90): string {
    const plain = (html || '')
      .replace(/<[^>]*>/g, ' ')
      .replace(/\s+/g, ' ')
      .trim();
    if (plain.length <= maxLen) {
      return plain;
    }
    return plain.slice(0, maxLen) + '…';
  }

  relativeTime(iso: string): string {
    const t = new Date(iso).getTime();
    if (Number.isNaN(t)) {
      return '';
    }
    const sec = Math.max(0, Math.floor((Date.now() - t) / 1000));
    if (sec < 45) {
      return this.translate.instant('notifications.time.justNow');
    }
    if (sec < 3600) {
      const n = Math.max(1, Math.floor(sec / 60));
      return this.translate.instant('notifications.time.minutesAgo', { n });
    }
    if (sec < 86400) {
      const n = Math.max(1, Math.floor(sec / 3600));
      return this.translate.instant('notifications.time.hoursAgo', { n });
    }
    const n = Math.max(1, Math.floor(sec / 86400));
    return this.translate.instant('notifications.time.daysAgo', { n });
  }

  avatarLetter(title: string): string {
    const t = (title || '').trim();
    if (!t) {
      return '?';
    }
    return t.charAt(0).toUpperCase();
  }

  /* ------------ UI actions -------------------------*/
  toggleSidebar() {
    this.isSidebarOpen.update(v => !v);
  }
  onSidebarClosed() {
    this.isSidebarOpen.set(false);
  }
  changeLang(lang: string) {
    this.store.dispatch(languageAction({ lang }));
    localStorage.setItem('lang', lang);
    this.translate.use(lang);
  }
  logout() {
    this.auth.logout().subscribe();
  }
}
