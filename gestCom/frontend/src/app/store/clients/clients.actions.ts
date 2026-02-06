import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  Client,
  CreateClientRequest,
  UpdateClientRequest,
  PaginatedResponse,
  ClientsFilter,
  PaginationParams,
} from '../../core/models/client.model';

/**
 * Clients page actions - triggered by components
 */
export const ClientsPageActions = createActionGroup({
  source: 'Clients Page',
  events: {
    // Load clients list
    'Load Clients': props<{ params?: PaginationParams }>(),
    
    // Load single client
    'Load Client': props<{ codeClient: string }>(),
    
    // Create client
    'Create Client': props<{ client: CreateClientRequest }>(),
    
    // Update client
    'Update Client': props<{ codeClient: string; client: UpdateClientRequest }>(),
    
    // Delete client
    'Delete Client': props<{ codeClient: string }>(),
    
    // Select client
    'Select Client': props<{ codeClient: string | null }>(),
    
    // Filter and pagination
    'Set Filter': props<{ filter: ClientsFilter }>(),
    'Clear Filter': emptyProps(),
    'Set Page': props<{ pageNumber: number; pageSize: number }>(),
    
    // Clear selection
    'Clear Selection': emptyProps(),
  }
});

/**
 * Clients API actions - triggered by effects after API calls
 */
export const ClientsApiActions = createActionGroup({
  source: 'Clients API',
  events: {
    // Load clients results
    'Load Clients Success': props<{ response: PaginatedResponse<Client> }>(),
    'Load Clients Failure': props<{ error: string }>(),
    
    // Load single client results
    'Load Client Success': props<{ client: Client }>(),
    'Load Client Failure': props<{ error: string }>(),
    
    // Create client results
    'Create Client Success': props<{ client: Client }>(),
    'Create Client Failure': props<{ error: string }>(),
    
    // Update client results
    'Update Client Success': props<{ client: Client }>(),
    'Update Client Failure': props<{ error: string }>(),
    
    // Delete client results
    'Delete Client Success': props<{ codeClient: string }>(),
    'Delete Client Failure': props<{ error: string }>(),
  }
});
