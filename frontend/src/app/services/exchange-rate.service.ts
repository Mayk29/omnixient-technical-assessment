import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { ExchangeRatesResponse } from '../models/exchange-rate.model';

@Injectable({ providedIn: 'root' })
export class ExchangeRateService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/api/exchangerates`;

  getDailyRates(date?: string): Observable<ExchangeRatesResponse> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date);
    }

    return this.http.get<ExchangeRatesResponse>(this.apiUrl, { params }).pipe(
      catchError(err => {
        const message =
          err?.error?.detail ??
          err?.message ??
          'An unexpected error occurred while loading exchange rates.';
        return throwError(() => new Error(message));
      })
    );
  }
}
