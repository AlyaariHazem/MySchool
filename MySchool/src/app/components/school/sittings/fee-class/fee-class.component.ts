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

  editMode = false; // Flag for fee editing
  editingFeeId: number | null = null;

  editModeClass = false; // Flag for FeeClass editing
  editingFeeClassId: number | null = null;

  exist = false; // To check if a FeeClass exists

  private ClassService = inject(ClassService);
  private feeClassService = inject(FeeClassService);

  constructor(private feeService: FeeService, private toastr: ToastrService) {}
  paginatedClassFee: FeeClasses[] = []; // Paginated data

  languageService=inject(LanguageService);

  first: number = 0; 
  rows: number = 4; 
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedClassFee = this.FeeClass.slice(start, end);
  }

  // Handle page change event from PrimeNG paginator
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows || 4; // Default to 4 rows
    this.updatePaginatedData();
  }

  ngOnInit(): void {
    this.getAllFees();
    this.getClasses();
    this.updatePaginatedData();
    this.getAllClassFees();
    this.languageService.currentLanguage();
  }

  // Fee Operations
  getAllFees(): void {
    this.feeService.getAllFee().subscribe({
      next: (res) =>{
        (this.Fees = res),
        console.log(res);
      } ,
      error: (err) => this.toastr.error('Error fetching fees'),
    });
    
  }

  onSubmit(feeForm: NgForm): void {
    if (feeForm.valid) {
      this.editMode ? this.updateFee() : this.addFee();
    }
  }

  addFee(): void {
    this.feeService.AddFee(this.Addfee).subscribe({
      next: () => {
        this.toastr.success('تم إضافة الرسوم بنجاح');
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
        next: () => {
          this.toastr.success('تم تعديل الرسوم بنجاح');
          this.getAllFees();
          this.editMode = false;
          this.resetForm();
        },
        error: () => this.toastr.error('حدث خطأ أثناء تعديل الرسوم'),
      });
    }
  }

  deleteFee(id: number): void {
    this.feeService.DeleteFee(id).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.toastr.success(res.result, 'Fee Deleted');
          this.getAllFees();
        }
      },
      error: () => this.toastr.error('Failed to delete Fee', 'Error'),
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
    }
  }

  addFeeClass(): void {
    this.feeClassService.AddFeeClass(this.FeeClassDTO).subscribe({
      next: () => {
        this.toastr.success('تم إضافة رسوم الصفوف بنجاح');
        this.getAllClassFees();
        this.resetClassFeeForm();
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

  deleteFeeClass(feeClassID: number): void {
    this.feeClassService.DeleteFeeClass(feeClassID).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastr.success(res);
          this.getAllClassFees();
        } else {
          this.toastr.error(res);
        }
      },
    });
  }

  getAllClassFees(): void {
    this.feeClassService.getAllFeeClass().subscribe({
      next: (res) =>{
        (this.FeeClass = res);
         this.updatePaginatedData();
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
      next: (res) => (this.classDTO = res),
      error: (err) => this.toastr.error('Error fetching classes'),
    });
  }

  getFeeClassByID(classId: number, feeId: number): boolean {
    this.exist = this.FeeClass.some((fc) => fc.classID === classId && fc.feeID === feeId);
    return this.exist;
  }
}
