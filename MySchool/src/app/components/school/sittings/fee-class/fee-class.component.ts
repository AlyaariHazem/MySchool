import { Component, inject, OnInit } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { PaginatorState } from 'primeng/paginator';

import { FeeService } from '../../core/services/fee.service';
import { Fees, FeeClasses, Fee, FeeClass } from '../../core/models/Fee.model';
import { ClassService } from '../../core/services/class.service';
import { ClassDTO } from '../../core/models/class.model';
import { FeeClassService } from '../../core/services/fee-class.service';
import { LanguageService } from '../../../../core/services/language.service';
import { PaginatorService } from '../../../../core/services/paginator.service';
import { DialogService } from 'primeng/dynamicdialog';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-fee-class',
  templateUrl: './fee-class.component.html',
  styleUrls: ['./fee-class.component.scss',
    '../../../../shared/styles/button.scss',
    '../../../../shared/styles/style-select.scss',
    '../../../../shared/styles/button.scss'
  ]
})
export class FeeClassComponent implements OnInit {
  // Model properties
  FeeClass: FeeClasses[] = [];
  FeeClassDTO: FeeClass = new FeeClass();
  Fees: Fees[] = [];
  Addfee: Fee = new Fee();
  feeAmount: number | null = null;
  selectedClass: string | null = null;
  selectedFee: Fees | null = null;
  classDTO: ClassDTO[] = [];

  isLoading: boolean = true;
  editMode = false; // Flag for fee editing
  editingFeeId: number | null = null;

  editModeClass = false; // Flag for FeeClass editing
  editingFeeClassId: number | null = null;

  exist = false; // To check if a FeeClass exists

  private ClassService = inject(ClassService);
  private feeClassService = inject(FeeClassService);

  constructor(private feeService: FeeService, private toastr: ToastrService
    ,private dialogService: DialogService
  ) {}
  paginatedClassFee: FeeClasses[] = []; // Paginated data

  languageService=inject(LanguageService);
  paginatorService = inject(PaginatorService);
  handlePageChange(event: PaginatorState): void {
    this.paginatorService.onPageChange(event);
    this.paginatedClassFee = this.paginatorService.pageSlice(this.FeeClass);
  }

  ngOnInit(): void {
    this.getAllFees();
    this.getClasses();
    this.paginatedClassFee =this.paginatorService.pageSlice(this.FeeClass);
    this.getAllClassFees();
    this.languageService.currentLanguage();
  }

  // Fee Operations
  getAllFees(): void {
    this.feeService.getAllFee().subscribe({
      next: (res) =>{
        if(res.isSuccess){
          this.Fees = res.result;
          this.isLoading=false;
        }else{
          this.toastr.error(res.errorMasseges[0]);
          this.isLoading=false;
        }
      } ,
      error: () => {
        this.toastr.error('Error fetching fees');
        this.isLoading=false;
      }
    });
    
  }

  onSubmit(feeForm: NgForm): void {
    if (feeForm.valid) {
      this.editMode ? this.updateFee() : this.addFee();
      feeForm.resetForm();
    }
  }

  addFee(): void {
    this.feeService.AddFee(this.Addfee).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.error(res.errorMasseges[0]);
          return;
        }
        this.toastr.success(res.result);
        this.getAllFees();
        this.resetForm();
      },
      error: () => this.toastr.error('حدث خطأ أثناء إضافة الرسوم'),
    });
  }

  editFee(fee: Fee): void {
    this.editMode = true;
    this.editingFeeId = fee.feeID;
    this.Addfee = { ...fee }; // Clone the fee for editing
  }

  updateFee(): void {
    if (this.editingFeeId) {
      this.feeService.Update(this.editingFeeId, this.Addfee).subscribe({
       next: (res)=>{
        if(res.isSuccess){
          this.toastr.success(res.result);
          this.getAllFees();
          this.editMode = false;
          this.resetForm();
        }
       },
        error: () => this.toastr.error('حدث خطأ أثناء تعديل الرسوم'),
      });
    }
  }

