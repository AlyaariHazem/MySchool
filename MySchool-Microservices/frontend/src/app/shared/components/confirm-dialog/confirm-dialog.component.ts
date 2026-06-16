import { Component } from '@angular/core';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss']
})
export class ConfirmDialogComponent {

  isDeleting = false;

  constructor(
    private ref: DynamicDialogRef,
    public config: DynamicDialogConfig,
    private toast: ToastrService
  ) { }

  accept(): void {
    if (this.isDeleting) {
      return;
    }
    const { deleteFn, successMessage } = this.config.data;
    this.isDeleting = true;
    deleteFn().pipe(
      finalize(() => {
        this.isDeleting = false;
      }),
    ).subscribe({
      next: () => {
        this.toast.success(successMessage || 'Deleted successfully');
        this.ref.close(true);
      },
      error: () => this.toast.error('Delete failed')
    });
  }

  reject(): void {
    if (this.isDeleting) {
      return;
    }
    this.ref.close(false);
  }
}
