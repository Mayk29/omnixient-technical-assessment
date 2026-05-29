import { TestBed } from '@angular/core/testing';
import {
  provideHttpClient,
  withFetch
} from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController
} from '@angular/common/http/testing';
import { ExchangeRateService } from './exchange-rate.service';
import { ExchangeRatesResponse } from '../models/exchange-rate.model';
import { environment } from '../../environments/environment';

describe('ExchangeRateService', () => {
  let service: ExchangeRateService;
  let httpMock: HttpTestingController;

  const mockResponse: ExchangeRatesResponse = {
    date: '2024-05-14',
    rates: [
      { currencyCode: 'USD', currencyName: 'dollar', country: 'USA', amount: 1, rate: 22.589 },
      { currencyCode: 'EUR', currencyName: 'euro',   country: 'EMU', amount: 1, rate: 24.310 }
    ]
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      // Angular 18: use provideHttpClient + provideHttpClientTesting
      providers: [
        ExchangeRateService,
        provideHttpClient(withFetch()),
        provideHttpClientTesting()
      ]
    });
    service  = TestBed.inject(ExchangeRateService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should fetch rates without a date parameter', () => {
    service.getDailyRates().subscribe(res => {
      expect(res.rates.length).toBe(2);
      expect(res.rates[0].currencyCode).toBe('USD');
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/exchangerates`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should append date query param when date is provided', () => {
    service.getDailyRates('2024-01-15').subscribe();

    const req = httpMock.expectOne(r =>
      r.url === `${environment.apiBaseUrl}/api/exchangerates` &&
      r.params.get('date') === '2024-01-15'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should propagate a human-readable error on HTTP failure', () => {
    service.getDailyRates().subscribe({
      next: () => fail('expected error'),
      error: (err: Error) => {
        expect(err.message).toContain('unexpected error');
      }
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/exchangerates`);
    req.flush(null, { status: 502, statusText: 'Bad Gateway' });
  });

  it('should use the detail field from a ProblemDetails error body', () => {
    service.getDailyRates().subscribe({
      next: () => fail('expected error'),
      error: (err: Error) => {
        expect(err.message).toBe('CNB service unavailable');
      }
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/exchangerates`);
    req.flush(
      { detail: 'CNB service unavailable' },
      { status: 502, statusText: 'Bad Gateway' }
    );
  });
});
