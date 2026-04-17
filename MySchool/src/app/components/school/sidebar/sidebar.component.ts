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
  get canViewDashboardNav(): boolean {
    return this.perm.hasPermission(PagePermission.Dashboard.View);
  }
  get canViewTeachersNav(): boolean {
    return this.perm.hasPermission(PagePermission.Teachers.View);
  }
  get canViewStudentsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Students.View);
  }
  get canViewSettingsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Settings.View);
  }
  /** Show التقارير submenu when any report (umbrella or sub-route) is allowed. */
  get canViewReportsSection(): boolean {
    return this.perm.hasAny([
      PagePermission.Reports.View,
      PagePermission.ReportsFinancial.View,
      PagePermission.ReportsTerm.View,
      PagePermission.ReportsMonthly.View,
      PagePermission.ReportsRegistration.View,
      PagePermission.ReportsAllotment.View,
    ]);
  }
  get canViewReportsFinancialNav(): boolean {
    return this.perm.hasPermission(PagePermission.ReportsFinancial.View);
  }
  get canViewReportsTermNav(): boolean {
    return this.perm.hasPermission(PagePermission.ReportsTerm.View);
  }
  get canViewReportsMonthlyNav(): boolean {
    return this.perm.hasPermission(PagePermission.ReportsMonthly.View);
  }
  get canViewReportsRegistrationNav(): boolean {
    return this.perm.hasPermission(PagePermission.ReportsRegistration.View);
  }
  get canViewGuardiansNav(): boolean {
    return this.perm.hasPermission(PagePermission.Guardians.View);
  }
  get canViewAccountsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Accounts.View);
  }
  get canViewGradesNav(): boolean {
    return this.perm.hasPermission(PagePermission.Grades.View);
  }
  get canViewCalendarNav(): boolean {
    return this.perm.hasPermission(PagePermission.Calendar.View);
  }
  get canViewScheduleNav(): boolean {
    return this.perm.hasPermission(PagePermission.Schedule.View);
  }
  get canViewExamsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Exams.View);
  }
  get canViewHomeworkNav(): boolean {
    return this.perm.hasPermission(PagePermission.Homework.View);
  }
  get canViewAttendanceNav(): boolean {
    return this.perm.hasPermission(PagePermission.Attendance.View);
  }
  get canViewNotificationsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Notifications.View);
  }
  get canViewTestsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Tests.View);
  }
  get canViewHolidaysNav(): boolean {
    return this.perm.hasPermission(PagePermission.Holidays.View);
  }
  get canViewEventsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Events.View);
  }
  get canViewFeesNav(): boolean {
    return this.perm.hasPermission(PagePermission.Fees.View);
  }
  /** المقررات والخطط — books / curricula / plans. */
  get canViewCoursesOrPlansNav(): boolean {
    return (
      this.perm.hasPermission(PagePermission.Courses.View) ||
      this.perm.hasPermission(PagePermission.Plans.View)
    );
  }
  get canViewPayrollNav(): boolean {
    return this.perm.hasPermission(PagePermission.Payroll.View);
  }
  get canViewBlogsNav(): boolean {
    return this.perm.hasPermission(PagePermission.Blogs.View);
  }
  get canViewEmployeesNav(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.View);
  }
  /** Job postings & applications (falls back to HR register permission). */
  get canViewRecruitmentNav(): boolean {
    return this.perm.hasAny([
      PagePermission.Recruitment.View,
      PagePermission.Employees.View,
    ]);
  }
  get canCreateRecruitmentNav(): boolean {
    return this.perm.hasAny([
      PagePermission.Recruitment.Create,
      PagePermission.Employees.Create,
    ]);
  }
  get canCreateEmployeesHrNav(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.Create);
  }
  get canViewActivitiesNav(): boolean {
    return this.perm.hasPermission(PagePermission.Activities.View);
  }
  /** إدارة — staff/HR block: employees (HR) or legacy teachers module. */
  get canViewManagementSection(): boolean {
    return (
      this.perm.hasPermission(PagePermission.Employees.View) ||
      this.perm.hasPermission(PagePermission.Teachers.View)
    );
  }
  /** Staff & HR sidebar block: employee register, recruitment, or teacher directory. */
  get canViewStaffHrSection(): boolean {
    return (
      this.canViewManagementSection ||
      this.perm.hasPermission(PagePermission.Recruitment.View)
    );
  }
}
