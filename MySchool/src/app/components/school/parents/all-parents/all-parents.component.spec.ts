import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AllParentsComponent } from './all-parents.component';

describe('AllParentsComponent', () => {
  let component: AllParentsComponent;
  let fixture: ComponentFixture<AllParentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AllParentsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AllParentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
