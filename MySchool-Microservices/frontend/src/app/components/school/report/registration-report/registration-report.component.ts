import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { ClassService } from '../../core/services/class.service';
import { DivisionService } from '../../core/services/division.service';
import { ReportTemplateService } from '../../core/services/report-template.service';
import { StudentService } from '../../../../core/services/student.service';
import { divisions } from '../../core/models/division.model';
import { GuardianChildReportOption } from '../../../../core/models/students.model';

/** API GET Students/{id} body (camelCase). */
type StudentForUpdate = {
  studentID?: number;
  studentFirstName?: string;
  studentMiddleName?: string;
  studentLastName?: string;
  studentGender?: string;
  studentDOB?: string;
  placeBirth?: string;
  studentPhone?: string;
  studentAddress?: string;
  divisionID?: number;
  classID?: number;
};

type StudentSelectRow = {
  studentID: number;
  displayName: string;
  classID: number;
  divisionID: number;
};

@Component({
  selector: 'app-registration-report',
  templateUrl: './registration-report.component.html',
  styleUrls: ['./registration-report.component.scss'],
})
export class RegistrationReportComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private classService = inject(ClassService);
  private divisionSerivce = inject(DivisionService);
  private studentService = inject(StudentService);
  private reportTemplateService = inject(ReportTemplateService);
  private toastr = inject(ToastrService);
  private sanitizer = inject(DomSanitizer);

  form!: FormGroup;
  AllClasses: { classID: number; className: string }[] = [];
  divisions: divisions[] = [];
  fiteredDivisions: divisions[] = [];
  studentSelectOptions: StudentSelectRow[] = [];

  templateHtml = '';
  processedHtml = '';
  safeReportHtml: SafeHtml | null = null;
  isLoadingTemplate = false;
  isLoadingStudent = false;

  readonly isGuardian =
    typeof localStorage !== 'undefined' && localStorage.getItem('userType') === 'GUARDIAN';

  SchoolLogo = localStorage.getItem('SchoolImageURL') || '';

  private landscapePrintStyleEl: HTMLStyleElement | null = null;

  /** Chrome يتجاهل غالباً @page المسماة؛ نستخدم @page الافتراضي داخل جلسة الطباعة فقط */
  private readonly onBeforePrint = (): void => {
    const report = document.getElementById('report');
    if (!report?.classList.contains('registration-report-print') || !this.processedHtml?.trim()) {
      return;
    }
    if (this.landscapePrintStyleEl?.isConnected) {
      return;
    }
    const style = document.createElement('style');
    style.setAttribute('data-registration-print', 'true');
    style.textContent = `
@media print {
  @page {
    size: A4 landscape !important;
    margin: 10mm 12mm !important;
  }
}`;
    document.head.appendChild(style);
    this.landscapePrintStyleEl = style;
  };

  private readonly onAfterPrint = (): void => {
    this.landscapePrintStyleEl?.remove();
    this.landscapePrintStyleEl = null;
  };

  ngOnInit(): void {
    this.form = this.fb.group({
      classId: [this.isGuardian ? 0 : 1],
      divisionId: [this.isGuardian ? 0 : 1],
      studentId: [0],
    });

    this.getAllClasses();
    this.getAllDivision();
    this.loadRegistrationTemplate();

    if (!this.isGuardian) {
      this.form.get('classId')?.valueChanges.subscribe((classId: number) => {
        this.fiteredDivisions = this.divisions.filter((d) => d.classID === classId);
        this.loadStaffStudentOptions();
      });
      this.form.get('divisionId')?.valueChanges.subscribe(() => {
        this.loadStaffStudentOptions();
      });
    }

    this.form.get('studentId')?.valueChanges.subscribe((id: number) => {
      if (this.isGuardian && id) {
        const row = this.studentSelectOptions.find((s) => s.studentID === id);
        if (row) {
          this.form.patchValue(
            { classId: row.classID, divisionId: row.divisionID },
            { emitEvent: false }
          );
        }
      }
      if (id) {
        this.buildReport();
      } else {
        this.processedHtml = '';
        this.safeReportHtml = null;
      }
    });

    if (this.isGuardian) {
      this.loadGuardianStudentOptions();
    }

    window.addEventListener('beforeprint', this.onBeforePrint);
    window.addEventListener('afterprint', this.onAfterPrint);
  }

  ngOnDestroy(): void {
    window.removeEventListener('beforeprint', this.onBeforePrint);
    window.removeEventListener('afterprint', this.onAfterPrint);
    this.landscapePrintStyleEl?.remove();
    this.landscapePrintStyleEl = null;
  }

  private loadRegistrationTemplate(): void {
    this.isLoadingTemplate = true;
    const schoolIdStr = localStorage.getItem('schoolId');
    const schoolId = schoolIdStr ? parseInt(schoolIdStr, 10) : undefined;

    this.reportTemplateService.getTemplateByCode('REGISTRATION_FORM', schoolId).subscribe({
      next: (t) => {
        this.templateHtml = t.templateHtml || '';
        this.isLoadingTemplate = false;
        if (this.form.get('studentId')?.value) {
          this.buildReport();
        }
      },
      error: () => {
        this.templateHtml = this.defaultTemplate();
        this.isLoadingTemplate = false;
        this.toastr.info('سيُستخدم نموذج افتراضي حتى تُحفظ استمارة من تخصيص التقارير.', 'قالب التسجيل');
      },
    });
  }

  private defaultTemplate(): string {
    return `<div dir="rtl" style="line-height:1.8;padding:16px;font-family:Tajawal,Arial,sans-serif">
<p style="text-align:center">بسم الله الرحمن الرحيم</p>
<h2 style="text-align:center;color:#a86b00">استمارة تسجيل الطالب للعام الدراسي</h2>
<p><b>اسم الطالب:</b> #FullName#</p>
<p><b>المرحلة:</b> #PhaseName# &nbsp; <b>الشعبة:</b> #DivisionName#</p>
<p><b>الصف:</b> #ClassName# &nbsp; <b>العمر:</b> #Age#</p>
<p><b>النوع:</b> #Sex# &nbsp; <b>مكان الميلاد:</b> #Birthplace#</p>
<p><b>الهاتف:</b> #Phone# &nbsp; <b>العنوان:</b> #Address#</p>
</div>`;
  }

  getAllClasses(): void {
    this.classService.GetAllNames().subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.AllClasses = res.result || [];
        }
      },
      error: () => this.toastr.error('تعذر تحميل الصفوف'),
    });
  }

  getAllDivision(): void {
    this.divisionSerivce.GetAll().subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.divisions = res.result || [];
          if (!this.isGuardian) {
            const cid = this.form.get('classId')?.value;
            this.fiteredDivisions = this.divisions.filter((d) => d.classID === cid);
            this.loadStaffStudentOptions();
          }
        }
      },
      error: () => this.toastr.error('تعذر تحميل الشُعب'),
    });
  }

  private loadGuardianStudentOptions(): void {
    this.studentService.getGuardianMyChildrenForReport().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.studentSelectOptions = [];
          return;
        }
        const raw = (res.result ?? []) as GuardianChildReportOption[];
        this.studentSelectOptions = raw.map((r) => ({
          studentID: r.studentID,
          displayName: r.displayName,
          classID: r.classID,
          divisionID: r.divisionID,
        }));
      },
      error: () => {
        this.studentSelectOptions = [];
        this.toastr.error('تعذر تحميل قائمة الأبناء');
      },
    });
  }

  private loadStaffStudentOptions(): void {
    const classId = Number(this.form.get('classId')?.value);
    const divisionId = Number(this.form.get('divisionId')?.value);
    if (!classId || !divisionId) {
      this.studentSelectOptions = [];
      return;
    }
    this.studentService.getStudentsPageForAttendance(1, 500, { classId, divisionId }).subscribe({
      next: (page) => {
        const rows = page.data ?? [];
        this.studentSelectOptions = [
          { studentID: 0, displayName: '— اختر طالب —', classID: classId, divisionID: divisionId },
          ...rows.map((s: any) => ({
            studentID: s.studentID,
            displayName: [s.fullName?.firstName, s.fullName?.middleName, s.fullName?.lastName]
              .filter(Boolean)
              .join(' ')
              .replace(/\s+/g, ' ')
              .trim(),
            classID: classId,
            divisionID: divisionId,
          })),
        ];
      },
      error: () => {
        this.studentSelectOptions = [];
        this.toastr.error('تعذر تحميل الطلاب');
      },
    });
  }

  refreshReport(): void {
    this.buildReport();
  }

  private buildReport(): void {
    const sid = Number(this.form.get('studentId')?.value);
    if (!sid) {
      this.processedHtml = '';
      this.safeReportHtml = null;
      return;
    }
    const html = this.templateHtml || this.defaultTemplate();
    if (this.isGuardian) {
      this.fillFromGuardianRow(sid, html);
      return;
    }
    this.isLoadingStudent = true;
    this.studentService.getStudentById(sid).subscribe({
      next: (raw: StudentForUpdate) => {
        this.processedHtml = this.applyPlaceholders(html, raw);
        this.safeReportHtml = this.sanitizer.bypassSecurityTrustHtml(this.processedHtml);
        this.isLoadingStudent = false;
      },
      error: () => {
        this.isLoadingStudent = false;
        const row = this.studentSelectOptions.find((s) => s.studentID === sid);
        if (row) {
          this.processedHtml = this.applyPlaceholdersMinimal(html, row);
          this.safeReportHtml = this.sanitizer.bypassSecurityTrustHtml(this.processedHtml);
          this.toastr.warning('عُرضت بيانات محدودة للطالب.', 'تنبيه');
        } else {
          this.toastr.error('تعذر تحميل بيانات الطالب');
        }
      },
    });
  }

  private fillFromGuardianRow(sid: number, html: string): void {
    const row = this.studentSelectOptions.find((s) => s.studentID === sid);
    if (!row) {
      this.processedHtml = '';
      this.safeReportHtml = null;
      return;
    }
    const cls = this.AllClasses.find((c) => c.classID === row.classID);
    const div = this.divisions.find((d) => d.divisionID === row.divisionID);
    const map: Record<string, string> = {
      FullName: row.displayName,
      StudentId: String(row.studentID),
      SID: String(row.studentID),
      ClassName: cls?.className ?? '',
      DivisionName: div?.divisionName ?? '',
      PhaseName: div?.stageName ?? '',
      SchoolYear: this.schoolYearText(),
      Age: '',
      Sex: '',
      Birthplace: '',
      Phone: '',
      Address: '',
    };
    this.processedHtml = this.replaceMap(html, map);
    this.safeReportHtml = this.sanitizer.bypassSecurityTrustHtml(this.processedHtml);
  }

  private applyPlaceholders(html: string, s: StudentForUpdate): string {
    const fn = [s.studentFirstName, s.studentMiddleName, s.studentLastName]
      .filter(Boolean)
      .join(' ')
      .replace(/\s+/g, ' ')
      .trim();
    const cls = this.AllClasses.find((c) => c.classID === s.classID);
    const div = this.divisions.find((d) => d.divisionID === s.divisionID);
    const age = this.ageFromDob(s.studentDOB);
    const map: Record<string, string> = {
      FullName: fn,
      StudentId: String(s.studentID ?? ''),
      SID: String(s.studentID ?? ''),
      ClassName: cls?.className ?? '',
      DivisionName: div?.divisionName ?? '',
      PhaseName: div?.stageName ?? '',
      SchoolYear: this.schoolYearText(),
      Age: age !== null ? String(age) : '',
      Sex: this.genderAr(s.studentGender),
      Birthplace: s.placeBirth ?? '',
      Phone: s.studentPhone ?? '',
      Address: s.studentAddress ?? '',
    };
    return this.replaceMap(html, map);
  }

  private applyPlaceholdersMinimal(html: string, row: StudentSelectRow): string {
    const cls = this.AllClasses.find((c) => c.classID === row.classID);
    const div = this.divisions.find((d) => d.divisionID === row.divisionID);
    const map: Record<string, string> = {
      FullName: row.displayName,
      StudentId: String(row.studentID),
      SID: String(row.studentID),
      ClassName: cls?.className ?? '',
      DivisionName: div?.divisionName ?? '',
      PhaseName: div?.stageName ?? '',
      SchoolYear: this.schoolYearText(),
      Age: '',
      Sex: '',
      Birthplace: '',
      Phone: '',
      Address: '',
    };
    return this.replaceMap(html, map);
  }

  private replaceMap(html: string, map: Record<string, string>): string {
    let out = html;
    for (const [key, val] of Object.entries(map)) {
      const re = new RegExp(`#${key}#`, 'g');
      out = out.replace(re, val ?? '');
    }
    return out;
  }

  private schoolYearText(): string {
    return (
      localStorage.getItem('academicYear')?.trim() ||
      localStorage.getItem('SchoolYear')?.trim() ||
      ''
    );
  }

  private genderAr(g?: string): string {
    if (!g) {
      return '';
    }
    const x = g.toLowerCase();
    if (x === 'male' || x === 'm' || g === 'ذكر') {
      return 'ذكر';
    }
    if (x === 'female' || x === 'f' || g === 'أنثى') {
      return 'أنثى';
    }
    return g;
  }

  private ageFromDob(iso?: string): number | null {
    if (!iso) {
      return null;
    }
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) {
      return null;
    }
    const diff = Date.now() - d.getTime();
    return Math.max(0, Math.floor(diff / (365.25 * 24 * 3600 * 1000)));
  }

  /**
   * `beforeprint` يحقن `@page` الافتراضي (A4 بالعرض) — يتوافق مع معاينة Chrome أكثر من `@page` المسماة.
   */
  printReport(): void {
    if (!this.processedHtml?.trim()) {
      this.toastr.warning('اختر طالباً أولاً', 'تنبيه');
      return;
    }
    if (!document.getElementById('report')) {
      this.toastr.error('لم يتم العثور على محتوى الطباعة', 'خطأ');
      return;
    }
    this.onBeforePrint();
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        window.print();
      });
    });
  }
}
