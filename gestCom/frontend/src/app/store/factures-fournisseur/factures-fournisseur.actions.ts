import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  FactureFournisseur,
  CreateFactureFournisseurRequest,
  UpdateFactureFournisseurRequest,
  FacturesFournisseurFilter,
} from '../../core/models/facture-fournisseur.model';
import { PaginatedResponse, PaginationParams } from '../../core/models/client.model';

/**
 * Factures Fournisseur page actions - triggered by components
 */
export const FacturesFournisseurPageActions = createActionGroup({
  source: 'Factures Fournisseur Page',
  events: {
    'Load Factures Fournisseur': props<{ params?: PaginationParams }>(),
    'Load Facture Fournisseur': props<{ codeFacture: string }>(),
    'Create Facture Fournisseur': props<{ facture: CreateFactureFournisseurRequest }>(),
    'Update Facture Fournisseur': props<{ codeFacture: string; facture: UpdateFactureFournisseurRequest }>(),
    'Delete Facture Fournisseur': props<{ codeFacture: string }>(),
    'Select Facture Fournisseur': props<{ codeFacture: string | null }>(),
    'Set Filter': props<{ filter: FacturesFournisseurFilter }>(),
    'Clear Filter': emptyProps(),
    'Set Page': props<{ pageNumber: number; pageSize: number }>(),
    'Clear Selection': emptyProps(),
  }
});

/**
 * Factures Fournisseur API actions - triggered by effects after API calls
 */
export const FacturesFournisseurApiActions = createActionGroup({
  source: 'Factures Fournisseur API',
  events: {
    'Load Factures Fournisseur Success': props<{ response: PaginatedResponse<FactureFournisseur> }>(),
    'Load Factures Fournisseur Failure': props<{ error: string }>(),
    'Load Facture Fournisseur Success': props<{ facture: FactureFournisseur }>(),
    'Load Facture Fournisseur Failure': props<{ error: string }>(),
    'Create Facture Fournisseur Success': props<{ facture: FactureFournisseur }>(),
    'Create Facture Fournisseur Failure': props<{ error: string }>(),
    'Update Facture Fournisseur Success': props<{ facture: FactureFournisseur }>(),
    'Update Facture Fournisseur Failure': props<{ error: string }>(),
    'Delete Facture Fournisseur Success': props<{ codeFacture: string }>(),
    'Delete Facture Fournisseur Failure': props<{ error: string }>(),
  }
});
