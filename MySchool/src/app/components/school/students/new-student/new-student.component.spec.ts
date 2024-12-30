import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewStudentComponent } from './new-student.component';

describe('NewStudentComponent', () => {
  let component: NewStudentComponent;
  let fixture: ComponentFixture<NewStudentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewStudentComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(NewStudentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
