import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AllotmentComponent } from './allotment.component';

describe('AllotmentComponent', () => {
  let component: AllotmentComponent;
  let fixture: ComponentFixture<AllotmentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AllotmentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AllotmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
