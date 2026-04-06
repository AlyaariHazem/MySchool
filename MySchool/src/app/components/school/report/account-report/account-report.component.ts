import { Component, ElementRef, ViewChild, OnInit, inject } from '@angular/core';
import { AutoComplete } from 'primeng/autocomplete';
import { ReportTemplateService } from '../../core/services/report-template.service';
import { AccountService } from '../../core/services/account.service';
import { StudentService } from '../../../../core/services/student.service';
import { StudentNameIdDTO, StudentNameIdSearchRequest } from '../../../../core/models/students.model';
import { StudentAccounts } from '../../core/models/accounts.model';
import { ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../../../environments/environment';
import {
  AutoCompleteCompleteEvent,
  AutoCompleteLazyLoadEvent,
  AutoCompleteSelectEvent
} from 'primeng/autocomplete';

@Component({
  selector: 'app-account-report',
  templateUrl: './account-report.component.html',
  styleUrl: './account-report.component.scss'
})
export class AccountReportComponent implements OnInit {
  /* يرتبط بالقسم الذى نريد طباعته فقط */
  @ViewChild('printArea', { static: true })
  printArea!: ElementRef<HTMLDivElement>;

  @ViewChild('studentAccountAutocomplete')
  studentAccountAutocomplete?: AutoComplete;

  // Services
  private reportTemplateService = inject(ReportTemplateService);
  private accountService = inject(AccountService);
  private studentService = inject(StudentService);
  private route = inject(ActivatedRoute);
  private toastr = inject(ToastrService);

  // School data will be loaded from database
  logo: string = '';
  schoolName: string = '';
  schoolAddress: string = '';
  schoolPhone: string = '';
  academicYear: string = '';
  
  // Template HTML from database
  templateHtml: string = '';
  processedHtml: string = '';
  isLoading = false;

  // Account data (will be loaded from database)
  header = {
    accountNo: '',
    guardian: '',
    createdDate: '',
    totalDebit: 0,
    totalCredit: 0,
    balance: 0
  };

  rows: any[] = [];
  transactions: any[] = []; // All transactions with studentID
  savings: any[] = []; // Savings/Deposits data
  students: any[] = []; // Students data
  accountNumberInput: string = ''; // Input field for account number

  /** POST Students/names-ids (paged) — suggestions from server */
  filteredStudentsForPicker: StudentNameIdDTO[] = [];
  selectedStudentForSearch: StudentNameIdDTO | null = null;
  private readonly studentPickerPageSize = 5;
  /** Wait this many ms after typing stops before calling the search API (PrimeNG AutoComplete `delay`). */
  readonly studentPickerSearchDelayMs = 400;
  /** Skip one Enter keyup after list selection (PrimeNG handles selection on keydown). */
  private skipStudentSearchKeyUpAfterSelect = false;

  /** Pagination for student picker (same filters as last search). */
  private studentPickerLastQuery: Pick<StudentNameIdSearchRequest, 'studentID' | 'fullName'> | null = null;
  private studentPickerLoadedPage = 0;
  private studentPickerTotalPages = 0;
  private studentPickerLoadingMore = false;
  private studentPickerRequestSeq = 0;
  /** One auto "page 2" when the first page fills the panel and nothing scrolls yet (first stays 0). */
  private studentPickerNoScrollAppendDone = false;

  /** Shown under the report header (from API school/report or editable in DB template via #HeaderMessage#) */
  headerMessage = '';

  /** Resolved logo for [src] when API returns a path relative to the API host */
  get displayLogoUrl(): string {
    return this.resolveSchoolLogoUrl(this.logo);
  }

  ngOnInit(): void {
    // Get account ID from route params or query params
    this.route.params.subscribe(params => {
      const accountId = params['accountId'] || this.route.snapshot.queryParams['accountId'];
      if (accountId) {
        this.accountNumberInput = accountId.toString();
        this.loadAccountData(parseInt(accountId));
      } else {
        // Use default data for demo
        this.loadDefaultData();
      }
    });
  }

  /**
   * Load account report data by account number
   */
  loadAccountByNumber(): void {
    const accountId = parseInt(this.accountNumberInput);
    if (isNaN(accountId) || accountId <= 0) {
      this.toastr.warning('يرجى إدخال رقم حساب صحيح', 'تحذير');
      return;
    }
    this.selectedStudentForSearch = null;
    this.loadAccountData(accountId);
  }

  onStudentPickerSearch(event: AutoCompleteCompleteEvent): void {
    const raw = (event.query || '').trim();
    const req: StudentNameIdSearchRequest = {
      pageNumber: 1,
      pageSize: this.studentPickerPageSize
    };
    if (/^\d+$/.test(raw)) {
      req.studentID = parseInt(raw, 10);
    } else if (raw.length > 0) {
      req.fullName = raw;
    }

    const seq = ++this.studentPickerRequestSeq;
    this.studentPickerLoadingMore = false;
    this.studentPickerLoadedPage = 0;
    this.studentPickerTotalPages = 0;
    this.studentPickerLastQuery = null;
    this.studentPickerNoScrollAppendDone = false;

    this.studentService.searchStudentNamesAndIds(req).subscribe({
      next: (page) => {
        if (seq !== this.studentPickerRequestSeq) {
          return;
        }
        this.filteredStudentsForPicker = page.data;
        this.studentPickerLoadedPage = page.pageNumber;
        this.studentPickerTotalPages = Math.max(1, page.totalPages);
        this.studentPickerLastQuery =
          req.studentID != null && req.studentID > 0
            ? { studentID: req.studentID }
            : req.fullName != null && String(req.fullName).trim() !== ''
              ? { fullName: String(req.fullName).trim() }
              : null;
      },
      error: () => {
        if (seq !== this.studentPickerRequestSeq) {
          return;
        }
        this.filteredStudentsForPicker = [];
        this.studentPickerLastQuery = null;
        this.toastr.error('تعذّر البحث عن الطلاب', 'خطأ');
      }
    });
  }

  onStudentPickerLazyLoad(event: AutoCompleteLazyLoadEvent): void {
    const len = this.filteredStudentsForPicker.length;
    if (len === 0 || this.studentPickerLoadingMore || !this.studentPickerLastQuery) {
      return;
    }
    if (this.studentPickerLoadedPage >= this.studentPickerTotalPages) {
      return;
    }

    const first = Number(event.first);
    const last = Number(event.last);
    if (!Number.isFinite(first) || !Number.isFinite(last)) {
      return;
    }

    const nearEndOfLoaded = last >= len - 1;
    if (!nearEndOfLoaded) {
      return;
    }

    const scrolledAwayFromTop = first > 0;
    const canAppendWithoutScroll =
      first === 0 &&
      len === this.studentPickerPageSize &&
      this.studentPickerLoadedPage < this.studentPickerTotalPages &&
      !this.studentPickerNoScrollAppendDone;

    if (!scrolledAwayFromTop && !canAppendWithoutScroll) {
      return;
    }

    if (canAppendWithoutScroll && !scrolledAwayFromTop) {
      this.studentPickerNoScrollAppendDone = true;
    }

    this.loadStudentPickerNextPage();
  }

  private loadStudentPickerNextPage(): void {
    if (
      !this.studentPickerLastQuery ||
      this.studentPickerLoadingMore ||
      this.studentPickerLoadedPage >= this.studentPickerTotalPages
    ) {
      return;
    }

    const seq = this.studentPickerRequestSeq;
    const nextPage = this.studentPickerLoadedPage + 1;
    this.studentPickerLoadingMore = true;

    const req: StudentNameIdSearchRequest = {
      ...this.studentPickerLastQuery,
      pageNumber: nextPage,
      pageSize: this.studentPickerPageSize
    };

    this.studentService.searchStudentNamesAndIds(req).subscribe({
      next: (page) => {
        this.studentPickerLoadingMore = false;
        if (seq !== this.studentPickerRequestSeq) {
          return;
        }
        const existing = new Set(this.filteredStudentsForPicker.map((s) => s.studentID));
        const merged = [...this.filteredStudentsForPicker];
        for (const row of page.data) {
          if (!existing.has(row.studentID)) {
            merged.push(row);
            existing.add(row.studentID);
          }
        }
        this.filteredStudentsForPicker = merged;
        this.studentPickerLoadedPage = page.pageNumber;
        this.studentPickerTotalPages = Math.max(this.studentPickerTotalPages, Math.max(1, page.totalPages));
      },
      error: () => {
        this.studentPickerLoadingMore = false;
        if (seq === this.studentPickerRequestSeq) {
          this.toastr.error('تعذّر تحميل المزيد من الطلاب', 'خطأ');
        }
      }
    });
  }

  onStudentPickerClear(): void {
    this.filteredStudentsForPicker = [];
    this.studentPickerLastQuery = null;
    this.studentPickerLoadedPage = 0;
    this.studentPickerTotalPages = 0;
    this.studentPickerLoadingMore = false;
    this.studentPickerNoScrollAppendDone = false;
    this.studentPickerRequestSeq++;
  }

  onStudentSearchKeyUp(event: KeyboardEvent): void {
    if (event.key !== 'Enter' && event.key !== 'NumpadEnter') {
      return;
    }
    if (this.skipStudentSearchKeyUpAfterSelect) {
      this.skipStudentSearchKeyUpAfterSelect = false;
      return;
    }
    const ac = this.studentAccountAutocomplete;
    if (!ac) {
      return;
    }
    const raw = ((event.target as HTMLInputElement)?.value ?? '').trim();
    if (!raw.length) {
      this.filteredStudentsForPicker = [];
      return;
    }
    ac.search(event, raw, 'input');
  }

  onStudentPickerSelect(event: AutoCompleteSelectEvent): void {
    const row = event.value as StudentNameIdDTO;
    if (!row?.studentID) {
      return;
    }
    this.skipStudentSearchKeyUpAfterSelect = true;
    this.loadAccountByStudentId(row.studentID);
  }

  /** Resolve guardian account from student via Accounts/studentAndAccountNames + accountStudentGuardianId */
  private loadAccountByStudentId(studentId: number): void {
    this.accountService.getAccountAndStudentNames().subscribe({
      next: (res) => {
        if (!res.isSuccess || !res.result) {
          this.toastr.warning('تعذّر ربط الطالب بحساب', 'تحذير');
          return;
        }
        const links = res.result as StudentAccounts[];
        const matches = links.filter((l) => l.studentID === studentId);
        if (!matches.length) {
          this.toastr.warning('لا يوجد حساب مرتبط بهذا الطالب', 'تحذير');
          return;
        }
        const asgId = matches[0].accountStudentGuardianID;
        if (asgId == null) {
          this.toastr.warning('بيانات ربط الحساب غير مكتملة', 'تحذير');
          return;
        }
        this.accountService.getAccountIdByAccountStudentGuardianId(asgId).subscribe({
          next: (r2) => {
            if (!r2.isSuccess || r2.result == null) {
              const msg =
                (r2.errorMasseges && r2.errorMasseges[0]) || 'تعذّر تحديد رقم الحساب';
              this.toastr.error(msg, 'خطأ');
              return;
            }
            const accountId = Number(r2.result);
            if (Number.isNaN(accountId) || accountId <= 0) {
              this.toastr.error('رقم حساب غير صالح', 'خطأ');
              return;
            }
            this.accountNumberInput = String(accountId);
            this.loadAccountData(accountId);
          },
          error: () => this.toastr.error('تعذّر تحديد رقم الحساب', 'خطأ')
        });
      },
      error: () => this.toastr.error('تعذّر تحميل بيانات الحسابات', 'خطأ')
    });
  }

  loadAccountData(accountId: number): void {
    this.isLoading = true;
    
    // Load account report data from database (includes transactions and savings)
    this.accountService.getAccountReport(accountId).subscribe({
      next: (response: any) => {
        if (response.isSuccess && response.result) {
          const report = response.result;
          
          // Populate school info from database
          if (report.schoolInfo) {
            this.schoolName = report.schoolInfo.schoolName || '';
            this.schoolAddress = report.schoolInfo.schoolAddress || 'شملان | مديرية معين | صنعاء';
            this.schoolPhone = report.schoolInfo.schoolPhone || '01‑xxxxxxx';
            this.logo = report.schoolInfo.schoolLogo || '';
            this.academicYear = report.schoolInfo.academicYear || '';
            this.headerMessage = this.extractReportHeaderMessage(report, report.schoolInfo);
          } else {
            // Fallback to localStorage if schoolInfo not available
            this.logo = localStorage.getItem('SchoolImageURL') || '';
            this.schoolName = localStorage.getItem('schoolName') || '';
            this.schoolAddress = localStorage.getItem('schoolAddress') || 'شملان | مديرية معين | صنعاء';
            this.schoolPhone = localStorage.getItem('schoolPhone') || '01‑xxxxxxx';
            this.academicYear = '';
            this.headerMessage = this.extractReportHeaderMessage(report, null);
          }

          // Populate header data
          this.header = {
            accountNo: report.accountID?.toString() || '',
            guardian: report.accountName || '',
            createdDate: this.formatDate(report.hireDate?.toString() || ''),
            totalDebit: report.totalDebit || 0,
            totalCredit: report.totalCredit || 0,
            balance: report.balance || 0
          };
          
          // Store all transactions with studentID (normalize type for API casing / numeric enums)
          this.transactions = report.transactions.map((t: any) => {
            const type = this.normalizeTransactionType(t.type);
            return {
              id: t.id,
              desc: t.description,
              type,
              date: this.formatDate(t.date.toString()),
              amount: t.amount,
              studentID: t.studentID,
              discount: this.isCreditType(type) ? t.amount : 0,
              required: this.isDebitType(type) ? t.amount : 0
            };
          });
          
          // Keep rows for backward compatibility (all transactions)
          this.rows = this.transactions;
          
          // Convert savings to savings format
          this.savings = report.savings.map((s: any) => ({
            id: s.id,
            description: s.description,
            type: s.type,
            amount: s.amount,
            date: this.formatDate(s.date.toString())
          }));
          
          // Convert students to students format
          this.students = report.students?.map((s: any) => ({
            studentID: s.studentID,
            studentName: s.studentName,
            divisionName: s.divisionName || '',
            className: s.className || '',
            stageName: s.stageName || ''
          })) || [];
          
          // Load template and process it
          this.loadTemplate();
        } else {
          this.isLoading = false;
          const errorMessage = (response as any).errorMasseges && (response as any).errorMasseges.length > 0 
            ? (response as any).errorMasseges[0] 
            : 'لم يتم العثور على الحساب';
          this.toastr.error(errorMessage, 'خطأ');
        }
      },
      error: (error: any) => {
        console.error('Error loading account report:', error);
        this.isLoading = false;
        
        // Extract error message from response
        let errorMessage = 'حدث خطأ أثناء تحميل بيانات الحساب';
        if (error?.error) {
          if (error.error.errorMasseges && Array.isArray(error.error.errorMasseges) && error.error.errorMasseges.length > 0) {
            errorMessage = error.error.errorMasseges[0];
          } else if (error.error.message) {
            errorMessage = error.error.message;
          } else if (typeof error.error === 'string') {
            errorMessage = error.error;
          }
        } else if (error?.message) {
          errorMessage = error.message;
        }
        
        this.toastr.error(errorMessage, 'خطأ');
      }
    });
  }

  loadDefaultData(): void {
    // Default data for demo
    this.header = {
    accountNo: '123456789',
    guardian: 'ولي الأمر: أحمد محمد',
    createdDate: '2024‑09‑01',
    totalDebit: 300_000,
    totalCredit: 120_000,
    balance: -180_000
  };

    this.rows = [
    { id: 1, desc: 'رسوم دراسية', type: 'Debit', date: '2024‑09‑05', amount: 200_000, discount: 0, required: 200_000 },
    { id: 2, desc: 'خصم أخوة', type: 'Credit', date: '2024‑09‑10', amount: 40_000, discount: 40_000, required: 0 },
    { id: 3, desc: 'رسوم مواصلات', type: 'Debit', date: '2024‑09‑12', amount: 100_000, discount: 0, required: 100_000 },
  ];
  
    // Default savings/deposits data
    this.savings = [
      { id: 1, amount: 50_000, date: '2024‑09‑28', type: 'true', description: 'قسط اول' },
    ];
    
    // Default students data
    this.students = [
      { studentID: 1, studentName: 'أحمد محمد علي', className: 'أول', stageName: 'اساسي' },
    ];

    this.headerMessage = 'ملاحظة: يرجى مراجعة الرصيد. (نص تجريبي — يُستبدل من الخادم عند ربط الحقل reportHeaderMessage)';
    
    this.loadTemplate();
  }

  loadTemplate(): void {
    const schoolIdStr = localStorage.getItem('schoolId');
    const schoolId = schoolIdStr ? parseInt(schoolIdStr, 10) : undefined;

    this.reportTemplateService.getTemplateByCode('ACCOUNT_REPORT', schoolId).subscribe({
      next: (template) => {
        this.templateHtml = template.templateHtml || '';
        this.processTemplate();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading template:', error);
        // Use default HTML if template not found
        this.templateHtml = this.getDefaultTemplate();
        this.processTemplate();
        this.isLoading = false;
      }
    });
  }

  processTemplate(): void {
    const source =
      this.templateHtml && this.templateHtml.trim().length > 0
        ? this.templateHtml
        : this.getDefaultTemplate();

    let processed = source;

    // Replace placeholders with actual data
    processed = processed.replace(/#AccountNo#/g, this.header.accountNo);
    processed = processed.replace(/#Guardian#/g, this.header.guardian);
    processed = processed.replace(/#CreatedDate#/g, this.header.createdDate);
    processed = processed.replace(/#TotalDebit#/g, this.formatNumber(this.header.totalDebit));
    processed = processed.replace(/#TotalCredit#/g, this.formatNumber(this.header.totalCredit));
    processed = processed.replace(/#Balance#/g, this.formatNumber(this.header.balance));
    processed = processed.replace(/#SchoolName#/g, this.schoolName || '');
    processed = processed.replace(/#SchoolAddress#/g, this.schoolAddress);
    processed = processed.replace(/#SchoolPhone#/g, this.schoolPhone);
    const logoUrl = this.resolveSchoolLogoUrl(this.logo);
    processed = processed.replace(/#SchoolLogo#/g, logoUrl ? this.escapeHtmlAttr(logoUrl) : '');
    processed = processed.replace(/<img\b[^>]*\ssrc=""[^>]*>/gi, '');
    processed = processed.replace(/#SchoolYear#/g, this.academicYear || '');
    processed = processed.replace(/#HeaderMessage#/g, this.formatHeaderMessagePlain());
    processed = processed.replace(/#HeaderMessageBlock#/g, this.buildHeaderMessageBlockHtml());

    // Process students information
    const studentsHtml = this.generateStudentsHtml();
    processed = processed.replace(/#StudentsInfo#/g, studentsHtml);
    
    // Process rows/transactions - generate table rows
    const rowsHtml = this.generateRowsHtml();
    processed = processed.replace(/#TransactionsTable#/g, rowsHtml);
    
    // Process savings/deposits table
    const savingsHtml = this.generateSavingsHtml();
    processed = processed.replace(/#SavingsTable#/g, savingsHtml);
    
    // Calculate total savings
    const totalSavings = this.savings.reduce((sum, s) => sum + (s.amount || 0), 0);
    processed = processed.replace(/#TotalSavings#/g, this.formatNumber(totalSavings));

    processed = this.stripEmptyQuillHeadings(processed);

    this.processedHtml = processed;
  }

  /**
   * Removes empty Quill headings (e.g. `<h2 class="ql-direction-rtl ql-align-center"> </h2><br>`)
   * that reserve vertical space in DB-stored report templates.
   */
  private stripEmptyQuillHeadings(html: string): string {
    const emptyHeadingWithOptionalBr =
      /<h([1-6])\b[^>]*>(?:(?:\s|\u00A0|&nbsp;|<br\s*\/?>|<\/?span\b[^>]*>)*)<\/h\1>\s*(?:<br\s*\/?>)?/gi;
    let out = html;
    let prev = '';
    while (out !== prev) {
      prev = out;
      out = out.replace(emptyHeadingWithOptionalBr, '');
    }
    return out.replace(/(?:<br\s*\/?>\s*){2,}/gi, '<br>');
  }

  generateRowsHtml(): string {
    if (!this.rows || this.rows.length === 0) {
      return '<tr><td colspan="5" class="text-center p-2 border">لا توجد معاملات</td></tr>';
    }

    return this.rows.map((row, index) => `
      <tr class="${index % 2 === 0 ? 'bg-gray-50' : ''}">
        <td class="p-2 border">${index + 1}</td>
        <td class="p-2 border">${row.desc}</td>
        <td class="p-2 border">${this.transactionTypeLabel(row.type)}</td>
        <td class="p-2 border">YR ${this.formatNumber(row.amount)}</td>
        <td class="p-2 border">${row.date}</td>
      </tr>
    `).join('');
  }

  generateSavingsHtml(): string {
    if (!this.savings || this.savings.length === 0) {
      return '<tr><td colspan="5" class="text-center p-2 border">لا توجد مدخرات</td></tr>';
    }

    return this.savings.map((saving, index) => `
      <tr class="${index % 2 === 0 ? 'bg-gray-50' : ''}">
        <td class="p-2 border">${index + 1}</td>
        <td class="p-2 border">${saving.description || ''}</td>
        <td class="p-2 border">${saving.type === 'true' || saving.type === true ? 'مدخر' : 'سحب'}</td>
        <td class="p-2 border">YR ${this.formatNumber(saving.amount)}</td>
        <td class="p-2 border">${saving.date}</td>
      </tr>
    `).join('');
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);
  }

  formatDate(date: string | Date): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }

  /** Arabic column "النوع": debit = charge, credit = discount/payment */
  transactionTypeLabel(type: unknown): string {
    if (this.isDebitType(type)) return 'مديونية';
    if (this.isCreditType(type)) return 'خصم';
    const raw = String(type ?? '').trim();
    return raw || '—';
  }

  private normalizeTransactionType(type: unknown): string {
    const raw = String(type ?? '').trim();
    const t = raw.toLowerCase();
    if (t === 'debit' || t === 'd' || t === '0' || raw === 'مدين') return 'Debit';
    if (t === 'credit' || t === 'c' || t === '1' || raw === 'دائن') return 'Credit';
    return raw;
  }

  private isDebitType(type: unknown): boolean {
    return this.normalizeTransactionType(type) === 'Debit';
  }

  private isCreditType(type: unknown): boolean {
    return this.normalizeTransactionType(type) === 'Credit';
  }

  private escapeHtmlAttr(value: string): string {
    return value.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
  }

  private escapeHtmlText(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }

  /** API: report.reportHeaderMessage | report.reportMessage | schoolInfo.reportHeaderMessage */
  private extractReportHeaderMessage(report: any, schoolInfo: any): string {
    const raw =
      report?.reportHeaderMessage ??
      report?.reportMessage ??
      schoolInfo?.reportHeaderMessage ??
      '';
    return String(raw).trim();
  }

  /** Plain escaped text for custom templates (#HeaderMessage#) */
  private formatHeaderMessagePlain(): string {
    const t = this.headerMessage.trim();
    return t ? this.escapeHtmlText(t) : '';
  }

  /** Box under header for default template (#HeaderMessageBlock#); empty if no message */
  private buildHeaderMessageBlockHtml(): string {
    const t = this.headerMessage.trim();
    if (!t) return '';
    const inner = this.escapeHtmlText(t).replace(/\r\n|\r|\n/g, '<br/>');
    return `<div class="account-report-header-message" role="note">${inner}</div>`;
  }

  /** Build absolute URL for logos stored as API-relative paths */
  private resolveSchoolLogoUrl(src: string | null | undefined): string {
    const s = (src ?? '').trim();
    if (!s) return '';
    if (/^(https?:\/\/|data:|blob:)/i.test(s)) return s;
    if (s.startsWith('//')) return `${window.location.protocol}${s}`;
    const apiRoot = environment.baseUrl.replace(/\/api\/?$/i, '');
    if (s.startsWith('/')) return `${apiRoot}${s}`;
    return `${apiRoot}/${s.replace(/^\/+/, '')}`;
  }

  getTotalSavings(): number {
    return this.savings.reduce((sum, s) => sum + (s.amount || 0), 0);
  }

  generateStudentsHtml(): string {
    if (!this.students || this.students.length === 0) {
      return '';
    }

    return this.students.map((student) => {
      const gradeInfo = [];
      if (student.className) gradeInfo.push(student.className);
      if (student.stageName) gradeInfo.push(student.stageName);
      const gradeText = gradeInfo.length > 0 ? gradeInfo.join(' / ') : '';
      
      // Get transactions for this student
      const studentTransactions = this.getTransactionsForStudent(student.studentID);
      const transactionsHtml = this.generateStudentTransactionsHtml(studentTransactions);
      
      return `
        <div style="margin-top: 10px;">
          <h4 style="font-size: 1.1em; margin-bottom: 5px; font-weight: bold;">${student.studentName}</h4>
          ${gradeText ? `<p style="color: #666; margin: 0;">الصف: ${gradeText}</p>` : ''}
          ${transactionsHtml}
        </div>
      `;
    }).join('');
  }

  getTransactionsForStudent(studentID: number): any[] {
    return this.transactions.filter(t => t.studentID === studentID);
  }

  getStudentTotalDebit(studentID: number): number {
    return this.getTransactionsForStudent(studentID)
      .filter(t => this.isDebitType(t.type))
      .reduce((sum, t) => sum + (t.amount || 0), 0);
  }

  generateStudentTransactionsHtml(transactions: any[]): string {
    if (!transactions || transactions.length === 0) {
      return '';
    }

    const rowsHtml = transactions.map((row, index) => `
      <tr class="${index % 2 === 0 ? 'bg-gray-50' : ''}">
        <td class="p-2 border">${index + 1}</td>
        <td class="p-2 border">${row.desc}</td>
        <td class="p-2 border">${this.transactionTypeLabel(row.type)}</td>
        <td class="p-2 border">YR ${this.formatNumber(row.amount)}</td>
        <td class="p-2 border">${row.date}</td>
      </tr>
    `).join('');

    const totalDebit = transactions
      .filter(t => this.isDebitType(t.type))
      .reduce((sum, t) => sum + (t.amount || 0), 0);

    return `
      <div style="margin-top: 10px; margin-bottom: 20px;">
        <table style="width:100%; border-collapse:collapse; margin-top:10px;" border="1">
          <thead style="background-color:#f3f4f6;">
            <tr>
              <th style="padding:8px; border:1px solid #ddd; text-align:center;">#</th>
              <th style="padding:8px; border:1px solid #ddd; text-align:center;">البند</th>
              <th style="padding:8px; border:1px solid #ddd; text-align:center;">النوع</th>
              <th style="padding:8px; border:1px solid #ddd; text-align:center;">المبلغ</th>
              <th style="padding:8px; border:1px solid #ddd; text-align:center;">التاريخ</th>
            </tr>
          </thead>
          <tbody>
            ${rowsHtml}
            ${totalDebit > 0 ? `
              <tr style="font-weight:bold; background-color:#fef3c7;">
                <td colspan="4" style="text-align:right; padding:8px; border:1px solid #ddd;">إجمالي المديونية:</td>
                <td style="padding:8px; border:1px solid #ddd;">YR ${this.formatNumber(totalDebit)}</td>
              </tr>
            ` : ''}
          </tbody>
        </table>
      </div>
    `;
  }

  getDefaultTemplate(): string {
    return `
      <div dir="rtl" style="padding:20px; line-height:1.8; font-family:Arial;">
        <div style="display:flex; justify-content:space-between; margin-bottom:20px; border-bottom:2px solid #ccc; padding-bottom:20px;">
          <div style="flex:1; text-align:right;">
            <h2 style="font-size:1.5em; margin-bottom:10px;">رقم الحساب: #AccountNo#</h2>
            <h3 style="font-size:1.2em; margin-bottom:5px;">#Guardian#</h3>
            #StudentsInfo#
            <p style="color:#666; margin-bottom:10px;">تاريخ الإنشاء: <strong>#CreatedDate#</strong></p>
            
            <div style="margin-top:15px;">
              <div style="display:flex; justify-content:space-between; border-bottom:1px solid #ddd; padding:5px 0;">
                <span>إجمالي المديونية</span>
                <span>YR #TotalDebit#</span>
              </div>
              <div style="display:flex; justify-content:space-between; border-bottom:1px solid #ddd; padding:5px 0;">
                <span>إجمالي المدفوعات</span>
                <span>YR #TotalCredit#</span>
              </div>
              <div style="display:flex; justify-content:space-between; font-weight:bold; padding:5px 0; color:#f97316;">
                <span>الرصيد</span>
                <span>YR #Balance#</span>
              </div>
            </div>
          </div>
          
          <div style="min-width:200px; text-align:left;">
            <img src="#SchoolLogo#" alt="شعار المدرسة" style="height:48px; margin-bottom:10px;" />
            <div style="font-weight:bold; font-size:1.1em; margin-bottom:5px;">#SchoolName#</div>
            <div style="color:#666;">#SchoolAddress#</div>
            <div style="color:#666;">Tel: #SchoolPhone#</div>
          </div>
        </div>
        #HeaderMessageBlock#
        
        <div style="margin-top:20px;">
          <table style="width:100%; border-collapse:collapse; margin-top:20px;" border="1">
            <thead style="background-color:#f3f4f6;">
              <tr>
                <th style="padding:8px; border:1px solid #ddd; text-align:center;">#</th>
                <th style="padding:8px; border:1px solid #ddd; text-align:center;">البند</th>
                <th style="padding:8px; border:1px solid #ddd; text-align:center;">النوع</th>
                <th style="padding:8px; border:1px solid #ddd; text-align:center;">المبلغ</th>
                <th style="padding:8px; border:1px solid #ddd; text-align:center;">التاريخ</th>
              </tr>
            </thead>
            <tbody>
              #TransactionsTable#
              <tr style="font-weight:bold; background-color:#fef3c7;">
                <td colspan="4" style="text-align:right; padding:8px; border:1px solid #ddd;">إجمالي المديونية:</td>
                <td style="padding:8px; border:1px solid #ddd;">YR #TotalDebit#</td>
              </tr>
            </tbody>
          </table>
        </div>
        
        <!-- Savings/Deposits Table -->
        <div style="margin-top:30px;">
          <h3 style="margin-bottom:10px; font-size:1.2em;">المدخرات:</h3>
          <table style="width:100%; border-collapse:collapse; margin-top:10px;" border="1">
            <thead style="background-color:#f3f4f6;">
              <tr>
                <th style="padding:8px; border:1px solid #ddd; text-align:center;">#</th>
                <th style="padding:8px; border:1px solid #ddd; text-align:center;">الوصف</th>
                <th style="padding:8px; border:1px solid #ddd; text-align:center;">النوع</th>
                <th style="padding:8px; border:1px solid #ddd; text-align:center;">المبلغ</th>
                <th style="padding:8px; border:1px solid #ddd; text-align:center;">تاريخ الإنشاء</th>
              </tr>
            </thead>
            <tbody>
              #SavingsTable#
              <tr style="font-weight:bold; background-color:#dbeafe;">
                <td colspan="4" style="text-align:right; padding:8px; border:1px solid #ddd;">إجمالي المدخرات:</td>
                <td style="padding:8px; border:1px solid #ddd;">YR #TotalSavings#</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    `;
  }

  /* لو أردت معاينة مباشرة عبر window.print(): */
  ngAfterViewInit(): void {
    // uncomment to auto‑open print preview when component loads
    // setTimeout(() => this.nativePrint(), 0);
  }

  nativePrint(): void {
    // Check if we have data to print
    if (!this.header.accountNo && !this.processedHtml) {
      this.toastr.warning('لا توجد بيانات للطباعة. يرجى تحميل بيانات الحساب أولاً.', 'تحذير');
      return;
    }

    if (!document.getElementById('report')) {
      this.toastr.error('لم يتم العثور على محتوى الطباعة', 'خطأ');
      return;
    }

    // Print the current document (not a popup). Popup + document.write often stays blank in Chrome
    // ("about:blank") because stylesheets load async or cloned <style> breaks the document.
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        window.print();
      });
    });
  }
}
