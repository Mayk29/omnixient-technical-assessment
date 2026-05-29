import { Component, OnInit, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ExchangeRateService } from '../../services/exchange-rate.service';
import { ExchangeRate, ExchangeRatesResponse } from '../../models/exchange-rate.model';

@Component({
  selector: 'app-exchange-rates',
  standalone: true,
  // Angular 18: @if / @for are built-in — no CommonModule needed.
  // DatePipe is still required for the date format pipe in the template.
  imports: [DatePipe],
  templateUrl: './exchange-rates.component.html',
  styleUrl: './exchange-rates.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExchangeRatesComponent implements OnInit {
  private readonly service = inject(ExchangeRateService);

  // State
  response     = signal<ExchangeRatesResponse | null>(null);
  loading      = signal(false);
  error        = signal<string | null>(null);
  search       = signal('');
  selectedDate = signal('');

  // Used by @for in the skeleton loader
  readonly skeletonRows = [1, 2, 3, 4, 5, 6, 7, 8];

  filteredRates = computed(() => {
    const term  = this.search().toLowerCase().trim();
    const rates = this.response()?.rates ?? [];
    if (!term) return rates;
    return rates.filter(
      r =>
        r.currencyCode.toLowerCase().includes(term) ||
        r.currencyName.toLowerCase().includes(term) ||
        r.country.toLowerCase().includes(term)
    );
  });

  ngOnInit(): void {
    this.loadRates();
  }

  loadRates(): void {
    this.loading.set(true);
    this.error.set(null);

    this.service.getDailyRates(this.selectedDate() || undefined).subscribe({
      next: data => {
        this.response.set(data);
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.loading.set(false);
      }
    });
  }

  onDateChange(value: string): void {
    this.selectedDate.set(value);
  }

  onSearch(value: string): void {
    this.search.set(value);
  }

  /** Format rate with correct per-unit label, e.g. "22.589 CZK / 1 USD" */
  formatRate(rate: ExchangeRate): string {
    return `${rate.rate.toFixed(3)} CZK / ${rate.amount} ${rate.currencyCode}`;
  }

  /** Today's date string for the max attribute on the date input */
  get today(): string {
    return new Date().toISOString().split('T')[0];
  }
}
