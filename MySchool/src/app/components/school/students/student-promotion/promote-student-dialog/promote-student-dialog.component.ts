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
  copyCoursePlansFromCurrentYear: boolean = false; // Copy course plans from student's current year
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
          }, 150);
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
          } else {
            this.toastr.warning('لم يتم تحميل الأقسام بعد. يرجى الانتظار...', 'تحذير', { timeOut: 3000 });
          }
        }, 200);
      } else {
        // Use setTimeout to ensure forms are ready
        setTimeout(() => {
          this.autoAssignNextClass();
        }, 100);
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
    console.log('=== Finding next class division for student ===', {
      studentName: student.studentName,
      currentClassName: student.currentClassName,
      currentStageName: student.currentStageName,
      currentDivisionName: student.currentDivisionName,
      totalDivisions: this.divisions.length
    });

    // If no divisions loaded, return null
    if (this.divisions.length === 0) {
      console.warn('No divisions available');
      return null;
    }

    // Get all available divisions (filter out empty names)
    let availableDivisions = this.divisions.filter(d => 
      d.divisionName && d.divisionName.trim() !== '' &&
      d.classesName && d.classesName.trim() !== ''
    );

    console.log('Available divisions:', availableDivisions.length);

    // If student has stage info, try to filter by stage first
    if (student.currentStageName) {
      const stageDivisions = availableDivisions.filter(d => 
        d.stageName && 
        d.stageName.trim().toLowerCase() === student.currentStageName?.trim().toLowerCase()
      );
      
      if (stageDivisions.length > 0) {
        availableDivisions = stageDivisions;
        console.log('Filtered by stage:', availableDivisions.length, 'divisions');
      } else {
        console.warn('No divisions found in stage, using all available divisions');
      }
    }

    // Get unique classes, sorted by ClassID
    const uniqueClasses = availableDivisions
      .map(d => ({ classID: d.classID, className: d.classesName?.trim() || '', division: d }))
      .filter((c, i, arr) => arr.findIndex(x => x.classID === c.classID) === i)
      .sort((a, b) => a.classID - b.classID);

    console.log('Unique classes found:', uniqueClasses.map(c => ({ id: c.classID, name: c.className })));

    if (uniqueClasses.length === 0) {
      console.warn('No classes found');
      return null;
    }

    // If student has current class name, try to find it
    let currentClassIndex = -1;
    if (student.currentClassName) {
      const currentClass = student.currentClassName.trim().toLowerCase();
      
      // Try exact match first
      currentClassIndex = uniqueClasses.findIndex(c => 
        c.className.toLowerCase() === currentClass
      );

      // Try partial match
      if (currentClassIndex === -1) {
        currentClassIndex = uniqueClasses.findIndex(c => {
          const className = c.className.toLowerCase();
          return className.includes(currentClass) || currentClass.includes(className);
        });
      }

      // Try matching Arabic number words (أول, ثاني, ثالث, etc.)
      if (currentClassIndex === -1) {
        const arabicNumbers: { [key: string]: number } = {
          'أول': 1, 'اول': 1, 'first': 1,
          'ثاني': 2, 'second': 2,
          'ثالث': 3, 'third': 3,
          'رابع': 4, 'fourth': 4,
          'خامس': 5, 'fifth': 5,
          'سادس': 6, 'sixth': 6
        };
        
        const currentClassNum = arabicNumbers[currentClass];
        if (currentClassNum) {
          // Find class that contains the next number
          const nextClassNum = currentClassNum + 1;
          const nextNumWords = Object.keys(arabicNumbers).filter(k => arabicNumbers[k] === nextClassNum);
          
          currentClassIndex = uniqueClasses.findIndex(c => {
            const className = c.className.toLowerCase();
            return nextNumWords.some(word => className.includes(word));
          });
          
          // If found next class directly, return it
          if (currentClassIndex !== -1) {
            const nextClass = uniqueClasses[currentClassIndex];
            console.log('Found next class by Arabic number:', nextClass.className);
            
            // Find division in this class
            let preferredDivision = availableDivisions.find(d => 
              d.classID === nextClass.classID && 
              d.divisionName === student.currentDivisionName
            );

            if (!preferredDivision) {
              preferredDivision = availableDivisions.find(d => d.classID === nextClass.classID);
            }

            return preferredDivision || null;
          }
        }
      }

      console.log('Current class index:', currentClassIndex, 
        currentClassIndex >= 0 ? `(${uniqueClasses[currentClassIndex].className})` : 'not found');
    }

    // If we found current class and it's not the last one, get next class
    if (currentClassIndex >= 0 && currentClassIndex < uniqueClasses.length - 1) {
      const nextClass = uniqueClasses[currentClassIndex + 1];
      console.log('Next class found by index:', nextClass.className);
      
      // Find a division in the next class (prefer same division name if possible)
      let preferredDivision = availableDivisions.find(d => 
        d.classID === nextClass.classID && 
        d.divisionName === student.currentDivisionName
      );

      if (!preferredDivision) {
        // If same division name not found, get first division in next class
        preferredDivision = availableDivisions.find(d => d.classID === nextClass.classID);
      }

      if (preferredDivision) {
        console.log('Found division:', preferredDivision.divisionName, 'in class:', preferredDivision.classesName);
        return preferredDivision;
      }
    }

    // If current class not found or is last, try to find next class by ClassID increment
    if (student.currentDivisionID) {
      const currentDivision = availableDivisions.find(d => d.divisionID === student.currentDivisionID);
      if (currentDivision) {
        const currentClassID = currentDivision.classID;
        const nextClass = uniqueClasses.find(c => c.classID > currentClassID);
        
        if (nextClass) {
          console.log('Found next class by ClassID increment:', nextClass.className);
          
          let preferredDivision = availableDivisions.find(d => 
            d.classID === nextClass.classID && 
            d.divisionName === student.currentDivisionName
          );

          if (!preferredDivision) {
            preferredDivision = availableDivisions.find(d => d.classID === nextClass.classID);
          }

          return preferredDivision || null;
        }
      }
    }

    // Last resort: if we have multiple classes, try to get the second class (assuming first is current)
    if (uniqueClasses.length > 1) {
      const nextClass = uniqueClasses[1];
      console.log('Using second class as fallback:', nextClass.className);
      
      const preferredDivision = availableDivisions.find(d => d.classID === nextClass.classID);
      return preferredDivision || null;
    }

    console.warn('Could not find next class division');
    return null;
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

    this.ref.close({ 
      students: promoteRequests,
      copyCoursePlansFromCurrentYear: this.copyCoursePlansFromCurrentYear
    });
  }
}
