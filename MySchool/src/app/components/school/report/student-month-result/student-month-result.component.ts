// student-month-result.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { MonthlyResultService } from '../../core/services/monthly-resultt.service';
import { MonthlyResult } from '../../core/models/monthly-result.model';


@Component({
  selector: 'app-student-month-result',
  templateUrl: `./student-month-result.component.html`,
  styleUrl: `./student-month-result.component.scss`,
})
export class StudentMonthResultComponent implements OnInit {
  private title = 'Student Month Result';
  monthlyResults!: MonthlyResult[];
  monthlyResult = inject(MonthlyResultService);
  constructor() {
    this.monthlyResult.getMonthlyGradesReport().subscribe((res) => {
      this.monthlyResults = res;
    });
  }

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

  SchoolLogo = localStorage.getItem('SchoolImageURL');
  ngOnInit(): void {
  }

  printReport(subj: MonthlyResult): void {
    const page = document.getElementById(`report-${subj.studentID}`);
    if (!page) return;
    this.title = `${subj.studentName}_${subj.year}_${subj.month}_${subj.class}`;
    const links = Array.from(document.querySelectorAll('link[rel="stylesheet"], style'))
      .map(el => el.outerHTML)
      .join('');
    const base = `<base href="${document.baseURI}">`;

    const popup = window.open('', '', 'width=1000px,height=auto');
    if (!popup) return;

    popup.document.write(`
    <html>
      <head>
        <title>${this.title}</title>
        ${base}
        ${links}
        <style>
          @media print {
            body { margin: 0; direction: rtl; font-family: "Tajawal", "Arial", sans-serif; }
            .report, * { letter-spacing: 0 !important; }
          }
        </style>
      </head>
      <body>
        ${page.outerHTML}
      </body>
    </html>
  `);

    popup.document.close();
    popup.onload = () => popup.print();
  }


}
