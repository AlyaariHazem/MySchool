import { Component, OnInit, inject } from '@angular/core';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { DivisionService } from 'app/components/school/core/services/division.service';
import { UnregisteredStudent , PromoteStudentRequest } from 'app/core/models/students.model';
import { divisions } from '../../../core/models/division.model';

@Component({
  selector: 'app-promote-student-dialog',
  templateUrl: './promote-student-dialog.component.html',
  styleUrls: ['./promote-student-dialog.component.scss']
})
export class PromoteStudentDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService);
  private divisionService = inject(DivisionService);

  students: UnregisteredStudent[] = [];
  divisions: divisions[] = [];
  studentForms: FormGroup[] = [];
  isLoading = false;
  autoPromote: boolean = false; // Auto-increment class option
  targetYearID: number | null = null; // Target year for promotion

  constructor(
    public ref: DynamicDialogRef,
    public config: DynamicDialogConfig
  ) {
    this.students = config.data?.students || [];
    this.targetYearID = config.data?.targetYearID || null;
    console.log('PromoteStudentDialog initialized with:', {
      studentsCount: this.students.length,
      targetYearID: this.targetYearID,
      configData: config.data
    });
  }

  ngOnInit(): void {
    this.loadDivisions();
    this.initializeForms();
  }

  loadDivisions(): void {
    console.log('Loading divisions with targetYearID:', this.targetYearID);
    
    // Load divisions filtered by target year if provided
    this.divisionService.GetAll(this.targetYearID || undefined).subscribe({
      next: (response) => {
        this.divisions = response.result || [];
        console.log(`Loaded ${this.divisions.length} divisions for targetYearID: ${this.targetYearID || 'null (active year)'}`);
        
        if (this.divisions.length === 0) {
          const message = this.targetYearID 
            ? `لا توجد أقسام في السنة المستهدفة (YearID: ${this.targetYearID}). سيتم عرض أقسام السنة النشطة كبديل.`
            : 'لا توجد أقسام في السنة النشطة. يرجى التأكد من وجود أقسام.';
          console.warn(message);
          this.toastr.warning(message, 'تحذير', { timeOut: 5000 });
        } else if (this.targetYearID) {
          // Log if we're showing divisions from active year as fallback
          console.log(`Showing ${this.divisions.length} divisions (may include active year divisions as fallback for target year ${this.targetYearID})`);
        }
        
        // If auto-promote is enabled, assign next class after divisions are loaded
        if (this.autoPromote && this.divisions.length > 0) {
          // Use setTimeout to ensure forms are initialized
          setTimeout(() => {
            this.autoAssignNextClass();
          }, 50);
        }
      },
      error: (error) => {
        console.error('Error loading divisions:', error);
        this.toastr.error('فشل في تحميل الأقسام', 'خطأ');
      }
    });
  }

  initializeForms(): void {
    this.studentForms = this.students.map(student => {
      const form = this.fb.group({
        studentID: [student.studentID, Validators.required],
        newDivisionID: [null]
      });
      
      // Handle disabled state for reactive forms
      if (this.autoPromote) {
        form.get('newDivisionID')?.disable();
      }
      
      return form;
    });
  }

  onAutoPromoteChange(): void {
    if (this.autoPromote) {
      // Disable all division dropdowns
      this.studentForms.forEach(form => {
        form.get('newDivisionID')?.disable();
      });
      
      // Wait a bit to ensure divisions are loaded
      if (this.divisions.length === 0) {
        // If divisions not loaded yet, wait for them
        setTimeout(() => {
          if (this.divisions.length > 0) {
            this.autoAssignNextClass();
          }
        }, 100);
      } else {
        this.autoAssignNextClass();
      }
    } else {
      // Enable all division dropdowns
      this.studentForms.forEach(form => {
        form.get('newDivisionID')?.enable();
        form.patchValue({ newDivisionID: null });
        form.get('newDivisionID')?.setValidators(Validators.required);
        form.get('newDivisionID')?.updateValueAndValidity();
      });
    }
  }

  autoAssignNextClass(): void {
    if (this.divisions.length === 0 || this.studentForms.length === 0) {
      console.warn('Cannot auto-assign: divisions or forms not ready');
      return;
    }

    let assignedCount = 0;
    let failedCount = 0;
    
    this.students.forEach((student, index) => {
      if (index >= this.studentForms.length) {
        console.warn(`Form index ${index} out of range`);
        return;
      }

      const nextDivision = this.findNextClassDivision(student);
      if (nextDivision) {
        const form = this.studentForms[index];
        // Enable control temporarily to set value, then disable if auto-promote is active
        form.get('newDivisionID')?.enable();
        form.patchValue({ newDivisionID: nextDivision.divisionID });
        form.get('newDivisionID')?.clearValidators();
        form.get('newDivisionID')?.updateValueAndValidity();
        
        // Disable again if auto-promote is active
        if (this.autoPromote) {
          form.get('newDivisionID')?.disable();
        }
        
        // Force form to mark as touched and dirty to ensure value is recognized
        form.get('newDivisionID')?.markAsTouched();
        form.get('newDivisionID')?.markAsDirty();
        
        assignedCount++;
        console.log(`Assigned division ${nextDivision.divisionID} (${nextDivision.divisionName}) to student ${student.studentName}`);
      } else {
        // If auto-assign failed, show warning but don't block
        failedCount++;
        console.warn(`Could not find next class for student: ${student.studentName}`, {
          currentClass: student.currentClassName,
          currentStage: student.currentStageName,
          currentDivision: student.currentDivisionName
        });
      }
    });

    if (failedCount > 0) {
      this.toastr.warning(
        `تم تعيين ${assignedCount} طالب تلقائياً. ${failedCount} طالب يحتاج اختيار يدوي`,
        'ملاحظة',
        { timeOut: 4000 }
      );
    } else if (assignedCount > 0) {
      this.toastr.success(`تم تعيين ${assignedCount} طالب تلقائياً`, 'نجاح', { timeOut: 2000 });
    } else {
      this.toastr.warning('لم يتم العثور على صف تالي لأي طالب', 'تحذير', { timeOut: 3000 });
    }
  }

  findNextClassDivision(student: UnregisteredStudent): divisions | null {
    if (!student.currentClassName || !student.currentStageName) {
      console.log('Missing class or stage info', student);
      return null;
    }

    // Get all divisions in the same stage
    const stageDivisions = this.divisions.filter(d => 
      d.stageName && 
      d.stageName.trim().toLowerCase() === student.currentStageName?.trim().toLowerCase()
    );

    if (stageDivisions.length === 0) {
      console.log('No divisions found in stage', student.currentStageName);
      return null;
    }

    const currentClass = student.currentClassName.trim();
    
    // Get unique classes in the stage, sorted by ClassID
    const classesInStage = stageDivisions
      .map(d => ({ classID: d.classID, className: d.classesName, division: d }))
      .filter((c, i, arr) => arr.findIndex(x => x.classID === c.classID) === i)
      .sort((a, b) => a.classID - b.classID);

    console.log('Classes in stage:', classesInStage.map(c => c.className));
    console.log('Current class:', currentClass);

    // Find current class index - try exact match first, then partial match
    let currentClassIndex = classesInStage.findIndex(c => 
      c.className?.trim().toLowerCase() === currentClass.toLowerCase()
    );

    // If exact match not found, try to find by ClassID if we have it
    if (currentClassIndex === -1) {
      // Try to find by matching part of the name
      currentClassIndex = classesInStage.findIndex(c => 
        c.className?.trim().toLowerCase().includes(currentClass.toLowerCase()) ||
        currentClass.toLowerCase().includes(c.className?.trim().toLowerCase() || '')
      );
    }

    if (currentClassIndex === -1) {
      console.log('Current class not found in stage classes');
      return null;
    }

    if (currentClassIndex === classesInStage.length - 1) {
      console.log('Student is already in the last class');
      return null;
    }

    // Get next class
    const nextClass = classesInStage[currentClassIndex + 1];
    console.log('Next class found:', nextClass.className);
    
    // Find a division in the next class (prefer same division name if possible)
    let preferredDivision = stageDivisions.find(d => 
      d.classID === nextClass.classID && 
      d.divisionName === student.currentDivisionName
    );

    if (!preferredDivision) {
      // If same division name not found, get first division in next class
      preferredDivision = stageDivisions.find(d => d.classID === nextClass.classID);
    }

    return preferredDivision || null;
  }

  getFormGroup(index: number): FormGroup {
    return this.studentForms[index];
  }

  onCancel(): void {
    this.ref.close();
  }

  getFilteredDivisions(student: UnregisteredStudent): divisions[] {
    // All divisions are already filtered by targetYearID when loaded
    // Filter out divisions with empty names and filter by stage if student has stage info
    let filtered = this.divisions.filter(d => 
      d.divisionName && d.divisionName.trim() !== ''
    );
    
    if (student.currentStageName) {
      filtered = filtered.filter(d => 
        d.stageName && d.stageName.trim().toLowerCase() === student.currentStageName?.trim().toLowerCase()
      );
      
      if (filtered.length === 0) {
        console.warn(`No divisions found in stage ${student.currentStageName} (after filtering empty names)`);
      }
    }
    
    // If still no divisions, show all divisions with names (even if from different stage)
    if (filtered.length === 0) {
      filtered = this.divisions.filter(d => 
        d.divisionName && d.divisionName.trim() !== ''
      );
    }
    
    return filtered;
  }

  onPromote(): void {
    // Collect students without division selected
    const studentsWithoutDivision: string[] = [];
    
    this.studentForms.forEach((form, index) => {
      const divisionID = form.get('newDivisionID')?.value;
      if (!divisionID || divisionID === null) {
        studentsWithoutDivision.push(this.students[index].studentName);
      }
    });

    // If there are students without division, show warning but allow partial promotion
    if (studentsWithoutDivision.length > 0) {
      const message = studentsWithoutDivision.length === 1
        ? `يرجى اختيار قسم للطالب: ${studentsWithoutDivision[0]}`
        : `يرجى اختيار قسم للطلاب: ${studentsWithoutDivision.slice(0, 3).join(', ')}${studentsWithoutDivision.length > 3 ? '...' : ''}`;
      
      this.toastr.warning(message, 'تحذير', { timeOut: 5000 });
      
      // If auto-promote is enabled, try to assign again
      if (this.autoPromote) {
        this.autoAssignNextClass();
        // Wait a bit then check again
        setTimeout(() => {
          const stillMissing = this.studentForms.filter(f => !f.get('newDivisionID')?.value);
          if (stillMissing.length > 0) {
            this.toastr.info(
              `سيتم ترقية ${this.studentForms.length - stillMissing.length} طالب فقط`,
              'ملاحظة'
            );
          }
        }, 500);
      }
      
      // Don't return - allow partial promotion
    }

    // Get all students with valid division IDs
    const promoteRequests: PromoteStudentRequest[] = this.studentForms
      .filter(form => {
        const divisionID = form.get('newDivisionID')?.value;
        return divisionID && divisionID !== null;
      })
      .map(form => ({
        studentID: form.value.studentID,
        newDivisionID: form.value.newDivisionID
      }));

    if (promoteRequests.length === 0) {
      this.toastr.error('لم يتم اختيار أي قسم للترقية. يرجى اختيار قسم واحد على الأقل', 'خطأ');
      return;
    }

    // If some students are missing, proceed with available students
    if (studentsWithoutDivision.length > 0 && promoteRequests.length < this.students.length) {
      // Proceed with partial promotion
    }

    this.ref.close({ students: promoteRequests });
  }
}
