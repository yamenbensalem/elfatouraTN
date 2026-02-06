import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';

/**
 * List of URLs that should skip authentication
 */
const PUBLIC_URLS = [
  '/auth/login',
  '/auth/register'
];

/**
 * Check if the request URL is public (doesn't need auth)
 */
function isPublicUrl(url: string): boolean {
  return PUBLIC_URLS.some(publicUrl => url.includes(publicUrl));
}

/**
 * JWT Interceptor - Adds Authorization header to requests
 * Angular 20 functional interceptor style
 */
export const jwtInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  // Skip authentication for public URLs
  if (isPublicUrl(req.url)) {
    return next(req);
  }

  // Get token from localStorage
  const token = localStorage.getItem('gestcom_token');

  // If no token, proceed without modification
  if (!token) {
    return next(req);
  }

  // Clone request and add Authorization header
  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authReq);
};
