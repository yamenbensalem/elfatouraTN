import { AuthState } from './auth/auth.reducer';
import { ClientsState } from './clients/clients.reducer';

/**
 * Root application state interface
 * Combines all feature states into a single interface
 */
export interface AppState {
  auth: AuthState;
  clients: ClientsState;
}

/**
 * Feature state keys
 */
export const FEATURE_KEYS = {
  AUTH: 'auth',
  CLIENTS: 'clients',
} as const;

/**
 * Type for feature keys
 */
export type FeatureKey = (typeof FEATURE_KEYS)[keyof typeof FEATURE_KEYS];
