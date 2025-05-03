import { Component } from '@angular/core';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss']
})
export class ConfirmDialogComponent {

  constructor(
    private ref: DynamicDialogRef,
    public config: DynamicDialogConfig,
    private toast: ToastrService
  ) { }

  accept(): void {
    const { deleteFn, successMessage } = this.config.data;
    deleteFn().subscribe({
      next: () => {
        this.toast.success(successMessage || 'Deleted successfully');
        this.ref.close(true);
      },
      error: () => this.toast.error('Delete failed')
    });
  }

  reject(): void {
    this.ref.close(false);
  }
}
