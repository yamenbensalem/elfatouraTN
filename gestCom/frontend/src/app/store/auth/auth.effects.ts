import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType, OnInitEffects } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, tap, switchMap } from 'rxjs/operators';
import { jwtDecode } from 'jwt-decode';

import { AuthService } from '../../core/services/auth.service';
import { AuthActions, AuthApiActions } from './auth.actions';
import { User } from '../../core/models/auth.models';

/**
 * Storage keys for auth tokens
 */
const STORAGE_KEYS = {
  TOKEN: 'gestcom_token',
  REFRESH_TOKEN: 'gestcom_refresh_token',
  USER: 'gestcom_user',
} as const;

/**
 * JWT token payload interface
 */
interface JwtPayload {
  sub: string;
  email: string;
  name: string;
  role: string;
  entrepriseId: string;
  exp: number;
  iat: number;
}

/**
 * Auth effects for handling authentication side effects
 */
@Injectable()
export class AuthEffects implements OnInitEffects {
  private readonly actions$ = inject(Actions);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  /**
   * Handle login action
   */
  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.login),
      exhaustMap(({ credentials }) =>
        this.authService.login(credentials.email, credentials.password).pipe(
          map((response) => AuthActions.loginSuccess({ response })),
          catchError((error) =>
            of(AuthActions.loginFailure({
              error: error.error?.message || 'Échec de la connexion. Veuillez vérifier vos identifiants.'
            }))
          )
        )
      )
    )
  );

  /**
   * Handle successful login - store tokens and navigate
   */
  loginSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loginSuccess),
        tap(({ response }) => {
          // Store tokens in localStorage
          localStorage.setItem(STORAGE_KEYS.TOKEN, response.token);
          localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, response.refreshToken);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(response.user));

          // Navigate to dashboard
          this.router.navigate(['/dashboard']);
        })
      ),
    { dispatch: false }
  );

  /**
   * Handle logout action
   */
  logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logout),
      switchMap(() => {
        // Perform local logout
        this.authService.logout();
        return of(AuthActions.logoutSuccess());
      })
    )
  );

  /**
   * Handle successful logout - clear storage and navigate
   */
  logoutSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logoutSuccess),
        tap(() => {
          // Clear localStorage
          localStorage.removeItem(STORAGE_KEYS.TOKEN);
          localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
          localStorage.removeItem(STORAGE_KEYS.USER);

          // Navigate to login
          this.router.navigate(['/auth/login']);
        })
      ),
    { dispatch: false }
  );

  /**
   * Handle load user from storage
   */
  loadUser$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loadUser),
      map(() => {
        const userJson = localStorage.getItem(STORAGE_KEYS.USER);
        const token = localStorage.getItem(STORAGE_KEYS.TOKEN);

        if (userJson && token) {
          try {
            // Verify token is not expired
            const decoded = jwtDecode<JwtPayload>(token);
            const isExpired = decoded.exp * 1000 < Date.now();

            if (!isExpired) {
              const user: User = JSON.parse(userJson);
              return AuthActions.loadUserSuccess({ user });
            }
          } catch {
            // Token decode failed
          }
        }

        return AuthActions.loadUserFailure({ error: 'Aucune session valide trouvée' });
      })
    )
  );

  /**
   * Handle token refresh
   */
  refreshToken$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.refreshToken),
      exhaustMap(() => {
        const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);

        if (!refreshToken) {
          return of(AuthActions.refreshTokenFailure({ error: 'Aucun token de rafraîchissement' }));
        }

        return this.authService.refreshToken().pipe(
          map((response) => AuthActions.refreshTokenSuccess({ response })),
          catchError((error) =>
            of(AuthActions.refreshTokenFailure({
              error: error.error?.message || 'Échec du rafraîchissement du token'
            }))
          )
        );
      })
    )
  );

  /**
   * Handle successful token refresh - update storage
   */
  refreshTokenSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.refreshTokenSuccess),
        tap(({ response }) => {
          localStorage.setItem(STORAGE_KEYS.TOKEN, response.token);
          localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, response.refreshToken);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(response.user));
        })
      ),
    { dispatch: false }
  );

  /**
   * Handle token refresh failure - logout
   */
  refreshTokenFailure$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.refreshTokenFailure),
        tap(() => {
          localStorage.removeItem(STORAGE_KEYS.TOKEN);
          localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
          localStorage.removeItem(STORAGE_KEYS.USER);
          this.router.navigate(['/auth/login']);
        })
      ),
    { dispatch: false }
  );

  /**
   * Restore session on app init
   */
  restoreSession$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.restoreSession),
      map(() => {
        const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
        const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
        const userJson = localStorage.getItem(STORAGE_KEYS.USER);

        if (token && refreshToken && userJson) {
          try {
            const decoded = jwtDecode<JwtPayload>(token);
            const isExpired = decoded.exp * 1000 < Date.now();

            if (!isExpired) {
              const user: User = JSON.parse(userJson);
              return AuthActions.restoreSessionSuccess({ user, token, refreshToken });
            } else {
              // Token expired, try to refresh
              return AuthActions.refreshToken();
            }
          } catch {
            // Token decode failed
          }
        }

        return AuthActions.restoreSessionFailure();
      })
    )
  );

  /**
   * Handle API unauthorized errors
   */
  handleUnauthorized$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthApiActions.unauthorized, AuthApiActions.tokenExpired),
        tap(() => {
          localStorage.removeItem(STORAGE_KEYS.TOKEN);
          localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
          localStorage.removeItem(STORAGE_KEYS.USER);
          this.router.navigate(['/auth/login']);
        })
      ),
    { dispatch: false }
  );

  /**
   * Initialize auth state on app start
   */
  ngrxOnInitEffects(): Action {
    return AuthActions.restoreSession();
  }
}
