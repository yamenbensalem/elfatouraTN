import { createFeature, createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { CommandeVente, CommandesVenteFilter } from '../../core/models/commande-vente.model';
import { CommandesVentePageActions, CommandesVenteApiActions } from './commandes-vente.actions';

/**
 * Pagination state interface
 */
export interface PaginationState {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Commandes Vente state interface extending EntityState
 */
export interface CommandesVenteState extends EntityState<CommandeVente> {
  loading: boolean;
  error: string | null;
  selectedCommandeVenteId: string | null;
  filter: CommandesVenteFilter;
  pagination: PaginationState;
}

/**
 * Entity adapter for CommandeVente using codeCommande as ID
 */
export const commandesVenteAdapter: EntityAdapter<CommandeVente> = createEntityAdapter<CommandeVente>({
  selectId: (commande: CommandeVente) => commande.codeCommande,
  sortComparer: (a, b) => new Date(b.dateCommande).getTime() - new Date(a.dateCommande).getTime(),
});

/**
 * Initial pagination state
 */
const initialPagination: PaginationState = {
  pageNumber: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

/**
 * Initial commandes vente state
 */
export const initialCommandesVenteState: CommandesVenteState = commandesVenteAdapter.getInitialState({
  loading: false,
  error: null,
  selectedCommandeVenteId: null,
  filter: {},
  pagination: initialPagination,
});

/**
 * Commandes Vente feature using createFeature
 */
export const commandesVenteFeature = createFeature({
  name: 'commandesVente',
  reducer: createReducer(
    initialCommandesVenteState,

    // Load Commandes Vente
    on(CommandesVentePageActions.loadCommandesVente, (state): CommandesVenteState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(CommandesVenteApiActions.loadCommandesVenteSuccess, (state, { response }): CommandesVenteState =>
      commandesVenteAdapter.setAll(response.items, {
        ...state,
        loading: false,
        error: null,
        pagination: {
          pageNumber: response.pageNumber,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          totalPages: response.totalPages,
          hasPreviousPage: response.hasPreviousPage,
          hasNextPage: response.hasNextPage,
        },
      })
    ),

    on(CommandesVenteApiActions.loadCommandesVenteFailure, (state, { error }): CommandesVenteState => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Single Commande Vente
    on(CommandesVentePageActions.loadCommandeVente, (state): CommandesVenteState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(CommandesVenteApiActions.loadCommandeVenteSuccess, (state, { commande }): CommandesVenteState =>
      commandesVenteAdapter.upsertOne(commande, {
        ...state,
        loading: false,
        error: null,
        selectedCommandeVenteId: commande.codeCommande,
      })
    ),

    on(CommandesVenteApiActions.loadCommandeVenteFailure, (state, { error }): CommandesVenteState => ({
      ...state,
      loading: false,
      error,
    })),

    // Create Commande Vente
    on(CommandesVentePageActions.createCommandeVente, (state): CommandesVenteState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(CommandesVenteApiActions.createCommandeVenteSuccess, (state, { commande }): CommandesVenteState =>
      commandesVenteAdapter.addOne(commande, {
        ...state,
        loading: false,
        error: null,
        pagination: {
          ...state.pagination,
          totalCount: state.pagination.totalCount + 1,
        },
      })
    ),

    on(CommandesVenteApiActions.createCommandeVenteFailure, (state, { error }): CommandesVenteState => ({
      ...state,
      loading: false,
      error,
    })),

    // Update Commande Vente
    on(CommandesVentePageActions.updateCommandeVente, (state): CommandesVenteState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(CommandesVenteApiActions.updateCommandeVenteSuccess, (state, { commande }): CommandesVenteState =>
      commandesVenteAdapter.updateOne(
        { id: commande.codeCommande, changes: commande },
        {
          ...state,
          loading: false,
          error: null,
        }
      )
    ),

    on(CommandesVenteApiActions.updateCommandeVenteFailure, (state, { error }): CommandesVenteState => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Commande Vente
    on(CommandesVentePageActions.deleteCommandeVente, (state): CommandesVenteState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(CommandesVenteApiActions.deleteCommandeVenteSuccess, (state, { codeCommande }): CommandesVenteState =>
      commandesVenteAdapter.removeOne(codeCommande, {
        ...state,
        loading: false,
        error: null,
        selectedCommandeVenteId:
          state.selectedCommandeVenteId === codeCommande ? null : state.selectedCommandeVenteId,
        pagination: {
          ...state.pagination,
          totalCount: Math.max(0, state.pagination.totalCount - 1),
        },
      })
    ),

    on(CommandesVenteApiActions.deleteCommandeVenteFailure, (state, { error }): CommandesVenteState => ({
      ...state,
      loading: false,
      error,
    })),

    // Select Commande Vente
    on(CommandesVentePageActions.selectCommandeVente, (state, { codeCommande }): CommandesVenteState => ({
      ...state,
      selectedCommandeVenteId: codeCommande,
    })),

    on(CommandesVentePageActions.clearSelection, (state): CommandesVenteState => ({
      ...state,
      selectedCommandeVenteId: null,
    })),

    // Filter
    on(CommandesVentePageActions.setFilter, (state, { filter }): CommandesVenteState => ({
      ...state,
      filter,
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    on(CommandesVentePageActions.clearFilter, (state): CommandesVenteState => ({
      ...state,
      filter: {},
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    // Pagination
    on(CommandesVentePageActions.setPage, (state, { pageNumber, pageSize }): CommandesVenteState => ({
      ...state,
      pagination: {
        ...state.pagination,
        pageNumber,
        pageSize,
      },
    }))
  ),
});

/**
 * Export reducer and feature
 */
export const {
  name: commandesVenteFeatureKey,
  reducer: commandesVenteReducer,
  selectCommandesVenteState,
  selectLoading: selectCommandesVenteLoading,
  selectError: selectCommandesVenteError,
  selectSelectedCommandeVenteId,
  selectFilter: selectCommandesVenteFilter,
  selectPagination: selectCommandesVentePagination,
} = commandesVenteFeature;

/**
 * Entity selectors
 */
const entitySelectors = commandesVenteAdapter.getSelectors();
export const selectCommandeVenteIds = entitySelectors.selectIds;
export const selectCommandeVenteEntities = entitySelectors.selectEntities;
export const selectAllCommandesVenteFromAdapter = entitySelectors.selectAll;
export const selectTotalCommandesVente = entitySelectors.selectTotal;
