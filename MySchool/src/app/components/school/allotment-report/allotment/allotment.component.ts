import {
  Component, ViewChild, ViewContainerRef, Injector,
  Type, ElementRef
} from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import html2pdf from 'html2pdf.js';
import type { Html2PdfOptions } from 'html2pdf.js';

import { StudentMonthResultComponent } from '../../report/student-month-result/student-month-result.component';

interface ReportOption { label: string; value: Type<any>; }

@Component({
  selector: 'app-allotment',
  templateUrl: './allotment.component.html',
  styleUrls: ['./allotment.component.scss']
})
export class AllotmentComponent {

  /* dynamic outlet */
  @ViewChild('reportContainer', { read: ViewContainerRef, static: true })
  reportContainer!: ViewContainerRef;

  /* element we convert to PDF */
  @ViewChild('editorPreview', { static: false })
  editorPreview!: ElementRef<HTMLDivElement>;

  /* rich‑text form */
  formGroup = new FormGroup({ text: new FormControl('') });
  currentReportHtml!: string; // declare the property

  /* dropdown */
  selectedReportType: Type<any> | null = null;
  reportOptions: ReportOption[] = [
    { label: 'Student Month Result', value: StudentMonthResultComponent },
    { label: 'Receipt Voucher',  value: StudentMonthResultComponent },
    { label: 'Registration Form', value: StudentMonthResultComponent }
  ];

  constructor(private injector: Injector) { }

  async onReportSelect(type: Type<any> | null): Promise<void> {
    this.selectedReportType = type;
    this.reportContainer.clear();
    this.formGroup.reset();
    this.currentReportHtml = '';

    if (!type) { return; }

    const cmpRef = this.reportContainer.createComponent(type, {
      injector: this.injector
    });

    /* Wait one micro‑task so Angular finishes rendering */
    await Promise.resolve();

    /* ------------------------------------------------------------------
       1) If the component exposes getContent() (your own helper) use it.
       2) Otherwise fall back to raw DOM innerHTML.
       ------------------------------------------------------------------ */
    const inst: any = cmpRef.instance;
    this.currentReportHtml = typeof inst.getContent === 'function'
      ? inst.getContent()
      : cmpRef.location.nativeElement.innerHTML;

    /* push the same HTML into p‑editor for the user to tweak */
    this.formGroup.get('text')!.setValue(this.currentReportHtml,
      { emitEvent: false });
  }


  exportPDF(): void {
    if (!this.currentReportHtml) { return; }

    /* Build a temporary off‑screen element */
    const tmp = document.createElement('div');
    tmp.innerHTML = this.currentReportHtml;
    document.body.appendChild(tmp);          // required by html2pdf

    const opt: Html2PdfOptions = {
      margin: 10,
      filename: 'student-registration.pdf',
      image: { type: 'pdf', quality: 0.98 },
      html2canvas: { scale: 2, useCORS: true },
      jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
    };

    html2pdf().set(opt).from(tmp).save().then(() => {
      document.body.removeChild(tmp);        // tidy up
    });
  }

}
