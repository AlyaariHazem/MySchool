import { Component, inject } from '@angular/core';
import { PaginatorState } from 'primeng/paginator';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { AddManagerComponent } from './add-manager/add-manager.component';
import { managerInfo } from '../core/models/managerInfo.model';
import { ManagerService } from '../../../core/services/manager.service';
import { map } from 'rxjs';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent {
  constructor(
    private toastr: ToastrService,
    private store: Store,
    public dialog: MatDialog) {

  }
  managerInfo: managerInfo[] = [];
  managerService = inject(ManagerService);
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  ngOnInit(): void {
    this.getAllManagers();
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
        this.getAllManagers(); // Refresh the list
      }
    });
  }

  editUser(manager: managerInfo): void {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '95%';
    dialogConfig.panelClass = 'custom-dialog-container';
    dialogConfig.data = { manager: manager, isEditMode: true };

    const dialogRef = this.dialog.open(AddManagerComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم تحديث الطالب بنجاح');
        this.getAllManagers(); // Refresh the list
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
