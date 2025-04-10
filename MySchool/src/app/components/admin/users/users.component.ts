import { Component, inject } from '@angular/core';
import { PaginatorState } from 'primeng/paginator';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { LanguageService } from '../../../core/services/language.service';
import { TranslationService } from '../../../core/services/translation.service';
import { AddManagerComponent } from './add-manager/add-manager.component';
import { managerInfo } from '../core/models/managerInfo.model';
import { ManagerService } from '../../../core/services/manager.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent {
  constructor(
    private toastr: ToastrService,
    public dialog: MatDialog) {

  }
  managerInfo: managerInfo[] = [];
  managerService = inject(ManagerService);
  languageService = inject(LanguageService);
  translationService = inject(TranslationService);

  ngOnInit(): void {
    this.languageService.currentLanguage();
    this.getAllManagers();
    this.translationService.changeLanguage(this.languageService.langDir);
  }
  getAllManagers(): void {
    this.managerService.getAllManagers().subscribe(res => this.managerInfo = res);
  }

  first: number = 0;
  rows: number = 4;
  onPageChange(event: PaginatorState) {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows!;
  }

  openDialog(): void {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '95%';
    dialogConfig.panelClass = 'custom-dialog-container';

    const dialogRef = this.dialog.open(AddManagerComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم إضافة الطالب بنجاح');
      }
    });
  }
  deleteUser(userID: number) {
    if (confirm('هل أنت متأكد من حذف هذا المستخدم؟')) {
      this.managerService.deleteManager(userID).subscribe({
        next: () => {
          this.managerInfo = this.managerInfo.filter(s => s.managerID !== userID);
        },
        error: (err) => {
          console.error('Error deleting student:', err);
        }
      });
    }
  }
}
