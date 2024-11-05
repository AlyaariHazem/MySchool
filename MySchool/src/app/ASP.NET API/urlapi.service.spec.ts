import { TestBed } from '@angular/core/testing';

import { URLAPIService } from './urlapi.service';

describe('URLAPIService', () => {
  let service: URLAPIService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(URLAPIService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
