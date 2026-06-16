import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StagesGradesComponent } from './stages-grades.component';

describe('StagesGradesComponent', () => {
  let component: StagesGradesComponent;
  let fixture: ComponentFixture<StagesGradesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StagesGradesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(StagesGradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
