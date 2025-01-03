import { Component, OnInit, Input, inject } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
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
  langDir!:string;
  languageStore=inject(Store);
  dir:string="ltr";
  currentLanguage():void{
    this.languageStore.select("language").subscribe((res)=>{
      this.langDir=res;
      console.log("the language is",this.langDir);
      this.dir=(res=="en")?"ltr":"rtl";
    });
  }

  @Input() sidebar: boolean = false;

  ngOnInit() {}

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
