import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { User, LoginRequest, AuthResponse } from '../../core/models/auth.models';

/**
 * Auth actions using createActionGroup for better organization
 */
export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    // Login actions
    'Login': props<{ credentials: LoginRequest }>(),
    'Login Success': props<{ response: AuthResponse }>(),
    'Login Failure': props<{ error: string }>(),

    // Logout actions
    'Logout': emptyProps(),
    'Logout Success': emptyProps(),

    // User loading actions
    'Load User': emptyProps(),
    'Load User Success': props<{ user: User }>(),
    'Load User Failure': props<{ error: string }>(),

    // Token refresh actions
    'Refresh Token': emptyProps(),
    'Refresh Token Success': props<{ response: AuthResponse }>(),
    'Refresh Token Failure': props<{ error: string }>(),

    // Error handling
    'Clear Error': emptyProps(),

    // Session restoration
    'Restore Session': emptyProps(),
    'Restore Session Success': props<{ user: User; token: string; refreshToken: string }>(),
    'Restore Session Failure': emptyProps(),
  }
});

/**
 * Auth API actions for external integrations
 */
export const AuthApiActions = createActionGroup({
  source: 'Auth API',
  events: {
    'Token Expired': emptyProps(),
    'Unauthorized': emptyProps(),
    'Session Timeout': emptyProps(),
  }
});
