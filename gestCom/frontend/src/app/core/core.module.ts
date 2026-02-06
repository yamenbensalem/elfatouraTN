import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

// Services
import { AuthService } from './services/auth.service';
import { ApiService } from './services/api.service';

// Interceptors
import { jwtInterceptor } from './interceptors/jwt.interceptor';
import { errorInterceptor } from './interceptors/error.interceptor';

/**
 * Core Module - Contains singleton services and app-wide providers
 * This module should only be imported once in AppModule
 */
@NgModule({
  imports: [
    CommonModule
  ],
  providers: [
    // Core services (also provided in root, but listed here for clarity)
    AuthService,
    ApiService,
    
    // HTTP Client with interceptors
    provideHttpClient(
      withInterceptors([
        jwtInterceptor,
        errorInterceptor
      ])
    )
  ]
})
export class CoreModule {
  /**
   * Guard against importing CoreModule more than once
   * Uses @Optional to handle the case where it's imported first time
   * Uses @SkipSelf to look in parent injector, not the current one
   */
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error(
        'CoreModule has already been loaded. Import it in the AppModule only.'
      );
    }
  }
}
