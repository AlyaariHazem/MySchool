import { Component, inject, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

import { ExamsService } from '../../../core/services/exams.service';
import { StudentExamCard } from '../../../core/models/exams.model';

@Component({
  selector: 'app-student-exams',
  templateUrl: './student-exams.component.html',
  styleUrls: ['./student-exams.component.scss'],
})
export class StudentExamsComponent implements OnInit {
  private readonly exams = inject(ExamsService);
  private readonly toastr = inject(ToastrService);

  upcomingOnly = false;
  rows: StudentExamCard[] = [];
  loading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.exams.getStudentMy(this.upcomingOnly).subscribe({
      next: (r) => {
        this.rows = (r.result ?? []) as StudentExamCard[];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('تعذر تحميل الامتحانات');
      },
    });
  }

  toggleFilter(): void {
    this.load();
  }
}
