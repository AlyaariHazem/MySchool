import { Component } from '@angular/core';
import { map } from 'rxjs';
import { Store } from '@ngrx/store';

import { selectLanguage } from '../../../core/store/language/language.selectors';

/** Shell for all `/students/*` routes (same structure as {@link TeacherLayoutComponent}). */
@Component({
  selector: 'app-student-layout',
  templateUrl: './student-layout.component.html',
  styleUrl: './student-layout.component.scss',
})
export class StudentLayoutComponent {
  constructor(private store: Store) {}

  readonly dir$ = this.store
    .select(selectLanguage)
    .pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));
}
