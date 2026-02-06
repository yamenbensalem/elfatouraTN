import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  Devis,
  CreateDevisRequest,
  UpdateDevisRequest,
  DevisFilter,
} from '../../core/models/devis.model';
import { PaginatedResponse, PaginationParams } from '../../core/models/client.model';

/**
 * Devis page actions - triggered by components
 */
export const DevisPageActions = createActionGroup({
  source: 'Devis Page',
  events: {
    // Load devis list
    'Load Devis List': props<{ params?: PaginationParams }>(),

    // Load single devis
    'Load Devis': props<{ numeroDevis: string }>(),

    // Create devis
    'Create Devis': props<{ devis: CreateDevisRequest }>(),

    // Update devis
    'Update Devis': props<{ numeroDevis: string; devis: UpdateDevisRequest }>(),

    // Delete devis
    'Delete Devis': props<{ numeroDevis: string }>(),

    // Select devis
    'Select Devis': props<{ numeroDevis: string | null }>(),

    // Filter and pagination
    'Set Filter': props<{ filter: DevisFilter }>(),
    'Clear Filter': emptyProps(),
    'Set Page': props<{ pageNumber: number; pageSize: number }>(),

    // Clear selection
    'Clear Selection': emptyProps(),
  }
});

/**
 * Devis API actions - triggered by effects after API calls
 */
export const DevisApiActions = createActionGroup({
  source: 'Devis API',
  events: {
    // Load devis list results
    'Load Devis List Success': props<{ response: PaginatedResponse<Devis> }>(),
    'Load Devis List Failure': props<{ error: string }>(),

    // Load single devis results
    'Load Devis Success': props<{ devis: Devis }>(),
    'Load Devis Failure': props<{ error: string }>(),

    // Create devis results
    'Create Devis Success': props<{ devis: Devis }>(),
    'Create Devis Failure': props<{ error: string }>(),

    // Update devis results
    'Update Devis Success': props<{ devis: Devis }>(),
    'Update Devis Failure': props<{ error: string }>(),

    // Delete devis results
    'Delete Devis Success': props<{ numeroDevis: string }>(),
    'Delete Devis Failure': props<{ error: string }>(),
  }
});
