import { Component, OnInit, Input, inject } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { TranslationService } from '../../../core/services/translation.service';
import { LanguageService } from '../../../core/services/language.service';

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
  translationService=inject(TranslationService);
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
languageService=inject(LanguageService);

  @Input() sidebar: boolean = false;

  ngOnInit() {
    this.languageService.currentLanguage();
    this.translationService.changeLanguage(this.languageService.langDir);
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
