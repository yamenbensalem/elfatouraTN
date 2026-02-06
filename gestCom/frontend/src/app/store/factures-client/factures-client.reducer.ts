import { createFeature, createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { FactureClient, FacturesClientFilter } from '../../core/models/facture-client.model';
import { FacturesClientPageActions, FacturesClientApiActions } from './factures-client.actions';

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
 * Factures Client state interface extending EntityState
 */
export interface FacturesClientState extends EntityState<FactureClient> {
  loading: boolean;
  error: string | null;
  selectedFactureClientId: string | null;
  filter: FacturesClientFilter;
  pagination: PaginationState;
}

/**
 * Entity adapter for FactureClient using codeFacture as ID
 */
export const facturesClientAdapter: EntityAdapter<FactureClient> = createEntityAdapter<FactureClient>({
  selectId: (facture: FactureClient) => facture.codeFacture,
  sortComparer: (a, b) => {
    const dateA = new Date(a.dateFacture).getTime();
    const dateB = new Date(b.dateFacture).getTime();
    return dateB - dateA; // Descending
  },
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
 * Initial factures client state
 */
export const initialFacturesClientState: FacturesClientState = facturesClientAdapter.getInitialState({
  loading: false,
  error: null,
  selectedFactureClientId: null,
  filter: {},
  pagination: initialPagination,
});

/**
 * Factures Client feature using createFeature
 */
export const facturesClientFeature = createFeature({
  name: 'facturesClient',
  reducer: createReducer(
    initialFacturesClientState,

    // Load Factures Client
    on(FacturesClientPageActions.loadFacturesClient, (state): FacturesClientState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FacturesClientApiActions.loadFacturesClientSuccess, (state, { response }): FacturesClientState =>
      facturesClientAdapter.setAll(response.items, {
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

    on(FacturesClientApiActions.loadFacturesClientFailure, (state, { error }): FacturesClientState => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Single Facture Client
    on(FacturesClientPageActions.loadFactureClient, (state): FacturesClientState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FacturesClientApiActions.loadFactureClientSuccess, (state, { facture }): FacturesClientState =>
      facturesClientAdapter.upsertOne(facture, {
        ...state,
        loading: false,
        error: null,
        selectedFactureClientId: facture.codeFacture,
      })
    ),

    on(FacturesClientApiActions.loadFactureClientFailure, (state, { error }): FacturesClientState => ({
      ...state,
      loading: false,
      error,
    })),

    // Create Facture Client
    on(FacturesClientPageActions.createFactureClient, (state): FacturesClientState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FacturesClientApiActions.createFactureClientSuccess, (state, { facture }): FacturesClientState =>
      facturesClientAdapter.addOne(facture, {
        ...state,
        loading: false,
        error: null,
        pagination: {
          ...state.pagination,
          totalCount: state.pagination.totalCount + 1,
        },
      })
    ),

    on(FacturesClientApiActions.createFactureClientFailure, (state, { error }): FacturesClientState => ({
      ...state,
      loading: false,
      error,
    })),

    // Update Facture Client
    on(FacturesClientPageActions.updateFactureClient, (state): FacturesClientState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FacturesClientApiActions.updateFactureClientSuccess, (state, { facture }): FacturesClientState =>
      facturesClientAdapter.updateOne(
        { id: facture.codeFacture, changes: facture },
        {
          ...state,
          loading: false,
          error: null,
        }
      )
    ),

    on(FacturesClientApiActions.updateFactureClientFailure, (state, { error }): FacturesClientState => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Facture Client
    on(FacturesClientPageActions.deleteFactureClient, (state): FacturesClientState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FacturesClientApiActions.deleteFactureClientSuccess, (state, { codeFacture }): FacturesClientState =>
      facturesClientAdapter.removeOne(codeFacture, {
        ...state,
        loading: false,
        error: null,
        selectedFactureClientId:
          state.selectedFactureClientId === codeFacture ? null : state.selectedFactureClientId,
        pagination: {
          ...state.pagination,
          totalCount: Math.max(0, state.pagination.totalCount - 1),
        },
      })
    ),

    on(FacturesClientApiActions.deleteFactureClientFailure, (state, { error }): FacturesClientState => ({
      ...state,
      loading: false,
      error,
    })),

    // Select Facture Client
    on(FacturesClientPageActions.selectFactureClient, (state, { codeFacture }): FacturesClientState => ({
      ...state,
      selectedFactureClientId: codeFacture,
    })),

    on(FacturesClientPageActions.clearSelection, (state): FacturesClientState => ({
      ...state,
      selectedFactureClientId: null,
    })),

    // Filter
    on(FacturesClientPageActions.setFilter, (state, { filter }): FacturesClientState => ({
      ...state,
      filter,
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    on(FacturesClientPageActions.clearFilter, (state): FacturesClientState => ({
      ...state,
      filter: {},
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    // Pagination
    on(FacturesClientPageActions.setPage, (state, { pageNumber, pageSize }): FacturesClientState => ({
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
  name: facturesClientFeatureKey,
  reducer: facturesClientReducer,
  selectFacturesClientState,
  selectLoading: selectFacturesClientLoading,
  selectError: selectFacturesClientError,
  selectSelectedFactureClientId,
  selectFilter: selectFacturesClientFilter,
  selectPagination: selectFacturesClientPagination,
} = facturesClientFeature;

/**
 * Entity selectors (private - use selectors from factures-client.selectors.ts)
 */
const entitySelectors = facturesClientAdapter.getSelectors();
export const selectFactureClientIds = entitySelectors.selectIds;
export const selectFactureClientEntities = entitySelectors.selectEntities;
export const selectAllFacturesClientFromAdapter = entitySelectors.selectAll;
export const selectTotalFacturesClient = entitySelectors.selectTotal;
