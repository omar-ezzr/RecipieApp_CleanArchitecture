import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CuisineService } from './cuisine.service';

describe('CuisineService', () => {
  let service: CuisineService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CuisineService, provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(CuisineService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('calls cuisine endpoints', () => {
    service.getAll().subscribe();
    http.expectOne('http://localhost:5130/api/cuisines').flush([]);

    service.getRegions('cuisine-1').subscribe();
    http.expectOne('http://localhost:5130/api/cuisines/cuisine-1/regions').flush([]);
  });
});
