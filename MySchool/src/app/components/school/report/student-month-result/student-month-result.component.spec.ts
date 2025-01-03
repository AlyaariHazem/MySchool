import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentMonthResultComponent } from './student-month-result.component';

describe('StudentMonthResultComponent', () => {
  let component: StudentMonthResultComponent;
  let fixture: ComponentFixture<StudentMonthResultComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudentMonthResultComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentMonthResultComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
