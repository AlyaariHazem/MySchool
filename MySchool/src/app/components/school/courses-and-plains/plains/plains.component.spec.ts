import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlainsComponent } from './plains.component';

describe('PlainsComponent', () => {
  let component: PlainsComponent;
  let fixture: ComponentFixture<PlainsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlainsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlainsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
