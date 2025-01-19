// src/app/components/school/report/report.component.ts

import { Component } from '@angular/core';
import jsPDF from 'jspdf';
import 'jspdf-autotable';

@Component({
  selector: 'app-report',
  template: `<button (click)="generatePDF()">Generate PDF</button>`,
  styleUrls: ['./report.component.scss']
})
export class ReportComponent {

  generatePDF(): void {
    const doc = new jsPDF();

    doc.setFontSize(18);
    doc.text('School Report', 14, 22);

    (doc as any).autoTable({
      startY: 30,
      head: [['Name', 'Grade', 'Score']],
      body: [
        ['Student A', '10', '90'],
        ['Student B', '10', '85'],
        ['Student C', '10', '78'],
      ],
      styles: { halign: 'center' }
    });

    doc.save('school-report.pdf');
  }
}
