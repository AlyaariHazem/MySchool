import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewYearComponent } from './new-year.component';

describe('NewYearComponent', () => {
  let component: NewYearComponent;
  let fixture: ComponentFixture<NewYearComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewYearComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NewYearComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
