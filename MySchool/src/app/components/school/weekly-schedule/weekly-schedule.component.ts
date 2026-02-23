import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { WeeklyScheduleService } from '../core/services/weekly-schedule.service';
import { ClassService } from '../core/services/class.service';
import { TermService } from '../core/services/term.service';
import { SubjectService } from '../core/services/subject.service';
import { TeacherService } from '../core/services/teacher.service';
import { CurriculmsPlanService } from '../core/services/curriculms-plan.service';
import { DivisionService } from '../core/services/division.service';
import {  AddWeeklySchedule, WeeklyScheduleGrid } from '../core/models/weekly-schedule.model';
import { CLass } from '../core/models/class.model';
import { Terms } from '../core/models/term.model';
import { Subjects } from '../core/models/subjects.model';
import { Employee } from '../core/models/employee.model';
import { divisions } from '../core/models/division.model';

interface ScheduleCell {
  dayOfWeek: number;
  periodNumber: number;
  subjectID?: number;
  teacherID?: number;
  subjectName?: string;
  teacherName?: string;
}

interface Period {
  periodNumber: number;
  periodName: string;
  startTime: string;
  endTime: string;
}

@Component({
  selector: 'app-weekly-schedule',
  templateUrl: './weekly-schedule.component.html',
  styleUrls: ['./weekly-schedule.component.scss']
})
export class WeeklyScheduleComponent implements OnInit {
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService);
  private store = inject(Store);
  private scheduleService = inject(WeeklyScheduleService);
  private classService = inject(ClassService);
  private termService = inject(TermService);
  private subjectService = inject(SubjectService);
  private teacherService = inject(TeacherService);
  private coursePlanService = inject(CurriculmsPlanService);
  private divisionService = inject(DivisionService);

  form: FormGroup;
  filterForm: FormGroup;

  // Data
  classes: CLass[] = [];
  terms: Terms[] = [];
  subjects: Subjects[] = [];
  teachers: Employee[] = [];
  allDivisions: divisions[] = [];
  filteredDivisions: divisions[] = [];
  scheduleGrid: WeeklyScheduleGrid | null = null;
  
  // Days of week (0 = Saturday, 1 = Sunday, etc.)
  daysOfWeek = [
    { value: 0, name: 'السبت' },
    { value: 1, name: 'الأحد' },
    { value: 2, name: 'الإثنين' },
    { value: 3, name: 'الثلاثاء' },
    { value: 4, name: 'الأربعاء' }
  ];

  // Periods (will be loaded from schedule or default) - always show all periods
  periods: Period[] = [
    { periodNumber: 1, periodName: 'الأولى', startTime: '08:00', endTime: '08:45' },
    { periodNumber: 2, periodName: 'الثانية', startTime: '08:45', endTime: '09:30' },
    { periodNumber: 3, periodName: 'الثالثة', startTime: '09:30', endTime: '10:15' },
    { periodNumber: 4, periodName: 'الرابعة', startTime: '10:30', endTime: '11:15' },
    { periodNumber: 5, periodName: 'الخامسة', startTime: '11:15', endTime: '12:00' },
    { periodNumber: 6, periodName: 'السادسة', startTime: '12:00', endTime: '12:45' }
  ];

  // Schedule cells: [day][period] = ScheduleCell
  scheduleCells: Map<string, ScheduleCell> = new Map();
  
  // CoursePlans: teachers with their subjects for the selected class/term
  coursePlans: any[] = [];
  
  // Menu state for three dots
  showMenu: Map<string, boolean> = new Map();
  
  selectedClassId: number | null = null;
  selectedTermId: number | null = null;
  selectedDivisionId: number | null = null;
  isLoading = false;
  hasChanges = false;

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr'))
  );

  constructor() {
    this.filterForm = this.fb.group({
      classId: [null],
      termId: [null],
      divisionId: [null]
    });

    this.form = this.fb.group({});
    
    // Ensure periods are always initialized with default values
    // This ensures all periods are always visible
    if (this.periods.length === 0) {
      this.periods = [
        { periodNumber: 1, periodName: 'الأولى', startTime: '08:00', endTime: '08:45' },
        { periodNumber: 2, periodName: 'الثانية', startTime: '08:45', endTime: '09:30' },
        { periodNumber: 3, periodName: 'الثالثة', startTime: '09:30', endTime: '10:15' },
        { periodNumber: 4, periodName: 'الرابعة', startTime: '10:30', endTime: '11:15' },
        { periodNumber: 5, periodName: 'الخامسة', startTime: '11:15', endTime: '12:00' },
        { periodNumber: 6, periodName: 'السادسة', startTime: '12:00', endTime: '12:45' }
      ];
    }
  }

  ngOnInit(): void {
    this.loadInitialData();
    
    // Watch for filter changes
    this.filterForm.get('classId')?.valueChanges.subscribe((classId) => {
      this.updateDivisionsByClass(classId);
      if (this.filterForm.get('classId')?.value && this.filterForm.get('termId')?.value) {
        this.loadSchedule();
      }
    });

    this.filterForm.get('termId')?.valueChanges.subscribe(() => {
      if (this.filterForm.get('classId')?.value && this.filterForm.get('termId')?.value) {
        this.loadSchedule();
      }
    });

    this.filterForm.get('divisionId')?.valueChanges.subscribe((divisionId) => {
      this.selectedDivisionId = divisionId || null;
      if (this.filterForm.get('classId')?.value && this.filterForm.get('termId')?.value) {
        this.loadSchedule();
      }
    });
  }

  loadInitialData(): void {
    // Load classes
    this.classService.GetAllNames().subscribe({
      next: (res) => {
        if (res.result) {
          this.classes = res.result;
        }
      },
      error: (err) => {
        console.error('Error loading classes:', err);
        this.toastr.error('فشل في تحميل الصفوف', 'خطأ');
      }
    });

    // Load terms
    this.termService.getAllTerm().subscribe({
      next: (res) => {
        if (res.result) {
            console.log("Terms",res.result);
          this.terms = res.result;
        }
      },
      error: (err) => {
        console.error('Error loading terms:', err);
        this.toastr.error('فشل في تحميل الفصول الدراسية', 'خطأ');
      }
    });

    // Load subjects
    this.subjectService.getAllSubjects().subscribe({
      next: (res) => {
        if (res.result) {
          this.subjects = res.result;
        }
      },
      error: (err) => {
        console.error('Error loading subjects:', err);
        this.toastr.error('فشل في تحميل المقررات', 'خطأ');
      }
    });

    // Load teachers using TeacherService
    this.teacherService.getAllTeacher().subscribe({
      next: (res) => {
        if (res.result && res.result.length > 0) {
          // Map Teachers to Employee model format
          this.teachers = res.result.map((teacher: any) => ({
            employeeID: teacher.teacherID,
            firstName: teacher.firstName || '',
            middleName: teacher.middleName || '',
            lastName: teacher.lastName || '',
            jopName: 'Teacher',
            address: teacher.address || null,
            mobile: teacher.phoneNumber || '',
            gender: teacher.gender || 'Male',
            hireDate: teacher.hireDate || new Date(),
            dob: teacher.dob || new Date(),
            email: teacher.email || null,
            imageURL: teacher.imageURL || null,
            managerID: teacher.managerID || null,
            teacherID: teacher.teacherID,
            userID: teacher.userID,
            userName: teacher.userName
          }));
          console.log('Teachers loaded:', this.teachers.length, 'teachers');
        } else {
          console.warn('No teachers found');
          this.teachers = [];
        }
      },
      error: (err) => {
        console.error('Error loading teachers:', err);
        this.toastr.error('فشل في تحميل المعلمين', 'خطأ');
        this.teachers = [];
      }
    });

    // Load divisions (but don't show them until class is selected)
    this.divisionService.GetAll().subscribe({
      next: (res) => {
        if (res.result) {
          this.allDivisions = res.result;
          // Don't populate filteredDivisions until a class is selected
          this.filteredDivisions = [];
        }
      },
      error: (err) => {
        console.error('Error loading divisions:', err);
        this.toastr.error('فشل في تحميل الأقسام', 'خطأ');
      }
    });
  }

  updateDivisionsByClass(classId: number | null): void {
    if (!classId) {
      this.filteredDivisions = [];
      this.filterForm.get('divisionId')?.setValue(null);
      this.selectedDivisionId = null;
      return;
    }

    // Filter divisions based on selected class
    this.filteredDivisions = this.allDivisions.filter(
      (division) => division.classID === classId
    );

    // Sort divisions by name for better UX
    this.filteredDivisions.sort((a, b) => {
      const nameA = (a.divisionName || '').trim();
      const nameB = (b.divisionName || '').trim();
      return nameA.localeCompare(nameB, 'ar');
    });

    // Clear division selection if current selection is not in filtered list
    const currentDivisionId = this.filterForm.get('divisionId')?.value;
    if (currentDivisionId && !this.filteredDivisions.find(d => d.divisionID === currentDivisionId)) {
      this.filterForm.get('divisionId')?.setValue(null);
      this.selectedDivisionId = null;
    }
  }

  loadSchedule(): void {
    const classId = this.filterForm.get('classId')?.value;
    const termId = this.filterForm.get('termId')?.value;
    const divisionId = this.filterForm.get('divisionId')?.value;

    if (!classId || !termId) {
      return;
    }

    this.isLoading = true;
    this.selectedClassId = classId;
    this.selectedTermId = termId;
    this.selectedDivisionId = divisionId || null;

    // Load CoursePlans to get teachers with subjects for this class/term
    this.coursePlanService.getAllCurriculmPlan().subscribe({
      next: (res) => {
        if (res.result) {
          // Filter CoursePlans for the selected class and term
          this.coursePlans = res.result.filter((plan: any) => 
            plan.classID === classId && plan.termID === termId
          );
          console.log('CoursePlans loaded:', this.coursePlans.length);
        }
      },
      error: (err) => {
        console.error('Error loading course plans:', err);
      }
    });

    // Log filtering parameters for debugging
    console.log('Loading schedule with filters:', { classId, termId, divisionId: divisionId || 'none' });
    
    this.scheduleService.GetScheduleGrid(classId, termId, divisionId || undefined).subscribe({
      next: (res) => {
        if (res.result) {
          this.scheduleGrid = res.result;
          console.log('Schedule loaded:', res.result.scheduleItems?.length || 0, 'items');
          
          // Merge periods from server with default periods - always show all periods
          // NEVER clear periods array - always keep all default periods visible
          if (res.result.periods && res.result.periods.length > 0) {
            // Create a map of server periods by period number for quick lookup
            const serverPeriodsMap = new Map<number, Period>(
              res.result.periods.map((p: any) => [p.periodNumber, {
                periodNumber: p.periodNumber,
                periodName: p.periodName || '',
                startTime: p.startTime || '',
                endTime: p.endTime || ''
              }])
            );
            
            // Update existing periods with server data (time/name), but keep all periods
            for (let i = 0; i < this.periods.length; i++) {
              const period: Period = this.periods[i];
              const serverPeriod = serverPeriodsMap.get(period.periodNumber);
              if (serverPeriod) {
                // Update time and name from server, but keep the period
                period.periodName = serverPeriod.periodName || period.periodName;
                period.startTime = serverPeriod.startTime || period.startTime;
                period.endTime = serverPeriod.endTime || period.endTime;
              }
            }
            
            // Add any new periods from server that don't exist in our list
            res.result.periods.forEach((serverPeriod: any) => {
              if (!this.periods.find(p => p.periodNumber === serverPeriod.periodNumber)) {
                this.periods.push({
                  periodNumber: serverPeriod.periodNumber,
                  periodName: serverPeriod.periodName,
                  startTime: serverPeriod.startTime,
                  endTime: serverPeriod.endTime
                });
              }
            });
            
            // Sort periods by period number to maintain order
            this.periods.sort((a, b) => a.periodNumber - b.periodNumber);
          }
          // If no periods from server, keep all default periods (always show all 6 periods)
          
          // Ensure we always have at least the default 6 periods
          const defaultPeriods = [1, 2, 3, 4, 5, 6];
          defaultPeriods.forEach(periodNum => {
            if (!this.periods.find(p => p.periodNumber === periodNum)) {
              const defaultTimes = [
                { start: '08:00', end: '08:45' },
                { start: '08:45', end: '09:30' },
                { start: '09:30', end: '10:15' },
                { start: '10:30', end: '11:15' },
                { start: '11:15', end: '12:00' },
                { start: '12:00', end: '12:45' }
              ];
              const periodNames = ['الأولى', 'الثانية', 'الثالثة', 'الرابعة', 'الخامسة', 'السادسة'];
              this.periods.push({
                periodNumber: periodNum,
                periodName: periodNames[periodNum - 1],
                startTime: defaultTimes[periodNum - 1].start,
                endTime: defaultTimes[periodNum - 1].end
              });
            }
          });
          this.periods.sort((a, b) => a.periodNumber - b.periodNumber);

          // Initialize schedule cells from loaded data
          this.scheduleCells.clear();
          res.result.scheduleItems.forEach((item: any) => {
            const key = `${item.dayOfWeek}-${item.periodNumber}`;
            this.scheduleCells.set(key, {
              dayOfWeek: item.dayOfWeek,
              periodNumber: item.periodNumber,
              subjectID: item.subjectID,
              teacherID: item.teacherID,
              subjectName: item.subjectName,
              teacherName: item.teacherName
            });
          });

          this.hasChanges = false;
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading schedule:', err);
        this.toastr.error('فشل في تحميل الجدول', 'خطأ');
        this.isLoading = false;
      }
    });
  }

  getCellKey(day: number, period: number): string {
    return `${day}-${period}`;
  }

  getCell(day: number, period: number): ScheduleCell | undefined {
    const key = this.getCellKey(day, period);
    return this.scheduleCells.get(key);
  }

  toggleMenu(day: number, period: number): void {
    const key = this.getCellKey(day, period);
    const currentState = this.showMenu.get(key) || false;
    // Close all other menus
    this.showMenu.clear();
    // Toggle this menu
    this.showMenu.set(key, !currentState);
  }

  closeAllMenus(): void {
    this.showMenu.clear();
  }

  // Get available teachers with subjects for a specific period (excluding already selected teachers for the same day)
  getAvailableTeachersForPeriod(day: number, period: number): any[] {
    // Get currently selected teacher IDs for this specific day (all periods)
    const selectedTeacherIds = new Set<number>();
    this.periods.forEach(p => {
      const cell = this.getCell(day, p.periodNumber);
      if (cell?.teacherID) {
        selectedTeacherIds.add(cell.teacherID);
      }
    });

    // Get teachers with subjects from CoursePlans, excluding already selected ones for this day
    const availableTeachers = this.coursePlans
      .filter((plan: any) => !selectedTeacherIds.has(plan.teacherID))
      .map((plan: any) => {
        // Extract subject name from coursePlanName (format: "SubjectName-ClassName")
        const subjectName = plan.coursePlanName?.split('-')[0]?.trim() || '';
        return {
          teacherID: plan.teacherID,
          teacherName: plan.teacherName,
          subjectID: plan.subjectID,
          subjectName: subjectName,
          displayName: `${plan.teacherName} - ${subjectName}`
        };
      });

    // Remove duplicates by teacherID (keep first occurrence)
    const uniqueTeachers = Array.from(
      new Map(availableTeachers.map(t => [t.teacherID, t])).values()
    );

    return uniqueTeachers;
  }

  // Select teacher from menu - auto-populate subject
  selectTeacherFromMenu(day: number, period: number, teacher: any): void {
    const key = this.getCellKey(day, period);
    let cell = this.scheduleCells.get(key);

    if (!cell) {
      cell = { dayOfWeek: day, periodNumber: period };
      this.scheduleCells.set(key, cell);
    }

    // Set teacher and auto-populate subject
    cell.teacherID = teacher.teacherID;
    cell.teacherName = teacher.teacherName;
    cell.subjectID = teacher.subjectID;
    cell.subjectName = teacher.subjectName;

    // Close menu
    this.closeAllMenus();
    this.hasChanges = true;
  }

  // Clear cell
  clearCell(day: number, period: number): void {
    const key = this.getCellKey(day, period);
    this.scheduleCells.delete(key);
    this.closeAllMenus();
    this.hasChanges = true;
  }

  onCellChange(day: number, period: number, field: 'subjectID' | 'teacherID', value: number | null): void {
    const key = this.getCellKey(day, period);
    let cell = this.scheduleCells.get(key);

    if (!cell) {
      cell = { dayOfWeek: day, periodNumber: period };
      this.scheduleCells.set(key, cell);
    }

    if (field === 'subjectID') {
      cell.subjectID = value || undefined;
      if (value) {
        const subject = this.subjects.find(s => s.subjectID === value);
        cell.subjectName = subject?.subjectName;
      } else {
        cell.subjectName = undefined;
      }
    } else if (field === 'teacherID') {
      cell.teacherID = value || undefined;
      if (value) {
        const teacher = this.teachers.find(t => t.employeeID === value);
        cell.teacherName = teacher ? `${teacher.firstName} ${teacher.lastName}`.trim() : undefined;
        
        // Auto-populate subject from CoursePlan if teacher is selected
        if (value && this.selectedClassId && this.selectedTermId) {
          const coursePlan = this.coursePlans.find((plan: any) => 
            plan.teacherID === value && 
            plan.classID === this.selectedClassId && 
            plan.termID === this.selectedTermId
          );
          if (coursePlan) {
            cell.subjectID = coursePlan.subjectID;
            cell.subjectName = coursePlan.coursePlanName?.split('-')[0] || '';
          }
        }
      } else {
        cell.teacherName = undefined;
        cell.subjectID = undefined;
        cell.subjectName = undefined;
      }
    }

    this.hasChanges = true;
  }

  saveChanges(): void {
    if (!this.selectedClassId || !this.selectedTermId) {
      this.toastr.warning('يرجى اختيار الصف والفصل الدراسي', 'تحذير');
      return;
    }

    // Get active year (assuming first active year)
    // In a real scenario, you'd get this from a service
    const yearId = 1; // This should come from your year service

    // Build schedule items from cells
    const schedules: AddWeeklySchedule[] = [];
    
    this.daysOfWeek.forEach(day => {
      this.periods.forEach(period => {
        const cell = this.getCell(day.value, period.periodNumber);
        if (cell && (cell.subjectID || cell.teacherID)) {
          schedules.push({
            dayOfWeek: day.value,
            periodNumber: period.periodNumber,
            startTime: period.startTime,
            endTime: period.endTime,
            classID: this.selectedClassId!,
            termID: this.selectedTermId!,
            yearID: yearId,
            subjectID: cell.subjectID,
            teacherID: cell.teacherID,
            divisionID: this.selectedDivisionId || undefined
          });
        }
      });
    });

    this.isLoading = true;
    this.scheduleService.BulkUpdate(schedules).subscribe({
      next: (res) => {
        this.toastr.success('تم حفظ الجدول بنجاح', 'نجاح');
        this.hasChanges = false;
        this.loadSchedule();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error saving schedule:', err);
        this.toastr.error('فشل في حفظ الجدول', 'خطأ');
        this.isLoading = false;
      }
    });
  }

  printSchedule(): void {
    // Ensure schedule is loaded before printing
    if (!this.selectedClassId || !this.selectedTermId) {
      this.toastr.warning('يرجى اختيار الصف والفصل الدراسي أولاً', 'تحذير');
      return;
    }

    // Ensure data is loaded and DOM is updated
    setTimeout(() => {
      // Force change detection to ensure bindings are updated
      if (this.classes.length === 0) {
        this.toastr.warning('جاري تحميل البيانات، يرجى المحاولة مرة أخرى', 'تحذير');
        return;
      }
      window.print();
    }, 200);
  }

  getSubjectOptions(): Subjects[] {
    return this.subjects;
  }

  getTeacherOptions(): any[] {
    return this.teachers.map(teacher => ({
      ...teacher,
      displayName: `${teacher.firstName || ''} ${teacher.middleName || ''} ${teacher.lastName || ''}`.trim()
    }));
  }

  isMenuOpen(day: number, period: number): boolean {
    const key = this.getCellKey(day, period);
    return this.showMenu.get(key) || false;
  }

  getSelectedClassName(): string {
    if (!this.selectedClassId || this.classes.length === 0) return '';
    const selectedClass = this.classes.find((c: any) => c.classID === this.selectedClassId);
    if (selectedClass) {
      return selectedClass.className || '';
    }
    // Fallback: try to find by any property that might exist
    const fallbackClass = this.classes.find((c: any) => 
      (c.classID === this.selectedClassId) || 
      (c.ClassID === this.selectedClassId) ||
      (c.id === this.selectedClassId)
    );
    return (fallbackClass as any)?.className || '';
  }

  getSelectedDivisionName(): string {
    if (!this.selectedDivisionId || this.filteredDivisions.length === 0) return '';
    const selectedDivision = this.filteredDivisions.find(d => d.divisionID === this.selectedDivisionId);
    return selectedDivision?.divisionName || '';
  }
}
