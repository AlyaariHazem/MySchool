import { Component, OnInit } from '@angular/core';


@Component({
  selector: 'app-term-result',
  templateUrl: './term-result.component.html',
  styleUrls: ['./term-result.component.scss']
})
export class TermResultComponent implements OnInit {

  monthlyReports: MonthlyResult[] = [];
  constructor() { }
  private allReports: MonthlyResult[] = [];

  subjectNames: string[] = [];
  gradeTypes: string[] = [];
  print() {
    window.print();
  }
  months = [
    { label: 'يناير', value: 'January' },
    { label: 'فبراير', value: 'February' },
    { label: 'مارس', value: 'March' },
    { label: 'أكتوبر', value: 'October' }
  ];
  selectedMonth = 'October';

  ngOnInit() {
    this.loadMockData();
    this.prepareHeaders();
    this.filterByMonth();
  }

  
  private loadMockData() {
    this.allReports = [
      {
        studentID: 1,
        studentName: 'هاني هاني البحري',
        month: 'October',
        percentage: 85.2,
        gradeTotal: 426,
        gradeSubjects: [
          {
            subjectID: 1, subjectName: 'قرآن كريم',
            grades: { '20 واج': 20, 'موا 20': 19, 'مشا 10': 10, 'شفه 10': 7, 'تحر 40': 34, 'المجموع 100': 99 }
          },
          {
            subjectID: 2, subjectName: 'تربية إسلامية',
            grades: { '20 واج': 20, 'موا 20': 19, 'مشا 10': 10, 'شفه 10': 7, 'تحر 40': 34, 'المجموع 100': 99 }
          },
          {
            subjectID: 3, subjectName: 'رياضيات',
            grades: { '20 واج': 20, 'موا 20': 19, 'مشا 10': 10, 'شفه 10': 7, 'تحر 40': 34, 'المجموع 100': 99 }
          },
          {
            subjectID: 4, subjectName: 'علوم',
            grades: { '20 واج': 20, 'موا 20': 19, 'مشا 10': 10, 'شفه 10': 7, 'تحر 40': 34, 'المجموع 100': 99 }
          },
          {
            subjectID: 5, subjectName: 'لغة عربية',
            grades: { '20 واج': 20, 'موا 20': 19, 'مشا 10': 10, 'شفه 10': 7, 'تحر 40': 34, 'المجموع 100': 99 }
          }
        ]
      },
      {
        studentID: 2,
        studentName: 'صلاح محمد البحري',
        month: 'October',
        percentage: 100,
        gradeTotal: 500,
        gradeSubjects: [
          { subjectID: 1, subjectName: 'قرآن كريم', grades: { '20 واج': 20, 'موا 20': 19, 'مشا 10': 10, 'شفه 10': 7, 'تحر 40': 34, 'المجموع 100': 99 } },
          { subjectID: 2, subjectName: 'تربية إسلامية', grades: { '20 واج': 20, 'موا 20': 19, 'مشا 10': 10, 'شفه 10': 7, 'تحر 40': 34, 'المجموع 100': 99 } },
          { subjectID: 3, subjectName: 'رياضيات', grades: { '20 واج': 20, 'موا 20': 19, 'مشا 10': 10, 'شفه 10': 7, 'تحر 40': 34, 'المجموع 100': 99 } },
          { subjectID: 4, subjectName: 'علوم', grades: { '20 واج': 20, 'موا 20': 19, 'مشا 10': 10, 'شفه 10': 7, 'تحر 40': 34, 'المجموع 100': 99 } },
          { subjectID: 5, subjectName: 'لغة عربية', grades: { '20 واج': 20, 'موا 20': 19, 'مشا 10': 10, 'شفه 10': 7, 'تحر 40': 34, 'المجموع 100': 99 } }
        ]
      }
    ];
  }

  private prepareHeaders() {
    this.subjectNames = [...new Set(
      this.allReports.flatMap(r => r.gradeSubjects.map(s => s.subjectName))
    )];

    const firstGrades = this.allReports[0]?.gradeSubjects[0]?.grades ?? {};
    this.gradeTypes = Object.keys(firstGrades);
  }

  filterByMonth() {
    this.monthlyReports = this.allReports.filter(r => r.month === this.selectedMonth);
  }

  getGradeForSubjectType(
    subjects: GradeSubject[],
    subjectName: string,
    type: string
  ): number {
    return subjects.find(s => s.subjectName === subjectName)?.grades[type] ?? 0;
  }

}
export interface MonthlyResult {
  studentID: number;
  studentName: string;
  month: string;
  percentage: number;
  gradeTotal: number;
  gradeSubjects: GradeSubject[];
}

export interface GradeSubject {
  subjectID: number;
  subjectName: string;
  grades: { [gradeType: string]: number };
}
