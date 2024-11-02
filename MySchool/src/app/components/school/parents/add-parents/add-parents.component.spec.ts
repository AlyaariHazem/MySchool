import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddParentsComponent } from './add-parents.component';

describe('AddParentsComponent', () => {
  let component: AddParentsComponent;
  let fixture: ComponentFixture<AddParentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddParentsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddParentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
