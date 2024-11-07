import { TestBed } from '@angular/core/testing';

import { BackendAspService } from './backend-asp.service';

describe('BackendAspService', () => {
  let service: BackendAspService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BackendAspService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
