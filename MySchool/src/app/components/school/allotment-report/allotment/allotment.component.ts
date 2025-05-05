import {Component, ViewChild, ViewContainerRef,Type, ElementRef} from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';

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
    { label: 'Receipt Voucher', value: StudentMonthResultComponent },
    { label: 'Registration Form', value: StudentMonthResultComponent }
  ];

}
