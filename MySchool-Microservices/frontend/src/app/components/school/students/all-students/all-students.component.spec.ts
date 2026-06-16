import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AllStudentsComponent } from './all-students.component';

describe('AllStudentsComponent', () => {
  let component: AllStudentsComponent;
  let fixture: ComponentFixture<AllStudentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AllStudentsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AllStudentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
