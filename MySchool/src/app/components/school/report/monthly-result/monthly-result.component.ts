import { Component, OnInit } from '@angular/core';


@Component({
  selector: 'app-monthly-result',
  templateUrl: './monthly-result.component.html',
  styleUrls: ['./monthly-result.component.scss']
})
export class MonthlyResultComponent implements OnInit {

  monthlyReports: MonthlyResult[] = [];
  constructor() { }
  private allReports: MonthlyResult[] = [];

  subjectNames: string[] = [];
  gradeTypes: string[] = [];
  print(){
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
    this.loadMockData();                // بيانات وهميّة
    this.prepareHeaders();              // اشتق الرؤوس
    this.filterByMonth();               // أول تصفية
  }

  /* درجات تجريبية مقتبسة من لقطة الشاشة */
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
            grades: { q1: 20, q2: 19, q3: 10, qTotal: 99 }
          },
          {
            subjectID: 2, subjectName: 'تربية إسلامية',
            grades: { i1: 20, i2: 20, i3: 10, iTotal: 100 }
          },
          {
            subjectID: 3, subjectName: 'رياضيات',
            grades: { m1: 20, m2: 19, m3: 10, mTotal: 74 }
          },
          {
            subjectID: 4, subjectName: 'علوم',
            grades: { s1: 19, s2: 13, s3: 7, sTotal: 84 }
          },
          {
            subjectID: 5, subjectName: 'لغة عربية',
            grades: { a1: 10, a2: 6, a3: 8, aTotal: 69 }
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
          { subjectID: 1, subjectName: 'قرآن كريم', grades: { q1: 20, q2: 20, q3: 40, qTotal: 100 } },
          { subjectID: 2, subjectName: 'تربية إسلامية', grades: { i1: 20, i2: 20, i3: 40, iTotal: 100 } },
          { subjectID: 3, subjectName: 'رياضيات', grades: { m1: 20, m2: 20, m3: 60, mTotal: 100 } },
          { subjectID: 4, subjectName: 'علوم', grades: { s1: 20, s2: 20, s3: 60, sTotal: 100 } },
          { subjectID: 5, subjectName: 'لغة عربية', grades: { a1: 20, a2: 20, a3: 60, aTotal: 100 } }
        ]
      }
    ];
  }

  /* استخرج أسماء المواد وأنواع الدرجات */
  private prepareHeaders() {
    this.subjectNames = [...new Set(
      this.allReports.flatMap(r => r.gradeSubjects.map(s => s.subjectName))
    )];

    /* أنواع الدرجات من أوّل مادة (على فرض أنها موحّدة للجميع) */
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
  percentage: number;              // النسبة العامة
  gradeTotal: number;              // مجموع الشهر
  gradeSubjects: GradeSubject[];
}

export interface GradeSubject {
  subjectID: number;
  subjectName: string;
  grades: { [gradeType: string]: number };   // مفتاح النوع ← الدرجة
}
