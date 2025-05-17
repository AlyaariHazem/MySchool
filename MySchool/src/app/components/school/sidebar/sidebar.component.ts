import { Component, OnInit, Input } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';

import { map } from 'rxjs';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss',
    '../../../../assets/css/sideBar.css'
  ],
  animations: [
    trigger('submenuToggle', [
      state('closed', style({
        height: '0',
        opacity: 0
      })),
      state('open', style({
        height: '*',
        opacity: 1
      })),
      transition('closed <=> open', [
        animate('300ms ease-in-out')
      ])
    ])
  ]
})
export class SidebarComponent implements OnInit {
  
  readonly dir$ = this.store.select(selectLanguage).pipe(
      map(l => (l === 'ar' ? 'rtl' : 'ltr')),
    );
    constructor(private store: Store) { }

  isSubmenuOpen: { [key: string]: boolean } = {
    sittings: false,
    teachersSubmenu: false,
    studentsSubmenu: false,
    guardianSubmenu: false,
    accountSubmenu: false,
    accountsSubmenu: false,
    blogSubmenu: false,
    courses: false,
    GradeSubmenu: false,
    payrollSubmenu: false,
    mangmentSubmenu: false,
    employeesSubmenu: false,
    reportsSubmenu: false,
  };

  @Input() sidebar: boolean = false;

  ngOnInit() {
  }
  SchoolLogo=localStorage.getItem('SchoolImageURL');
  schoolName=localStorage.getItem('schoolName');
  toggleSubmenu(submenu: string, parentSubmenu?: string) {
    if (parentSubmenu) {
      this.closeOtherSubmenus(parentSubmenu, submenu);
    }
    this.isSubmenuOpen[submenu] = !this.isSubmenuOpen[submenu];
  }

  closeOtherSubmenus(parentSubmenu: string, currentSubmenu: string) {
    for (const submenu in this.isSubmenuOpen) {
      if (submenu !== currentSubmenu && submenu.startsWith(parentSubmenu)) {
        this.isSubmenuOpen[submenu] = false;
      }
    }
  }

  getSubmenuState(submenu: string): string {
    return this.isSubmenuOpen[submenu] ? 'open' : 'closed';
  }

  cancel() {
    this.sidebar = false;
  }
}
