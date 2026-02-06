import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  Fournisseur,
  CreateFournisseurRequest,
  UpdateFournisseurRequest,
  PaginatedResponse,
  FournisseursFilter,
  PaginationParams,
} from '../../core/models/fournisseur.model';

/**
 * Fournisseurs page actions - triggered by components
 */
export const FournisseursPageActions = createActionGroup({
  source: 'Fournisseurs Page',
  events: {
    // Load fournisseurs list
    'Load Fournisseurs': props<{ params?: PaginationParams }>(),
    
    // Load single fournisseur
    'Load Fournisseur': props<{ codeFournisseur: string }>(),
    
    // Create fournisseur
    'Create Fournisseur': props<{ fournisseur: CreateFournisseurRequest }>(),
    
    // Update fournisseur
    'Update Fournisseur': props<{ codeFournisseur: string; fournisseur: UpdateFournisseurRequest }>(),
    
    // Delete fournisseur
    'Delete Fournisseur': props<{ codeFournisseur: string }>(),
    
    // Select fournisseur
    'Select Fournisseur': props<{ codeFournisseur: string | null }>(),
    
    // Filter and pagination
    'Set Filter': props<{ filter: FournisseursFilter }>(),
    'Clear Filter': emptyProps(),
    'Set Page': props<{ pageNumber: number; pageSize: number }>(),
    
    // Clear selection
    'Clear Selection': emptyProps(),
  }
});

/**
 * Fournisseurs API actions - triggered by effects after API calls
 */
export const FournisseursApiActions = createActionGroup({
  source: 'Fournisseurs API',
  events: {
    // Load fournisseurs results
    'Load Fournisseurs Success': props<{ response: PaginatedResponse<Fournisseur> }>(),
    'Load Fournisseurs Failure': props<{ error: string }>(),
    
    // Load single fournisseur results
    'Load Fournisseur Success': props<{ fournisseur: Fournisseur }>(),
    'Load Fournisseur Failure': props<{ error: string }>(),
    
    // Create fournisseur results
    'Create Fournisseur Success': props<{ fournisseur: Fournisseur }>(),
    'Create Fournisseur Failure': props<{ error: string }>(),
    
    // Update fournisseur results
    'Update Fournisseur Success': props<{ fournisseur: Fournisseur }>(),
    'Update Fournisseur Failure': props<{ error: string }>(),
    
    // Delete fournisseur results
    'Delete Fournisseur Success': props<{ codeFournisseur: string }>(),
    'Delete Fournisseur Failure': props<{ error: string }>(),
  }
});
