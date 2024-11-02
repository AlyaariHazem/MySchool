import { AfterViewInit, Component, HostListener, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Stages } from '../../../../core/models/stages-grades.modul';
import { StageService } from '../../../../core/services/stage.service';
import { ToastrService } from 'ngx-toastr';
import { Observable, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { GradeService } from '../../../../core/services/grade.service';

@Component({
  selector: 'app-stages-grades',
  templateUrl: './stages-grades.component.html',
  styleUrls: ['./stages-grades.component.scss']
})
export class StagesGradesComponent implements AfterViewInit, OnInit {
  activeTab: string = 'News';
  form: FormGroup;
  stages: Stages[] = [];
  combinedData$: Observable<any[]> | undefined;
  outerDropdownState: { [key: string]: boolean } = {};
  innerDropdownState: { [key: string]: { [key: string]: boolean } } = {};
  currentPage: { [key: string]: number } = {};

  constructor(
    private stageService: StageService,
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    private gradesService: GradeService
  ) {
    this.form = this.formBuilder.group({
      id: '',
      stage: ['', Validators.required],
      note: '',
      state: true
    });
  }

  ngOnInit(): void {
    this.getStages();
    const stages$ = this.stageService.getStages();
    const grades$ = this.gradesService.getGrades();
    
    this.combinedData$ = combineLatest([stages$, grades$]).pipe(
      map(([stages, grades]) => {
        return stages.map(stage => {
          const gradesForStage = grades.filter(grade => grade.stage === stage.id);
          const totalStudents = gradesForStage.reduce((acc, grade) => acc + grade.totalStudents, 0);
          const gradesWithDivisions = gradesForStage.map(grade => ({
            ...grade
          }));
          this.currentPage[stage.id] = 0; // Initialize the current page for each stage
          return {
            ...stage,
            grades: gradesWithDivisions,
            totalStudents
          };
        });
      })
    );
  }

  ngAfterViewInit(): void {
    const defaultOpen = document.getElementById('defaultOpen');
    if (defaultOpen) {
      defaultOpen.click();
    }
  }

  openPage(pageName: string, elmnt: EventTarget | null): void {
    let i: number;
    let tabcontent: HTMLCollectionOf<HTMLElement>;
    let tablinks: HTMLCollectionOf<HTMLElement>;

    tabcontent = document.getElementsByClassName("tabcontent") as HTMLCollectionOf<HTMLElement>;
    for (i = 0; i < tabcontent.length; i++) {
      tabcontent[i].style.display = "none";
    }

    tablinks = document.getElementsByClassName("tablink") as HTMLCollectionOf<HTMLElement>;
    for (i = 0; i < tablinks.length; i++) {
      tablinks[i].classList.remove('active'); // Remove active class from all buttons
    }

    document.getElementById(pageName)!.style.display = "block";
    if (elmnt instanceof HTMLElement) {
      elmnt.classList.add('active'); // Add active class to the clicked button
    }

    this.activeTab = pageName;
  }

  getStages() {
    this.stageService.getStages().subscribe(stages => {
      this.stages = stages;
    });
  }

  addStage() {
    if (this.form.valid) {
      this.stageService.addStage(this.form.value).subscribe(() => {
        this.getStages();
        this.form.reset({ state: true });
        this.toastr.success('تم إضافة المرحلة بنجاح');
      });
    } else {
      this.toastr.error('يجب أن تدخل بيانات');
    }
  }

  editStage(stage: Stages) {
    this.form.patchValue(stage);
    this.stageService.editStage(stage).subscribe(() => {
      this.toastr.success('تم تحديث المرحلة بنجاح');
      this.getStages();
    });
  }

  deleteStage(stageId: string) {
    this.stageService.deleteStage(stageId).subscribe(() => {
      this.toastr.success('تم حذف المرحلة بنجاح');
      this.getStages();
    });
  }

  // Manage dropdown states
  openOuterDropdown: any = null;
  openInnerDropdown: any = null;
  openInnerDivision: any = null;

  toggleOuterDropdown(item: any): void {
    if (this.openOuterDropdown === item) {
      this.openOuterDropdown = null;
    } else {
      this.openOuterDropdown = item;
    }
  }

  isOuterDropdownOpen(item: any): boolean {
    return this.openOuterDropdown === item;
  }

  toggleInnerDropdown(item: any, division: any): void {
    if (this.openInnerDropdown === item && this.openInnerDivision === division) {
      this.openInnerDropdown = null;
      this.openInnerDivision = null;
    } else {
      this.openInnerDropdown = item;
      this.openInnerDivision = division;
    }
  }

  isInnerDropdownOpen(item: any, division: any): boolean {
    return this.openInnerDropdown === item && this.openInnerDivision === division;
  }

  // Pagination logic
  maxRowsPerPage = 3;
  
  getPaginatedGrades(item: any) {
    const startIndex = this.currentPage[item.id] * this.maxRowsPerPage;
    const endIndex = startIndex + this.maxRowsPerPage;
    return item.grades.slice(startIndex, endIndex);
  }

  nextPage(item: any) {
    if ((this.currentPage[item.id] + 1) * this.maxRowsPerPage < item.grades.length) {
      this.currentPage[item.id]++;
    }
  }

  previousPage(item: any) {
    if (this.currentPage[item.id] > 0) {
      this.currentPage[item.id]--;
    }
  }

  getTotalPages(item: any): number {
    return Math.ceil(item.grades.length / this.maxRowsPerPage);
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    const target = event.target as HTMLElement;

    if (!target.closest('.dropdown-menu') && !target.closest('.btn')) {
      this.openOuterDropdown = null;
      this.openInnerDropdown = null;
      this.openInnerDivision = null;
    }
  }
}
