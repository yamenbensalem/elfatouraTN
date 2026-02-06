import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  CommandeVente,
  CreateCommandeVenteRequest,
  UpdateCommandeVenteRequest,
  CommandesVenteFilter,
} from '../../core/models/commande-vente.model';
import { PaginatedResponse, PaginationParams } from '../../core/models/client.model';

/**
 * Commandes Vente page actions - triggered by components
 */
export const CommandesVentePageActions = createActionGroup({
  source: 'Commandes Vente Page',
  events: {
    'Load Commandes Vente': props<{ params?: PaginationParams }>(),
    'Load Commande Vente': props<{ codeCommande: string }>(),
    'Create Commande Vente': props<{ commande: CreateCommandeVenteRequest }>(),
    'Update Commande Vente': props<{ codeCommande: string; commande: UpdateCommandeVenteRequest }>(),
    'Delete Commande Vente': props<{ codeCommande: string }>(),
    'Select Commande Vente': props<{ codeCommande: string | null }>(),
    'Set Filter': props<{ filter: CommandesVenteFilter }>(),
    'Clear Filter': emptyProps(),
    'Set Page': props<{ pageNumber: number; pageSize: number }>(),
    'Clear Selection': emptyProps(),
  }
});

/**
 * Commandes Vente API actions - triggered by effects after API calls
 */
export const CommandesVenteApiActions = createActionGroup({
  source: 'Commandes Vente API',
  events: {
    'Load Commandes Vente Success': props<{ response: PaginatedResponse<CommandeVente> }>(),
    'Load Commandes Vente Failure': props<{ error: string }>(),
    'Load Commande Vente Success': props<{ commande: CommandeVente }>(),
    'Load Commande Vente Failure': props<{ error: string }>(),
    'Create Commande Vente Success': props<{ commande: CommandeVente }>(),
    'Create Commande Vente Failure': props<{ error: string }>(),
    'Update Commande Vente Success': props<{ commande: CommandeVente }>(),
    'Update Commande Vente Failure': props<{ error: string }>(),
    'Delete Commande Vente Success': props<{ codeCommande: string }>(),
    'Delete Commande Vente Failure': props<{ error: string }>(),
  }
});
