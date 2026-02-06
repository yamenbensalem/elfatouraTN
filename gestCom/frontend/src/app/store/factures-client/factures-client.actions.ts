import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  FactureClient,
  CreateFactureClientRequest,
  UpdateFactureClientRequest,
  FacturesClientFilter,
} from '../../core/models/facture-client.model';
import { PaginatedResponse, PaginationParams } from '../../core/models/client.model';

/**
 * Factures Client page actions - triggered by components
 */
export const FacturesClientPageActions = createActionGroup({
  source: 'Factures Client Page',
  events: {
    // Load factures client list
    'Load Factures Client': props<{ params?: PaginationParams }>(),

    // Load single facture client
    'Load Facture Client': props<{ codeFacture: string }>(),

    // Create facture client
    'Create Facture Client': props<{ facture: CreateFactureClientRequest }>(),

    // Update facture client
    'Update Facture Client': props<{ codeFacture: string; facture: UpdateFactureClientRequest }>(),

    // Delete facture client
    'Delete Facture Client': props<{ codeFacture: string }>(),

    // Select facture client
    'Select Facture Client': props<{ codeFacture: string | null }>(),

    // Filter and pagination
    'Set Filter': props<{ filter: FacturesClientFilter }>(),
    'Clear Filter': emptyProps(),
    'Set Page': props<{ pageNumber: number; pageSize: number }>(),

    // Clear selection
    'Clear Selection': emptyProps(),
  }
});

/**
 * Factures Client API actions - triggered by effects after API calls
 */
export const FacturesClientApiActions = createActionGroup({
  source: 'Factures Client API',
  events: {
    // Load factures client list results
    'Load Factures Client Success': props<{ response: PaginatedResponse<FactureClient> }>(),
    'Load Factures Client Failure': props<{ error: string }>(),

    // Load single facture client results
    'Load Facture Client Success': props<{ facture: FactureClient }>(),
    'Load Facture Client Failure': props<{ error: string }>(),

    // Create facture client results
    'Create Facture Client Success': props<{ facture: FactureClient }>(),
    'Create Facture Client Failure': props<{ error: string }>(),

    // Update facture client results
    'Update Facture Client Success': props<{ facture: FactureClient }>(),
    'Update Facture Client Failure': props<{ error: string }>(),

    // Delete facture client results
    'Delete Facture Client Success': props<{ codeFacture: string }>(),
    'Delete Facture Client Failure': props<{ error: string }>(),
  }
});
