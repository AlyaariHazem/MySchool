import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditParentsComponent } from './edit-parents.component';

describe('EditParentsComponent', () => {
  let component: EditParentsComponent;
  let fixture: ComponentFixture<EditParentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditParentsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EditParentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
