import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Error Interceptor - Handles HTTP errors globally
 * Angular 20 functional interceptor style
 */
export const errorInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle different error status codes
      switch (error.status) {
        case 401:
          return handleUnauthorizedError(req, next, error, authService, router);
        
        case 403:
          handleForbiddenError(router);
          break;
        
        case 404:
          console.error('Resource not found:', error.url);
          break;
        
        case 500:
          handleServerError();
          break;
        
        case 0:
          handleNetworkError();
          break;
        
        default:
          console.error('HTTP Error:', error.status, error.message);
      }

      return throwError(() => error);
    })
  );
};

/**
 * Handle 401 Unauthorized errors
 * Attempt to refresh token, if fails redirect to login
 */
function handleUnauthorizedError(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  error: HttpErrorResponse,
  authService: AuthService,
  router: Router
) {
  // Don't try to refresh if we're already on auth endpoints
  if (req.url.includes('/auth/')) {
    authService.logout();
    router.navigate(['/auth/login']);
    return throwError(() => error);
  }

  // Check if we have a refresh token
  const refreshToken = authService.getRefreshToken();
  
  if (!refreshToken) {
    authService.logout();
    router.navigate(['/auth/login'], {
      queryParams: { returnUrl: router.url }
    });
    return throwError(() => error);
  }

  // Try to refresh the token
  return authService.refreshToken().pipe(
    switchMap(() => {
      // Retry the original request with new token
      const token = authService.getToken();
      const authReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      return next(authReq);
    }),
    catchError(refreshError => {
      // Refresh failed, logout and redirect
      authService.logout();
      router.navigate(['/auth/login'], {
        queryParams: { returnUrl: router.url }
      });
      return throwError(() => refreshError);
    })
  );
}

/**
 * Handle 403 Forbidden errors
 */
function handleForbiddenError(router: Router): void {
  console.error('Access forbidden - insufficient permissions');
  router.navigate(['/forbidden']);
}

/**
 * Handle 500 Server errors
 */
function handleServerError(): void {
  console.error('Server error - please try again later');
  // You could show a toast/snackbar notification here
  // or navigate to an error page
}

/**
 * Handle network errors (status 0)
 */
function handleNetworkError(): void {
  console.error('Network error - please check your connection');
  // You could show a toast/snackbar notification here
}
