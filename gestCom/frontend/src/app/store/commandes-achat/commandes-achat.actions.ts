import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  CommandeAchat,
  CreateCommandeAchatRequest,
  UpdateCommandeAchatRequest,
  CommandesAchatFilter,
} from '../../core/models/commande-achat.model';
import { PaginatedResponse, PaginationParams } from '../../core/models/client.model';

/**
 * Commandes Achat page actions - triggered by components
 */
export const CommandesAchatPageActions = createActionGroup({
  source: 'Commandes Achat Page',
  events: {
    'Load Commandes Achat': props<{ params?: PaginationParams }>(),
    'Load Commande Achat': props<{ codeCommande: string }>(),
    'Create Commande Achat': props<{ commande: CreateCommandeAchatRequest }>(),
    'Update Commande Achat': props<{ codeCommande: string; commande: UpdateCommandeAchatRequest }>(),
    'Delete Commande Achat': props<{ codeCommande: string }>(),
    'Select Commande Achat': props<{ codeCommande: string | null }>(),
    'Set Filter': props<{ filter: CommandesAchatFilter }>(),
    'Clear Filter': emptyProps(),
    'Set Page': props<{ pageNumber: number; pageSize: number }>(),
    'Clear Selection': emptyProps(),
  }
});

/**
 * Commandes Achat API actions - triggered by effects after API calls
 */
export const CommandesAchatApiActions = createActionGroup({
  source: 'Commandes Achat API',
  events: {
    'Load Commandes Achat Success': props<{ response: PaginatedResponse<CommandeAchat> }>(),
    'Load Commandes Achat Failure': props<{ error: string }>(),
    'Load Commande Achat Success': props<{ commande: CommandeAchat }>(),
    'Load Commande Achat Failure': props<{ error: string }>(),
    'Create Commande Achat Success': props<{ commande: CommandeAchat }>(),
    'Create Commande Achat Failure': props<{ error: string }>(),
    'Update Commande Achat Success': props<{ commande: CommandeAchat }>(),
    'Update Commande Achat Failure': props<{ error: string }>(),
    'Delete Commande Achat Success': props<{ codeCommande: string }>(),
    'Delete Commande Achat Failure': props<{ error: string }>(),
  }
});
