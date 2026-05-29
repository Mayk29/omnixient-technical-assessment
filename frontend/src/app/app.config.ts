import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withFetch, withInterceptorsFromDi } from '@angular/common/http';
import { provideRouter } from '@angular/router';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter([]),
    // Angular 18: withFetch() uses the native Fetch API instead of XHR
    provideHttpClient(withFetch(), withInterceptorsFromDi())
  ]
};
