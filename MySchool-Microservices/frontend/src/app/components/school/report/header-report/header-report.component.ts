import { Component } from '@angular/core';

@Component({
  selector: 'app-header-report',
  templateUrl: './header-report.component.html',
  styleUrl: './header-report.component.scss'
})
export class HeaderReportComponent {

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
  
}
