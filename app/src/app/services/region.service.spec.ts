import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { RegionService } from './region.service';

describe('RegionService', () => {
  let service: RegionService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [RegionService, provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(RegionService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('calls region management endpoints', () => {
    service.getById('region-1').subscribe();
    http.expectOne('http://localhost:5130/api/regions/region-1').flush({});

    service.delete('region-1').subscribe();
    const deleteRequest = http.expectOne('http://localhost:5130/api/regions/region-1');
    expect(deleteRequest.request.method).toBe('DELETE');
    deleteRequest.flush(null);
  });
});
