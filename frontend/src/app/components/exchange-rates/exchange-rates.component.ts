import {
  Component, OnInit, OnDestroy,
  inject, signal, computed, ChangeDetectionStrategy
} from '@angular/core';
import { DatePipe, TitleCasePipe, DecimalPipe } from '@angular/common';
import { ExchangeRateService } from '../../services/exchange-rate.service';
import { ExchangeRate, ExchangeRatesResponse } from '../../models/exchange-rate.model';

@Component({
  selector: 'app-exchange-rates',
  standalone: true,
  imports: [DatePipe, TitleCasePipe, DecimalPipe],
  templateUrl: './exchange-rates.component.html',
  styleUrl: './exchange-rates.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExchangeRatesComponent implements OnInit, OnDestroy {
  private readonly service = inject(ExchangeRateService);
  private clockTimer: ReturnType<typeof setInterval> | null = null;

  response     = signal<ExchangeRatesResponse | null>(null);
  loading      = signal(false);
  error        = signal<string | null>(null);
  search       = signal('');
  selectedDate = signal('');

  // Live clock — updates every second
  currentTime = signal<Date>(new Date());

  readonly skeletonRows = [1, 2, 3, 4, 5, 6, 7, 8];

  filteredRates = computed(() => {
    const term  = this.search().toLowerCase().trim();
    const rates = this.response()?.rates ?? [];
    if (!term) return rates;
    return rates.filter(r =>
      r.currencyCode.toLowerCase().includes(term) ||
      r.currencyName.toLowerCase().includes(term) ||
      r.country.toLowerCase().includes(term)
    );
  });

  /**
   * Converts the API date string "yyyy-MM-dd" to a proper Date object
   * by appending T00:00:00 so DatePipe parses it correctly in all browsers.
   */
  parseApiDate(dateStr: string): Date {
    return new Date(dateStr + 'T00:00:00');
  }

  ngOnInit(): void {
    this.loadRates();
    this.clockTimer = setInterval(() => this.currentTime.set(new Date()), 1000);
  }

  ngOnDestroy(): void {
    if (this.clockTimer !== null) clearInterval(this.clockTimer);
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
    this.loadRates();
  }

  onSearch(value: string): void {
    this.search.set(value);
  }

  clearSearch(): void {
    this.search.set('');
  }

  get today(): string {
    return new Date().toISOString().split('T')[0];
  }
}