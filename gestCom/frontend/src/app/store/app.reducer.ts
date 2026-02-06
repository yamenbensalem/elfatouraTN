import { ActionReducerMap, MetaReducer, ActionReducer } from '@ngrx/store';
import { AppState } from './app.state';
import { authReducer } from './auth/auth.reducer';
import { clientsReducer } from './clients/clients.reducer';
import { environment } from '../../environments/environment';

/**
 * Root reducer map combining all feature reducers
 */
export const reducers: ActionReducerMap<AppState> = {
  auth: authReducer,
  clients: clientsReducer,
};

/**
 * Console logger for debugging in development
 */
export function logger(reducer: ActionReducer<AppState>): ActionReducer<AppState> {
  return (state, action) => {
    const result = reducer(state, action);
    
    console.groupCollapsed(`[NgRx] ${action.type}`);
    console.log('Previous State:', state);
    console.log('Action:', action);
    console.log('Next State:', result);
    console.groupEnd();
    
    return result;
  };
}

/**
 * State hydration from localStorage (optional)
 */
export function hydrationMetaReducer(reducer: ActionReducer<AppState>): ActionReducer<AppState> {
  return (state, action) => {
    // You can add state persistence/hydration logic here if needed
    return reducer(state, action);
  };
}

/**
 * Meta reducers for the application
 * Logger is only added in development mode
 */
export const metaReducers: MetaReducer<AppState>[] = !environment.production
  ? [logger]
  : [];

/**
 * Optional: Add hydration meta reducer if state persistence is needed
 * export const metaReducers: MetaReducer<AppState>[] = !environment.production
 *   ? [logger, hydrationMetaReducer]
 *   : [hydrationMetaReducer];
 */
