export interface ExchangeRate {
  currencyCode: string;
  currencyName: string;
  country: string;
  amount: number;
  rate: number;
}

export interface ExchangeRatesResponse {
  date: string; // ISO date string: "2024-05-14"
  rates: ExchangeRate[];
}
