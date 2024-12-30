import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AllotmentReportComponent } from './allotment-report.component';

describe('AllotmentReportComponent', () => {
  let component: AllotmentReportComponent;
  let fixture: ComponentFixture<AllotmentReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AllotmentReportComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AllotmentReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
