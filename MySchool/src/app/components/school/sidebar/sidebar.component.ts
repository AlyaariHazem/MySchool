/* sidebar.component.ts */
import {
  Component, EventEmitter, Input, Output
} from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { map } from 'rxjs';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: [
    './sidebar.component.scss',
    '../../../../assets/css/sideBar.css'
  ],
  animations: [
    trigger('submenuToggle', [
      state('closed', style({ height: 0, opacity: 0 })),
      state('open',   style({ height: '*', opacity: 1 })),
      transition('closed <=> open', animate('300ms ease-in-out')),
    ]),
  ],
})
export class SidebarComponent {
  /* rtl / ltr as an observable for the template */
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );
  constructor(private store: Store) {}

  /* ---------- open / close from the header ---------- */
  @Input()  open  = false;
  @Output() closed = new EventEmitter<void>();

  cancel() {                 // called by the X button
    this.closed.emit();
  }

  /* ---------- submenu logic (unchanged) ------------- */
  isSubmenuOpen: Record<string, boolean> = { /* … your keys … */ };

  toggleSubmenu(key: string, parent?: string) {
    if (parent) this.closeOtherSubmenus(parent, key);
    this.isSubmenuOpen[key] = !this.isSubmenuOpen[key];
  }
  closeOtherSubmenus(parent: string, current: string) {
    for (const k in this.isSubmenuOpen)
      if (k !== current && k.startsWith(parent)) this.isSubmenuOpen[k] = false;
  }
  getSubmenuState(key: string) { return this.isSubmenuOpen[key] ? 'open' : 'closed'; }

  /* ---------- misc fields (logo, school name) ------- */
  SchoolLogo  = localStorage.getItem('SchoolImageURL');
  schoolName  = localStorage.getItem('schoolName');
}
