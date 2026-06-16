import { AsyncPipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { TranslateModule } from '@ngx-translate/core';
import { filter, map, startWith } from 'rxjs';

import { selectLanguage } from 'app/core/store/language/language.selectors';

function pathOnly(url: string): string {
  return url.split('?')[0];
}

@Component({
  selector: 'app-public-recruitment-topbar',
  standalone: true,
  imports: [AsyncPipe, TranslateModule, RouterLink],
  templateUrl: './public-recruitment-topbar.component.html',
  styleUrl: './public-recruitment-topbar.component.scss',
})
export class PublicRecruitmentTopbarComponent {
  private readonly store = inject(Store);
  private readonly router = inject(Router);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  /** Avoid RouterLinkActive false positives (e.g. Jobs highlighted on job-applications/create). */
  private readonly currentPath = toSignal(
    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd),
      map(() => pathOnly(this.router.url)),
      startWith(pathOnly(this.router.url)),
    ),
    { initialValue: pathOnly(this.router.url) },
  );

  readonly isAboutActive = computed(() => this.currentPath() === '/school/recruitment/about');

  readonly isLoginActive = computed(() => this.currentPath() === '/login');

  /** List + posting detail only — not apply flow or other recruitment URLs. */
  readonly isJobsActive = computed(() => {
    const p = this.currentPath();
    return p === '/school/recruitment/job-postings' || /^\/school\/recruitment\/job-postings\/\d+$/.test(p);
  });
}
