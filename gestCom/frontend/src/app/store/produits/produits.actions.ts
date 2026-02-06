import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  Produit,
  CreateProduitRequest,
  UpdateProduitRequest,
  ProduitsFilter,
} from '../../core/models/produit.model';
import { PaginatedResponse, PaginationParams } from '../../core/models/client.model';

/**
 * Produits page actions - triggered by components
 */
export const ProduitsPageActions = createActionGroup({
  source: 'Produits Page',
  events: {
    // Load produits list
    'Load Produits': props<{ params?: PaginationParams }>(),
    
    // Load single produit
    'Load Produit': props<{ codeProduit: string }>(),
    
    // Create produit
    'Create Produit': props<{ produit: CreateProduitRequest }>(),
    
    // Update produit
    'Update Produit': props<{ codeProduit: string; produit: UpdateProduitRequest }>(),
    
    // Delete produit
    'Delete Produit': props<{ codeProduit: string }>(),
    
    // Select produit
    'Select Produit': props<{ codeProduit: string | null }>(),
    
    // Filter and pagination
    'Set Filter': props<{ filter: ProduitsFilter }>(),
    'Clear Filter': emptyProps(),
    'Set Page': props<{ pageNumber: number; pageSize: number }>(),
    
    // Clear selection
    'Clear Selection': emptyProps(),
  }
});

/**
 * Produits API actions - triggered by effects after API calls
 */
export const ProduitsApiActions = createActionGroup({
  source: 'Produits API',
  events: {
    // Load produits results
    'Load Produits Success': props<{ response: PaginatedResponse<Produit> }>(),
    'Load Produits Failure': props<{ error: string }>(),
    
    // Load single produit results
    'Load Produit Success': props<{ produit: Produit }>(),
    'Load Produit Failure': props<{ error: string }>(),
    
    // Create produit results
    'Create Produit Success': props<{ produit: Produit }>(),
    'Create Produit Failure': props<{ error: string }>(),
    
    // Update produit results
    'Update Produit Success': props<{ produit: Produit }>(),
    'Update Produit Failure': props<{ error: string }>(),
    
    // Delete produit results
    'Delete Produit Success': props<{ codeProduit: string }>(),
    'Delete Produit Failure': props<{ error: string }>(),
  }
});
