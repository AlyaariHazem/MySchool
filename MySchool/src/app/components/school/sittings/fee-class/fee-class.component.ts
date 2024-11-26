import { Component, inject } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { FeeService } from '../../../../core/services/fee.service';
import { Fees, FeeClasses, Fee, FeeClass } from '../../../../core/models/Fee.model';
import { ClassService } from '../../../../core/services/class.service';
import { ClassDTO } from '../../../../core/models/class.model';
import { FeeClassService } from '../../../../core/services/fee-class.service';

@Component({
  selector: 'app-fee-class',
  templateUrl: './fee-class.component.html',
  styleUrls: ['./fee-class.component.scss']
})
export class FeeClassComponent {

  // Model properties
  FeeClass: FeeClasses[] = [];
  FeeClassDTO: FeeClass = new FeeClass();
  Fees: Fees[] = [];
  Addfee: Fee = new Fee();
  feeAmount: number | null = null;
  selectedClass: string | null = null;
  selectedFee: Fees | null = null;
  classDTO: ClassDTO[] = [];

  ClassService: ClassService = inject(ClassService);
  feeClassService: FeeClassService = inject(FeeClassService);
  constructor(private feeService: FeeService, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.GetAllFees();
    this.GetClasses();
    this.getAllClassFee();
  }

  GetAllFees(): void {
    this.feeService.getAllFee().subscribe({
      next: (res) => {
        this.Fees = res.data;
      },
      error: (err) => {
        console.error('Error fetching fees:', err);
      }
    });
  }
  GetClasses(): void {
    this.ClassService.GetAll().subscribe({
      next: (res) => {
        this.classDTO = res;
        console.log("classes are", res)
      }
    })
  }


  editMode: boolean = false; // Flag to track edit mode
  editingFeeId: number | null = null; // To store the ID of the fee being edited

  editFee(fee: Fee): void {
    this.editMode = true;
    this.editingFeeId = fee.feeID;
    this.Addfee = { ...fee }; // Populate the form with the selected fee's data
  }

  updateFee(): void {
    if (this.editingFeeId) {
      this.feeService.Update(this.Addfee.feeID, this.Addfee).subscribe({
        next: () => {
          this.toastr.success('تم تعديل الرسوم بنجاح');
          this.GetAllFees(); // Refresh the list after editing
          this.editMode = false;
          this.resetForm();
        },
        error: () => this.toastr.error('حدث خطأ أثناء تعديل الرسوم')
      });
    }
  }
  resetForm(): void {
    this.Addfee = new Fee();
    this.editingFeeId = null;
    this.editMode = false;
  }

  onSubmit(feeForm: NgForm): void {
    if (feeForm.valid) {
      if (this.editMode) {
        this.updateFee();
      } else {
        this.feeService.AddFee(this.Addfee).subscribe({
          next: () => {
            this.toastr.success('تم إضافة الرسوم بنجاح');
            this.GetAllFees();
            this.resetForm();
          },
          error: () => this.toastr.error('حدث خطأ أثناء إضافة الرسوم')
        });
      }
    }
  }

  deleteFee(id: number): void {
    this.feeService.DeleteFee(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastr.success(response.message, 'Fee Deleted');
          this.GetAllFees(); // Refresh the list after deletion
        }
      },
      error: () => this.toastr.error('Failed to delete Fee', 'Error')
    });
  }

  //these for ClassFee-------------------------------------------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
  editModeClass: boolean = false; // Edit mode flag for FeeClass
  editingFeeClassId: number | null = null; // To track the FeeClass being edited
  onSubmitClassFee(classFeeForm: NgForm): void {
    if (classFeeForm.valid) {
      if (this.editModeClass) {
        //for edit 
        this.updateFeeClass();
      } else {
        this.feeClassService.AddFeeClass(this.FeeClassDTO).subscribe({
          next: () => {
            this.toastr.success('تم إضافة رسوم الصفوف بنجاح');
            this.getAllClassFee();
            this.resetClassFeeForm();
          },
          error: () => this.toastr.error('حدث خطأ أثناء إضافة رسوم الصفوف')
        });
      }
    }
  }
  editFeeClass(feeClass: FeeClasses): void {
    this.editModeClass = true;
    this.editingFeeClassId = feeClass.classID;
    this.FeeClassDTO = { ...feeClass };
    this.exist = false; // Reset exist flag
  }

  resetClassFeeForm(): void {
    this.FeeClassDTO = new FeeClass();
    this.editingFeeClassId = null;
    this.editModeClass = false;
    this.exist = false; // Reset exist flag
  }

  updateFeeClass(): void {
    if (this.editingFeeClassId) {
      this.feeClassService.UpdateFeeCass(this.editingFeeClassId, this.FeeClassDTO.feeID, this.FeeClassDTO).subscribe({
        next: () => {
          this.toastr.success('تم تعديل رسوم الصفوف بنجاح');
          this.getAllClassFee(); // Refresh the FeeClass list
          this.resetClassFeeForm();
        },
        error: () => this.toastr.error('حدث خطأ أثناء تعديل رسوم الصفوف')
      });
    }
  }

  getAllClassFee(): void {
    this.feeClassService.getAllFeeClass().subscribe({
      next: (res) => {
        this.FeeClass = res.data;
      },
      error: (err) => {
        console.error('Error fetching fees:', err);
      }
    });
  }

  deleteFeeClass(classId: number, feeId: number): void {
    this.feeClassService.DeleteFeeClass(classId, feeId).subscribe({
      next: res => {
        if (res.success === true) {
          this.toastr.success(res.data);
          this.getAllClassFee();
        }
        else
          this.toastr.error(res.data);
      }
    });
  }
  exist: boolean =false;

  GetFeeClassByID(classId: number, feeId: number): boolean {
    const match = this.FeeClass.some(fc => fc.classID === classId && fc.feeID === feeId);
    this.exist = match;
    console.log('Matched:', match);
    return match; // Always return a boolean value
  }
  
  
}
