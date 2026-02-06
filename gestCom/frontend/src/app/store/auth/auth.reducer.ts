import { createFeature, createReducer, on } from '@ngrx/store';
import { User } from '../../core/models/auth.models';
import { AuthActions, AuthApiActions } from './auth.actions';

/**
 * Auth state interface
 */
export interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  loading: boolean;
  error: string | null;
  isAuthenticated: boolean;
}

/**
 * Initial auth state
 */
export const initialAuthState: AuthState = {
  user: null,
  token: null,
  refreshToken: null,
  loading: false,
  error: null,
  isAuthenticated: false,
};

/**
 * Auth feature using createFeature for automatic selector generation
 */
export const authFeature = createFeature({
  name: 'auth',
  reducer: createReducer(
    initialAuthState,

    // Login
    on(AuthActions.login, (state): AuthState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(AuthActions.loginSuccess, (state, { response }): AuthState => ({
      ...state,
      user: response.user,
      token: response.token,
      refreshToken: response.refreshToken,
      loading: false,
      error: null,
      isAuthenticated: true,
    })),

    on(AuthActions.loginFailure, (state, { error }): AuthState => ({
      ...state,
      loading: false,
      error,
      isAuthenticated: false,
    })),

    // Logout
    on(AuthActions.logout, (state): AuthState => ({
      ...state,
      loading: true,
    })),

    on(AuthActions.logoutSuccess, (): AuthState => ({
      ...initialAuthState,
    })),

    // Load User
    on(AuthActions.loadUser, (state): AuthState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(AuthActions.loadUserSuccess, (state, { user }): AuthState => ({
      ...state,
      user,
      loading: false,
      error: null,
    })),

    on(AuthActions.loadUserFailure, (state, { error }): AuthState => ({
      ...state,
      loading: false,
      error,
    })),

    // Refresh Token
    on(AuthActions.refreshToken, (state): AuthState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(AuthActions.refreshTokenSuccess, (state, { response }): AuthState => ({
      ...state,
      token: response.token,
      refreshToken: response.refreshToken,
      user: response.user,
      loading: false,
      error: null,
      isAuthenticated: true,
    })),

    on(AuthActions.refreshTokenFailure, (state, { error }): AuthState => ({
      ...state,
      loading: false,
      error,
      isAuthenticated: false,
      token: null,
      refreshToken: null,
      user: null,
    })),

    // Clear Error
    on(AuthActions.clearError, (state): AuthState => ({
      ...state,
      error: null,
    })),

    // Restore Session
    on(AuthActions.restoreSession, (state): AuthState => ({
      ...state,
      loading: true,
    })),

    on(AuthActions.restoreSessionSuccess, (state, { user, token, refreshToken }): AuthState => ({
      ...state,
      user,
      token,
      refreshToken,
      loading: false,
      isAuthenticated: true,
      error: null,
    })),

    on(AuthActions.restoreSessionFailure, (state): AuthState => ({
      ...state,
      loading: false,
      isAuthenticated: false,
    })),

    // API Actions
    on(AuthApiActions.tokenExpired, AuthApiActions.unauthorized, (state): AuthState => ({
      ...state,
      isAuthenticated: false,
      token: null,
      refreshToken: null,
      user: null,
      error: 'Session expirée. Veuillez vous reconnecter.',
    })),

    on(AuthApiActions.sessionTimeout, (): AuthState => ({
      ...initialAuthState,
      error: 'Session expirée pour inactivité.',
    }))
  ),
});

/**
 * Export reducer and feature name
 */
export const {
  name: authFeatureKey,
  reducer: authReducer,
  selectAuthState,
  selectUser,
  selectToken,
  selectRefreshToken,
  selectLoading,
  selectError,
  selectIsAuthenticated,
} = authFeature;