deleteFee(id: number): void {
  const ref = this.dialogService.open(ConfirmDialogComponent, {
    header: 'Delete Fee',
    width: 'auto',
    data: {
      title: 'Delete Fee',
      message: 'هل أنت متأكد من أنك تريد حذف هذه رسوم؟',
      deleteFn: () => this.feeService.DeleteFee(id),
      successMessage: 'Fee deleted successfully'
    }
  });

  ref.onClose.subscribe((confirmed: boolean) => {
    if (confirmed) {
      this.Fees = this.Fees.filter(s => s.feeID !== id);
    }
  });
}

  resetForm(): void {
    this.Addfee = new Fee();
    this.editingFeeId = null;
    this.editMode = false;
  }

  // FeeClass Operations
  onSubmitClassFee(classFeeForm: NgForm): void {
    if (classFeeForm.valid) {
      this.editModeClass ? this.updateFeeClass() : this.addFeeClass();
      classFeeForm.resetForm();
    }
  }

  addFeeClass(): void {
    
    this.feeClassService.AddFeeClass(this.FeeClassDTO).subscribe({
      next: (res) => {
        if(res.isSuccess){
          this.toastr.success(res.result);
          this.getAllClassFees();
          this.resetClassFeeForm();
        }
      },
      error: () => this.toastr.error('حدث خطأ أثناء إضافة رسوم الصفوف'),
    });
  }

  editFeeClass(feeClass: FeeClasses): void {
    this.editModeClass = true;
    this.editingFeeClassId = feeClass.feeClassID;
    this.FeeClassDTO = { ...feeClass }; // Clone the FeeClass for editing
    this.exist = false;
  }

  updateFeeClass(): void {
    if (this.editingFeeClassId) {
      this.feeClassService.UpdateFeeClass(this.editingFeeClassId, this.FeeClassDTO).subscribe({
        next: () => {
          this.toastr.success('تم تعديل رسوم الصفوف بنجاح');
          this.getAllClassFees();
          this.resetClassFeeForm();
        },
        error: () => this.toastr.error('حدث خطأ أثناء تعديل رسوم الصفوف'),
      });
    }
  }

  deleteFeeClass(id: number): void {
    const ref = this.dialogService.open(ConfirmDialogComponent, {
      header: 'Delete Fee Class',
      width: 'auto',
      data: {
        title: 'Delete Fee Class',
        message: 'هل أنت متأكد من أنك تريد حذف هذه رسوم الصف؟',
        deleteFn: () => this.feeClassService.DeleteFeeClass(id),
        successMessage: 'Fee Class deleted successfully'
      }
    });

    ref.onClose.subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.paginatedClassFee = this.paginatedClassFee.filter(s => s.feeClassID !== id);
      }
    });
  }
  getAllClassFees(): void {
    this.feeClassService.getAllFeeClass().subscribe({
      next: (res) =>{
        (this.FeeClass = res);
        this.paginatedClassFee =this.paginatorService.pageSlice(this.FeeClass);
      },
      error: () => this.toastr.error('Error fetching class fees'),
    });
    
  }

  resetClassFeeForm(): void {
    this.FeeClassDTO = new FeeClass();
    this.editingFeeClassId = null;
    this.editModeClass = false;
    this.exist = false;
  }

  getClasses(): void {
    this.ClassService.GetAll().subscribe({
      next:res => {
        if (res.isSuccess) {
          this.classDTO = res.result;
        } else {
          this.toastr.error(res.errorMasseges[0]);
        }
      }
    });
  }

  getFeeClassByID(classId: number, feeId: number): boolean {
    this.exist = this.FeeClass.some((fc) => fc.classID === classId && fc.feeID === feeId);
    return this.exist;
  }

  changeFeeClass(feeClass: FeeClasses): void {
    const patchDoc = [
      { op: "replace", path: "/mandatory", value: !feeClass.mandatory }
    ];
    console.log('patchDoc', patchDoc);
    this.feeClassService.partialUpdate(feeClass.feeClassID, patchDoc).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.toastr.success(response.result);
          this.getAllClassFees();
        }
      },
      error: () => this.toastr.error('Failed to update FeeClass', 'Error')
    });
  }
  changeFee(fee: Fees): void {
    const patchDoc = [
      { op: "replace", path: "/state", value: !fee.state }
    ];
    console.log('patchDoc', patchDoc);
    this.feeService.partialUpdate(fee.feeID, patchDoc).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.toastr.success(response.result);
          this.getAllFees();
        }
      },
      error: () => this.toastr.error('Failed to update FeeClass', 'Error')
    });
  }
  
}
