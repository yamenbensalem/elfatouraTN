import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Role Guard - Protects routes that require specific roles
 * Angular 20 functional guard style
 * 
 * Usage in route configuration:
 * {
 *   path: 'admin',
 *   component: AdminComponent,
 *   canActivate: [authGuard, roleGuard],
 *   data: { roles: ['Admin', 'SuperAdmin'] }
 * }
 */
export const roleGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot
): boolean | UrlTree => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // First check if user is authenticated
  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/auth/login']);
  }

  // Get required roles from route data
  const requiredRoles = route.data['roles'] as string[] | undefined;

  // If no roles specified, allow access
  if (!requiredRoles || requiredRoles.length === 0) {
    return true;
  }

  // Check if user has any of the required roles
  const userRole = authService.getUserRole();

  if (userRole && requiredRoles.includes(userRole)) {
    return true;
  }

  // User doesn't have required role, redirect to forbidden page
  console.warn(`Access denied: User role '${userRole}' not in required roles [${requiredRoles.join(', ')}]`);
  return router.createUrlTree(['/forbidden']);
};

/**
 * Admin Guard - Shortcut for admin-only routes
 */
export const adminGuard: CanActivateFn = (): boolean | UrlTree => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/auth/login']);
  }

  if (authService.hasAnyRole(['Admin', 'SuperAdmin'])) {
    return true;
  }

  return router.createUrlTree(['/forbidden']);
};
