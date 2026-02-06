import { createSelector } from '@ngrx/store';
import {
  authFeature,
  selectAuthState,
  selectUser,
  selectToken,
  selectRefreshToken,
  selectLoading,
  selectError,
  selectIsAuthenticated,
} from './auth.reducer';

/**
 * Re-export auto-generated selectors from createFeature
 */
export {
  selectAuthState,
  selectUser,
  selectToken,
  selectRefreshToken,
  selectIsAuthenticated,
};

/**
 * Renamed selectors for clarity
 */
export const selectAuthLoading = selectLoading;
export const selectAuthError = selectError;

/**
 * Select user's full name
 */
export const selectUserFullName = createSelector(
  selectUser,
  (user) => user ? `${user.prenom} ${user.nom}` : null
);

/**
 * Select user's email
 */
export const selectUserEmail = createSelector(
  selectUser,
  (user) => user?.email ?? null
);

/**
 * Select user's role
 */
export const selectUserRole = createSelector(
  selectUser,
  (user) => user?.role ?? null
);

/**
 * Select user's entreprise ID
 */
export const selectUserEntrepriseId = createSelector(
  selectUser,
  (user) => user?.entrepriseId ?? null
);

/**
 * Check if user has a specific role
 */
export const selectHasRole = (role: string) =>
  createSelector(selectUserRole, (userRole) => userRole === role);

/**
 * Check if user is admin
 */
export const selectIsAdmin = createSelector(
  selectUserRole,
  (role) => role === 'Admin'
);

/**
 * Select auth view model for login component
 */
export const selectLoginViewModel = createSelector(
  selectAuthLoading,
  selectAuthError,
  selectIsAuthenticated,
  (loading, error, isAuthenticated) => ({
    loading,
    error,
    isAuthenticated,
  })
);

/**
 * Select auth view model for header/navbar component
 */
export const selectAuthHeaderViewModel = createSelector(
  selectUser,
  selectIsAuthenticated,
  selectUserFullName,
  (user, isAuthenticated, fullName) => ({
    user,
    isAuthenticated,
    fullName,
    initials: user ? `${user.prenom[0]}${user.nom[0]}`.toUpperCase() : null,
  })
);

/**
 * Check if token exists
 */
export const selectHasToken = createSelector(
  selectToken,
  (token) => !!token
);

/**
 * Check if refresh token exists
 */
export const selectHasRefreshToken = createSelector(
  selectRefreshToken,
  (refreshToken) => !!refreshToken
);
