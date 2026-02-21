import { Component, ElementRef, ViewChild, OnInit, inject } from '@angular/core';
import { ReportTemplateService } from '../../core/services/report-template.service';
import { AccountService } from '../../core/services/account.service';
import { ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-account-report',
  templateUrl: './account-report.component.html',
  styleUrl: './account-report.component.scss'
})
export class AccountReportComponent implements OnInit {
  /* يرتبط بالقسم الذى نريد طباعته فقط */
  @ViewChild('printArea', { static: true })
  printArea!: ElementRef<HTMLDivElement>;

  // Services
  private reportTemplateService = inject(ReportTemplateService);
  private accountService = inject(AccountService);
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
  savings: any[] = []; // Savings/Deposits data
  accountNumberInput: string = ''; // Input field for account number

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
    this.loadAccountData(accountId);
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
          } else {
            // Fallback to localStorage if schoolInfo not available
            this.logo = localStorage.getItem('SchoolImageURL') || '';
            this.schoolName = localStorage.getItem('schoolName') || '';
            this.schoolAddress = localStorage.getItem('schoolAddress') || 'شملان | مديرية معين | صنعاء';
            this.schoolPhone = localStorage.getItem('schoolPhone') || '01‑xxxxxxx';
            this.academicYear = '';
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
          
          // Convert transactions to rows format
          this.rows = report.transactions.map((t: any) => ({
            id: t.id,
            desc: t.description,
            type: t.type,
            date: this.formatDate(t.date.toString()),
            amount: t.amount,
            discount: t.type === 'Credit' ? t.amount : 0,
            required: t.type === 'Debit' ? t.amount : 0
          }));
          
          // Convert savings to savings format
          this.savings = report.savings.map((s: any) => ({
            id: s.id,
            description: s.description,
            type: s.type,
            amount: s.amount,
            date: this.formatDate(s.date.toString())
          }));
          
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
    if (!this.templateHtml) {
      this.processedHtml = this.getDefaultTemplate();
      return;
    }

    let processed = this.templateHtml;
    
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
    processed = processed.replace(/#SchoolLogo#/g, this.logo || '');
    processed = processed.replace(/#SchoolYear#/g, this.academicYear || '');
    
    // Process rows/transactions - generate table rows
    const rowsHtml = this.generateRowsHtml();
    processed = processed.replace(/#TransactionsTable#/g, rowsHtml);
    
    // Process savings/deposits table
    const savingsHtml = this.generateSavingsHtml();
    processed = processed.replace(/#SavingsTable#/g, savingsHtml);
    
    // Calculate total savings
    const totalSavings = this.savings.reduce((sum, s) => sum + (s.amount || 0), 0);
    processed = processed.replace(/#TotalSavings#/g, this.formatNumber(totalSavings));
    
    this.processedHtml = processed;
  }

  generateRowsHtml(): string {
    if (!this.rows || this.rows.length === 0) {
      return '<tr><td colspan="5" class="text-center p-2 border">لا توجد معاملات</td></tr>';
    }

    return this.rows.map((row, index) => `
      <tr class="${index % 2 === 0 ? 'bg-gray-50' : ''}">
        <td class="p-2 border">${index + 1}</td>
        <td class="p-2 border">${row.desc}</td>
        <td class="p-2 border">${row.type === 'Debit' ? '—' : 'خصم'}</td>
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

  getTotalSavings(): number {
    return this.savings.reduce((sum, s) => sum + (s.amount || 0), 0);
  }

  getDefaultTemplate(): string {
    return `
      <div dir="rtl" style="padding:20px; line-height:1.8; font-family:Arial;">
        <div style="display:flex; justify-content:space-between; margin-bottom:20px; border-bottom:2px solid #ccc; padding-bottom:20px;">
          <div style="flex:1; text-align:right;">
            <h2 style="font-size:1.5em; margin-bottom:10px;">رقم الحساب: #AccountNo#</h2>
            <h3 style="font-size:1.2em; margin-bottom:5px;">#Guardian#</h3>
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

    const page = document.getElementById('page');
    if (!page) { 
      this.toastr.error('لم يتم العثور على محتوى الطباعة', 'خطأ');
      return; 
    }

    /* ️نسخ كل ملفات الأنماط الموجودة */
    const links = Array.from(document.querySelectorAll('link[rel="stylesheet"], style'))
      .filter((el: Element) => el.getAttribute('href') !== 'assets/print.css')
      .map(el => el.outerHTML)
      .join('');
 
    const base = `<base href="${document.baseURI}">`;

    const popup = window.open('', '', 'width=1000px,height=auto');
    if (!popup) { 
      this.toastr.error('تم منع النافذة المنبثقة. يرجى السماح بالنوافذ المنبثقة للمتصفح.', 'خطأ');
      return; 
    }

    // Get the print area content (either processedHtml or the fallback template)
    let printContent = '';
    if (this.processedHtml) {
      // Use processed template HTML
      printContent = `
        <div dir="rtl" style="padding:20px; line-height:1.8; font-family:Arial;">
          ${this.processedHtml}
        </div>
      `;
    } else {
      // Use the fallback template from the page
      const printArea = this.printArea?.nativeElement;
      if (printArea) {
        printContent = printArea.innerHTML;
      } else {
        printContent = page.innerHTML;
      }
    }

    popup.document.write(`
      <html><head>
      <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;700&display=swap" rel="stylesheet">
        ${base}
        ${links}
        <style>
          @media print {
            body {
              margin: 0;
              direction: rtl;
              font-family: "Cairo", "Tahoma", sans-serif;
            }
            .report, * {
              letter-spacing: 0 !important;
            }
            .print\\:hidden {
              display: none !important;
            }
            table {
              border-collapse: collapse;
              width: 100%;
            }
            th, td {
              border: 1px solid #ddd;
              padding: 8px;
            }
            .bg-gray-50 {
              background-color: #f9fafb !important;
            }
            .bg-yellow-50 {
              background-color: #fef3c7 !important;
            }
            .bg-blue-50 {
              background-color: #dbeafe !important;
            }
          }
        </style>
      </head><body dir="rtl">
        ${printContent}
      </body></html>
    `);

    popup.document.close();
    
    // Wait for content to load, then print
    setTimeout(() => {
      popup.focus();
      popup.print();
    }, 250);
  }
}
