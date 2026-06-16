import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { filter, map, Subject, takeUntil } from 'rxjs';

import { isPublicRecruitmentRouteUrl } from '../../../core/utils/public-recruitment-route.util';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { PagePermission, PermissionService } from '../../../core/services/permission.service';

@Component({
  selector: 'app-navigate',
  templateUrl: './navigate.component.html',
  styleUrl: './navigate.component.scss'
})
export class NavigateComponent implements OnInit, OnDestroy {
  private readonly permissions = inject(PermissionService);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();

  constructor(private store: Store){}
  
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  /** Header (includes sidebar toggle) + breadcrumb; hidden for anonymous public job board. */
  showSchoolChrome = true;

  get canUseAiChat(): boolean {
    return this.permissions.hasPermission(PagePermission.AiChat.View);
  }

  ngOnInit(): void {
    this.refreshSchoolChrome();
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntil(this.destroy$),
      )
      .subscribe(() => this.refreshSchoolChrome());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private refreshSchoolChrome(): void {
    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('token') : null;
    if (token) {
      this.showSchoolChrome = true;
      return;
    }
    this.showSchoolChrome = !isPublicRecruitmentRouteUrl(this.router);
  }
}
