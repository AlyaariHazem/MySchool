import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';

import { selectLanguage } from '../../../core/store/language/language.selectors';

@Component({
  selector: 'app-guardian-home',
  templateUrl: './guardian-home.component.html',
  styleUrl: './guardian-home.component.scss',
})
export class GuardianHomeComponent implements OnInit {
  private router = inject(Router);
  private store = inject(Store);

  readonly dir$ = this.store
    .select(selectLanguage)
    .pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  displayName = '';

  ngOnInit(): void {
    if (typeof window !== 'undefined' && localStorage.getItem('userType') !== 'GUARDIAN') {
      void this.router.navigateByUrl('/school/dashboard', { replaceUrl: true });
      return;
    }

    this.displayName =
      (typeof localStorage !== 'undefined' ? localStorage.getItem('managerName') : null) ||
      (typeof localStorage !== 'undefined' ? localStorage.getItem('userName') : null) ||
      '';
  }

  avatarLetter(name: string): string {
    const t = (name || '').trim();
    if (!t) {
      return '?';
    }
    return t.charAt(0).toUpperCase();
  }
}
