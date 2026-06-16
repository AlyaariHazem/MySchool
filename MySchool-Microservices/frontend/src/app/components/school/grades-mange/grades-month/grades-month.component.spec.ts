import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GradesMonthComponent } from './grades-month.component';

describe('GradesMonthComponent', () => {
  let component: GradesMonthComponent;
  let fixture: ComponentFixture<GradesMonthComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GradesMonthComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GradesMonthComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
