import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudyYearComponent } from './study-year.component';

describe('StudyYearComponent', () => {
  let component: StudyYearComponent;
  let fixture: ComponentFixture<StudyYearComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudyYearComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(StudyYearComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
