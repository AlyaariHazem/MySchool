import { Component } from '@angular/core';
import { map } from 'rxjs';
import { Store } from '@ngrx/store';

import { selectLanguage } from '../../../core/store/language/language.selectors';

/**
 * Student-area wrapper around the shared school header (same chrome as admin/teacher).
 */
@Component({
  selector: 'app-student-header',
  templateUrl: './student-header.component.html',
  styleUrl: './student-header.component.scss',
})
export class StudentHeaderComponent {
  constructor(private store: Store) {}

  readonly dir$ = this.store
    .select(selectLanguage)
    .pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));
}
