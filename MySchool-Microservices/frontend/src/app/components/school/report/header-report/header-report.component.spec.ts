import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HeaderReportComponent } from './header-report.component';

describe('HeaderReportComponent', () => {
  let component: HeaderReportComponent;
  let fixture: ComponentFixture<HeaderReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HeaderReportComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HeaderReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
