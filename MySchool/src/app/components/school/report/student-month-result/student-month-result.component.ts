// student-month-result.component.ts
import { Component, OnInit } from '@angular/core';
interface SubjectGrade {
  name: string;
  max: number;
  score: number;
}

@Component({
  selector: 'app-student-month-result',
  templateUrl: `./student-month-result.component.html`,
  styleUrl: `./student-month-result.component.scss`,
})
export class StudentMonthResultComponent implements OnInit {
  private title = 'Student Month Result';

  getTitle(): string {
    return this.title;
  }

  getContent(): string {
  return `
  <div dir="rtl" style="font-family:Tajawal, sans-serif; line-height:1.7">
    <!-- top line -->
    <div style="display:flex; justify-content:space-between; margin-bottom:16px">
      <span style="font-weight:600">بسم الله الرحمن الرحيم</span>
    </div>

    <!-- main title -->
    <h2 style="color:#c47312; text-align:center; margin:0 0 4px 0">
      استمارة تسجيل الطالب للعام الدراسي
    </h2>
    <h2 style="color:#c47312; text-align:center; margin:0 0 24px 0">
      #SchoolYear#
    </h2>

    <!-- first two fields -->
    <p style="margin:0 0 4px 0">
      <span style="font-weight:bold; color:#c47312">رقم الطالب:</span>
      <span style="color:#c47312">#Sid#</span>
    </p>

    <p style="margin:0 0 16px 0">
      <span style="font-weight:bold; background:#00b5d8; color:#fff; padding:2px 4px;">#FullName#</span>
      : <span style="font-weight:bold">اسم الطالب</span>
    </p>

    <!-- three‑column table for the middle section -->
    <table style="width:100%; text-align:right;">
      <tr>
        <td style="padding:4px 0"><strong>المرحلة:</strong> #PhaseName#</td>
        <td style="padding:4px 0"><strong>الشعبة:</strong> #DivisionName#</td>
        <td style="padding:4px 0"><strong>الصف:</strong> #ClassName#</td>
      </tr>
    </table>

    <!-- spacer -->
    <div style="height:40px"></div>

    <!-- bottom row -->
    <table style="width:100%; text-align:right;">
      <tr>
        <td style="padding:4px 0"><strong>الجنس:</strong> #Sex#</td>
        <td style="padding:4px 0"><strong>العمر:</strong> #Age#</td>
      </tr>
      <tr>
        <td style="padding:4px 0"><strong>مكان الميلاد:</strong> #Birthplace#</td>
        <td style="padding:4px 0"><strong>الهاتف:</strong> #Phone#</td>
      </tr>
      <tr>
        <td colspan="2" style="padding:4px 0"><strong>العنوان:</strong> #Address#</td>
      </tr>
    </table>
  </div>
  `;
}
SchoolLogo=localStorage.getItem('SchoolImageURL');
ngOnInit(): void {
}
report = {
  school: 'Hossam Schools',
  year: '2021‑2022',
  term: 'First',
  month: 'أكتوبر',

  govHeader: [
    'الجمهورية اليمنية',
    'وزارة التربية والتعليم',
    'مكتب التربية أمانة العاصمة'
  ],

  studentId: '1001483',
  studentName: 'عبدالعليم علي حسن العزي',
  grade: 'أول',
  section: 'ب',

  subjects: <SubjectGrade[]>[
    { name: 'القرآن الكريم', max: 100, score: 100 },
    { name: 'القرآن الكريم', max: 100, score: 100 },
    { name: 'القرآن الكريم', max: 100, score: 100 },
    { name: 'علوم',          max: 100, score: 94  }
  ]
};

/** مجموع الدرجات */
get total(): number {
  return this.report.subjects.reduce((s, x) => s + x.score, 0);
}

/** النسبة المئوية */
get percentage(): number {
  const full = this.report.subjects.reduce((s, x) => s + x.max, 0);
  return +(this.total / full * 100).toFixed(0);
}

print(): void {
  const page = document.getElementById('report');
  if (!page) { return; }

  /* ️نسخ كل ملفات الأنماط الموجودة */
  const links = Array.from(document.querySelectorAll('link[rel="stylesheet"], style'))
                     .map(el => el.outerHTML)
                     .join('');

  const base = `<base href="${document.baseURI}">`;

  const popup = window.open('', '', 'width=1000px,height=auto');
  if (!popup) { return; }

  popup.document.write(`
    <html><head>
    <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;700&display=swap" rel="stylesheet">
    <title>${this.title}</title>
      ${base}
      ${links}
      <style>
      
        @media print {
          body{margin:0;direction:rtl;font-family:"Cairo","Tahoma",sans-serif}
          .report,*{letter-spacing:0!important}   /* إبقاء العربية متصلة */
        }
      </style>
    </head><body dir="rtl">
      ${page.outerHTML}
    </body></html>
  `);

  popup.document.close();
  popup.onload = () => popup.print();
}

}
