/* sidebar.component.ts */
import {
  Component, EventEmitter, inject, Input, Output
} from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { map } from 'rxjs';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { Store } from '@ngrx/store';

import { PagePermission, PermissionService } from '../../../core/services/permission.service';

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
  private readonly perm = inject(PermissionService);

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

  get isTeacher(): boolean {
    if (typeof window === 'undefined') {
      return false;
    }
    return localStorage.getItem('userType') === 'TEACHER';
  }

  /** School managers / platform admin in school shell — can review public registration requests. */
  get showPendingRegistrations(): boolean {
    if (typeof window === 'undefined') {
      return false;
    }
    const t = localStorage.getItem('userType');
    return t === 'MANAGER' || t === 'ADMIN';
  }

  homePath(): string {
    return this.isTeacher ? '/teacher/workspace' : '/school/dashboard';
  }

  /** Page-level flags for sidebar (JWT + login <c>permissions</c>). */
  get canViewTeachersNav(): boolean {
    return this.perm.hasPermission(PagePermission.Teachers.View);
  }
  get canViewStudentsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Students.View);
  }
  get canViewSettingsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Settings.View);
  }
  get canViewReportsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Reports.View);
  }
}
