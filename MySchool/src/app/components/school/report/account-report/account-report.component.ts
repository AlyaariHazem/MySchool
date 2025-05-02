import { Component, ElementRef, ViewChild } from '@angular/core';
import html2pdf, { Html2PdfOptions } from 'html2pdf.js';


@Component({
  selector: 'app-account-report',
  templateUrl: './account-report.component.html',
  styleUrl: './account-report.component.scss'
})
export class AccountReportComponent {
  /* يرتبط بالقسم الذى نريد طباعته فقط */
  @ViewChild('printArea', { static: true })
  printArea!: ElementRef<HTMLDivElement>;

  logo = localStorage.getItem('SchoolImageURL');
  schoolName = localStorage.getItem('schoolName');
  header = {
    accountNo: '123456789',
    guardian: 'ولي الأمر: أحمد محمد',
    createdDate: '2024‑09‑01',
    totalDebit: 300_000,
    totalCredit: 120_000,
    balance: -180_000
  };

  rows = [
    { id: 1, desc: 'رسوم دراسية', type: 'Debit', date: '2024‑09‑05', amount: 200_000, discount: 0, required: 200_000 },
    { id: 2, desc: 'خصم أخوة', type: 'Credit', date: '2024‑09‑10', amount: 40_000, discount: 40_000, required: 0 },
    { id: 3, desc: 'رسوم مواصلات', type: 'Debit', date: '2024‑09‑12', amount: 100_000, discount: 0, required: 100_000 },
  ];
  

  /* ---------- زر "طباعة" ---------- */
  print(): void {
    const opt: Html2PdfOptions = {
      margin: 10,
      filename: `account-${this.header.accountNo}.pdf`,
      image: { type: 'jpeg', quality: 0.98 },
      html2canvas: { scale: 2, useCORS: true },
      jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' as 'portrait' }
    };

    /* html2pdf تطبع فقط محتوى printArea */
    html2pdf().set(opt).from(this.printArea.nativeElement).save();
  }

  /* لو أردت معاينة مباشرة عبر window.print(): */
  ngAfterViewInit(): void {
    // uncomment to auto‑open print preview when component loads
    // setTimeout(() => this.nativePrint(), 0);
  }

  nativePrint(): void {
    const page = document.getElementById('page');
    if (!page) { return; }

    /* ️نسخ كل ملفات الأنماط الموجودة */
    const links = Array.from(document.querySelectorAll('link[rel="stylesheet"], style'))
      .filter((el: Element) => el.getAttribute('href') !== 'assets/print.css')
      .map(el => el.outerHTML)
      .join('');
 
      const base = `<base href="${document.baseURI}">`;

    const popup = window.open('', '', 'width=1000px,height=auto');
    if (!popup) { return; }

    popup.document.write(`
      <html><head>
      <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;700&display=swap" rel="stylesheet">
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
