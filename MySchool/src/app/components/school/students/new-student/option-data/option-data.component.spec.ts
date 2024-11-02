import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OptionDataComponent } from './option-data.component';

describe('OptionDataComponent', () => {
  let component: OptionDataComponent;
  let fixture: ComponentFixture<OptionDataComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OptionDataComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(OptionDataComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
