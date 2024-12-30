import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MediaPartalComponent } from './media-partal.component';

describe('MediaPartalComponent', () => {
  let component: MediaPartalComponent;
  let fixture: ComponentFixture<MediaPartalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MediaPartalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MediaPartalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
