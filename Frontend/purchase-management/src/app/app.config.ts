/**
 * @file app.config.ts
 * @description Root application configuration.
 * Registers global providers: Router, HttpClient for API calls.
 */

import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    // HttpClient is required by ApiService for all backend API calls
    provideHttpClient(withInterceptorsFromDi())
  ]
};
