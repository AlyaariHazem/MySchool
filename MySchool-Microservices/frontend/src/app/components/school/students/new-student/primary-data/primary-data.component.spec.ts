import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrimaryDataComponent } from './primary-data.component';

describe('PrimaryDataComponent', () => {
  let component: PrimaryDataComponent;
  let fixture: ComponentFixture<PrimaryDataComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PrimaryDataComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PrimaryDataComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
