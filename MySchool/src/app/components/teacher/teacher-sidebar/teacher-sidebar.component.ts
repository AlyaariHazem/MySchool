import { Component, EventEmitter, Input, Output } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { map } from 'rxjs';
import { Store } from '@ngrx/store';

import { selectLanguage } from '../../../core/store/language/language.selectors';

@Component({
  selector: 'app-teacher-sidebar',
  templateUrl: './teacher-sidebar.component.html',
  styleUrls: [
    './teacher-sidebar.component.scss',
    '../../../../assets/css/sideBar.css',
  ],
  animations: [
    trigger('submenuToggle', [
      state('closed', style({ height: 0, opacity: 0 })),
      state('open', style({ height: '*', opacity: 1 })),
      transition('closed <=> open', animate('300ms ease-in-out')),
    ]),
  ],
})
export class TeacherSidebarComponent {
  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  @Input() open = false;
  @Output() closed = new EventEmitter<void>();

  SchoolLogo = localStorage.getItem('SchoolImageURL');
  schoolName = localStorage.getItem('schoolName');

  isSubmenuOpen: Record<string, boolean> = {};

  constructor(private store: Store) {}

  cancel(): void {
    this.closed.emit();
  }

  toggleSubmenu(key: string): void {
    this.isSubmenuOpen[key] = !this.isSubmenuOpen[key];
  }

  getSubmenuState(key: string): string {
    return this.isSubmenuOpen[key] ? 'open' : 'closed';
  }
}
