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

  // Stored as yyyy-MM-dd (what the date input natively uses)
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
   * Safely converts "yyyy-MM-dd" API string → Date for DatePipe.
   * Appends T00:00:00 to force local-time parsing in all browsers.
   * Returns null if the string is missing or malformed so DatePipe
   * never receives an Invalid Date.
   */
  parseApiDate(dateStr: string | null | undefined): Date | null {
    if (!dateStr || !/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) return null;
    const d = new Date(dateStr + 'T00:00:00');
    return isNaN(d.getTime()) ? null : d;
  }

  /**
   * The date input always gives us yyyy-MM-dd natively.
   * We pass it straight to the API which also expects yyyy-MM-dd.
   */
  onDateChange(value: string): void {
    this.selectedDate.set(value);
    this.loadRates();
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

  onSearch(value: string): void {
    this.search.set(value);
  }

  clearSearch(): void {
    this.search.set('');
  }

  /** Max date for the date picker — today in yyyy-MM-dd */
  get today(): string {
    return new Date().toISOString().split('T')[0];
  }
}