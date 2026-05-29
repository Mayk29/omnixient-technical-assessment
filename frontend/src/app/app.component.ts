import { Component } from '@angular/core';
import { ExchangeRatesComponent } from './components/exchange-rates/exchange-rates.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ExchangeRatesComponent],
  template: '<app-exchange-rates />'
})
export class AppComponent {}
