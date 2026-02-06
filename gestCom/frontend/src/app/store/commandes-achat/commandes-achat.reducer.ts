import { createFeature, createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { CommandeAchat, CommandesAchatFilter } from '../../core/models/commande-achat.model';
import { CommandesAchatPageActions, CommandesAchatApiActions } from './commandes-achat.actions';

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
 * Commandes Achat state interface extending EntityState
 */
export interface CommandesAchatState extends EntityState<CommandeAchat> {
  loading: boolean;
  error: string | null;
  selectedCommandeAchatId: string | null;
  filter: CommandesAchatFilter;
  pagination: PaginationState;
}

/**
 * Entity adapter for CommandeAchat using codeCommande as ID
 */
export const commandesAchatAdapter: EntityAdapter<CommandeAchat> = createEntityAdapter<CommandeAchat>({
  selectId: (commande: CommandeAchat) => commande.codeCommande,
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
 * Initial commandes achat state
 */
export const initialCommandesAchatState: CommandesAchatState = commandesAchatAdapter.getInitialState({
  loading: false,
  error: null,
  selectedCommandeAchatId: null,
  filter: {},
  pagination: initialPagination,
});

/**
 * Commandes Achat feature using createFeature
 */
export const commandesAchatFeature = createFeature({
  name: 'commandesAchat',
  reducer: createReducer(
    initialCommandesAchatState,

    // Load Commandes Achat
    on(CommandesAchatPageActions.loadCommandesAchat, (state): CommandesAchatState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(CommandesAchatApiActions.loadCommandesAchatSuccess, (state, { response }): CommandesAchatState =>
      commandesAchatAdapter.setAll(response.items, {
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

    on(CommandesAchatApiActions.loadCommandesAchatFailure, (state, { error }): CommandesAchatState => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Single Commande Achat
    on(CommandesAchatPageActions.loadCommandeAchat, (state): CommandesAchatState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(CommandesAchatApiActions.loadCommandeAchatSuccess, (state, { commande }): CommandesAchatState =>
      commandesAchatAdapter.upsertOne(commande, {
        ...state,
        loading: false,
        error: null,
        selectedCommandeAchatId: commande.codeCommande,
      })
    ),

    on(CommandesAchatApiActions.loadCommandeAchatFailure, (state, { error }): CommandesAchatState => ({
      ...state,
      loading: false,
      error,
    })),

    // Create Commande Achat
    on(CommandesAchatPageActions.createCommandeAchat, (state): CommandesAchatState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(CommandesAchatApiActions.createCommandeAchatSuccess, (state, { commande }): CommandesAchatState =>
      commandesAchatAdapter.addOne(commande, {
        ...state,
        loading: false,
        error: null,
        pagination: {
          ...state.pagination,
          totalCount: state.pagination.totalCount + 1,
        },
      })
    ),

    on(CommandesAchatApiActions.createCommandeAchatFailure, (state, { error }): CommandesAchatState => ({
      ...state,
      loading: false,
      error,
    })),

    // Update Commande Achat
    on(CommandesAchatPageActions.updateCommandeAchat, (state): CommandesAchatState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(CommandesAchatApiActions.updateCommandeAchatSuccess, (state, { commande }): CommandesAchatState =>
      commandesAchatAdapter.updateOne(
        { id: commande.codeCommande, changes: commande },
        {
          ...state,
          loading: false,
          error: null,
        }
      )
    ),

    on(CommandesAchatApiActions.updateCommandeAchatFailure, (state, { error }): CommandesAchatState => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Commande Achat
    on(CommandesAchatPageActions.deleteCommandeAchat, (state): CommandesAchatState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(CommandesAchatApiActions.deleteCommandeAchatSuccess, (state, { codeCommande }): CommandesAchatState =>
      commandesAchatAdapter.removeOne(codeCommande, {
        ...state,
        loading: false,
        error: null,
        selectedCommandeAchatId:
          state.selectedCommandeAchatId === codeCommande ? null : state.selectedCommandeAchatId,
        pagination: {
          ...state.pagination,
          totalCount: Math.max(0, state.pagination.totalCount - 1),
        },
      })
    ),

    on(CommandesAchatApiActions.deleteCommandeAchatFailure, (state, { error }): CommandesAchatState => ({
      ...state,
      loading: false,
      error,
    })),

    // Select Commande Achat
    on(CommandesAchatPageActions.selectCommandeAchat, (state, { codeCommande }): CommandesAchatState => ({
      ...state,
      selectedCommandeAchatId: codeCommande,
    })),

    on(CommandesAchatPageActions.clearSelection, (state): CommandesAchatState => ({
      ...state,
      selectedCommandeAchatId: null,
    })),

    // Filter
    on(CommandesAchatPageActions.setFilter, (state, { filter }): CommandesAchatState => ({
      ...state,
      filter,
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    on(CommandesAchatPageActions.clearFilter, (state): CommandesAchatState => ({
      ...state,
      filter: {},
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    // Pagination
    on(CommandesAchatPageActions.setPage, (state, { pageNumber, pageSize }): CommandesAchatState => ({
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
  name: commandesAchatFeatureKey,
  reducer: commandesAchatReducer,
  selectCommandesAchatState,
  selectLoading: selectCommandesAchatLoading,
  selectError: selectCommandesAchatError,
  selectSelectedCommandeAchatId,
  selectFilter: selectCommandesAchatFilter,
  selectPagination: selectCommandesAchatPagination,
} = commandesAchatFeature;

/**
 * Entity selectors
 */
const entitySelectors = commandesAchatAdapter.getSelectors();
export const selectCommandeAchatIds = entitySelectors.selectIds;
export const selectCommandeAchatEntities = entitySelectors.selectEntities;
export const selectAllCommandesAchatFromAdapter = entitySelectors.selectAll;
export const selectTotalCommandesAchat = entitySelectors.selectTotal;
