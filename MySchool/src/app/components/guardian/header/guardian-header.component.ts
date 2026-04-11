import { Component } from '@angular/core';
import { map } from 'rxjs';
import { Store } from '@ngrx/store';

import { selectLanguage } from '../../../core/store/language/language.selectors';

/**
 * Guardian-area wrapper around the shared school header (same chrome as student/teacher).
 */
@Component({
  selector: 'app-guardian-header',
  templateUrl: './guardian-header.component.html',
  styleUrl: './guardian-header.component.scss',
})
export class GuardianHeaderComponent {
  constructor(private store: Store) {}

  readonly dir$ = this.store
    .select(selectLanguage)
    .pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));
}
