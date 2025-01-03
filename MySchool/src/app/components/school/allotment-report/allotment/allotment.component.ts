import {
  Component,
  OnInit,
  ViewChild,
  ComponentRef,
  ViewContainerRef,
  Injector,
  Type
} from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';

import { StudentMonthResultComponent } from '../../report/student-month-result/student-month-result.component';


interface ReportOption {
  label: string;
  value: Type<any>; // The component type
}

@Component({
  selector: 'app-allotment',
  templateUrl: './allotment.component.html',
  styleUrls: ['./allotment.component.scss']
})
export class AllotmentComponent implements OnInit {
  @ViewChild(StudentMonthResultComponent, { read: ViewContainerRef }) reportContainer!: ViewContainerRef;

  formGroup: FormGroup;
  selectedReportType: any;   // The user’s chosen component class

  // Dropdown options for the reports
  reportOptions: ReportOption[] = [
    { label: 'Student Month Result', value: StudentMonthResultComponent },
    // { label: 'Receipt Voucher',      value: ReceiptVoucherComponent },
    // { label: 'Registration Form',    value: RegistrationFormComponent }
  ];

  constructor(
    private injector: Injector
  ) {
    this.formGroup = new FormGroup({
      text: new FormControl('')
    });
  }

  ngOnInit() { }

  onReportSelect(): void {
    if (!this.reportContainer) return;

    // Clear any previously rendered component
    this.reportContainer.clear();

    if (!this.selectedReportType) {
      // Clear the editor text if no report is selected
      this.formGroup.patchValue({ text: '' });
      return;
    }

    // Dynamically create the chosen report component
    const componentRef: ComponentRef<any> = this.reportContainer.createComponent(
      this.selectedReportType,
      { injector: this.injector }
    );

    // Retrieve content from the dynamically loaded component
    const content = componentRef.instance.getContent();
    this.formGroup.patchValue({ text: content });
  }

}
