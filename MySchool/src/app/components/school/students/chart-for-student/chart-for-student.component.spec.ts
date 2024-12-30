import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChartForStudentComponent } from './chart-for-student.component';

describe('ChartForStudentComponent', () => {
  let component: ChartForStudentComponent;
  let fixture: ComponentFixture<ChartForStudentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChartForStudentComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ChartForStudentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
