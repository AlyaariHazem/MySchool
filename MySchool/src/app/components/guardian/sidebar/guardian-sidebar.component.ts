import { Component, EventEmitter, Input, Output } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { map } from 'rxjs';
import { Store } from '@ngrx/store';

import { selectLanguage } from '../../../core/store/language/language.selectors';

/**
 * Guardian-only navigation; same look-and-feel as the student sidebar (sideBar.css).
 */
@Component({
  selector: 'app-guardian-sidebar',
  templateUrl: './guardian-sidebar.component.html',
  styleUrls: ['./guardian-sidebar.component.scss', '../../../../assets/css/sideBar.css'],
  animations: [
    trigger('submenuToggle', [
      state('closed', style({ height: 0, opacity: 0 })),
      state('open', style({ height: '*', opacity: 1 })),
      transition('closed <=> open', animate('300ms ease-in-out')),
    ]),
  ],
})
export class GuardianSidebarComponent {
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map((l) => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  constructor(private store: Store) {}

  @Input() open = false;
  @Output() closed = new EventEmitter<void>();

  cancel(): void {
    this.closed.emit();
  }

  isSubmenuOpen: Record<string, boolean> = {};

  toggleSubmenu(key: string): void {
    this.isSubmenuOpen[key] = !this.isSubmenuOpen[key];
  }

  getSubmenuState(key: string): string {
    return this.isSubmenuOpen[key] ? 'open' : 'closed';
  }

  SchoolLogo = typeof localStorage !== 'undefined' ? localStorage.getItem('SchoolImageURL') : null;
  schoolName = typeof localStorage !== 'undefined' ? localStorage.getItem('schoolName') : '';

  homePath(): string {
    return '/guardian/home';
  }
}
