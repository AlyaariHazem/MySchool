import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FeeClassComponent } from './fee-class.component';

describe('FeeClassComponent', () => {
  let component: FeeClassComponent;
  let fixture: ComponentFixture<FeeClassComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeeClassComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FeeClassComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
