import { Component } from '@angular/core';
import { map } from 'rxjs';
import { Store } from '@ngrx/store';

import { selectLanguage } from '../../../core/store/language/language.selectors';

/** Shell for all `/guardian/*` routes (same structure as {@link StudentLayoutComponent}). */
@Component({
  selector: 'app-guardian-layout',
  templateUrl: './guardian-layout.component.html',
  styleUrl: './guardian-layout.component.scss',
})
export class GuardianLayoutComponent {
  constructor(private store: Store) {}

  readonly dir$ = this.store
    .select(selectLanguage)
    .pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));
}
