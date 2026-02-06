import { createFeature, createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Devis, DevisFilter } from '../../core/models/devis.model';
import { DevisPageActions, DevisApiActions } from './devis.actions';

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
 * Devis state interface extending EntityState
 */
export interface DevisState extends EntityState<Devis> {
  loading: boolean;
  error: string | null;
  selectedDevisId: string | null;
  filter: DevisFilter;
  pagination: PaginationState;
}

/**
 * Entity adapter for Devis using numeroDevis as ID, sorted by dateDevis desc
 */
export const devisAdapter: EntityAdapter<Devis> = createEntityAdapter<Devis>({
  selectId: (devis: Devis) => devis.numeroDevis,
  sortComparer: (a, b) => {
    const dateA = new Date(a.dateDevis).getTime();
    const dateB = new Date(b.dateDevis).getTime();
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
 * Initial devis state
 */
export const initialDevisState: DevisState = devisAdapter.getInitialState({
  loading: false,
  error: null,
  selectedDevisId: null,
  filter: {},
  pagination: initialPagination,
});

/**
 * Devis feature using createFeature
 */
export const devisFeature = createFeature({
  name: 'devis',
  reducer: createReducer(
    initialDevisState,

    // Load Devis List
    on(DevisPageActions.loadDevisList, (state): DevisState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(DevisApiActions.loadDevisListSuccess, (state, { response }): DevisState =>
      devisAdapter.setAll(response.items, {
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

    on(DevisApiActions.loadDevisListFailure, (state, { error }): DevisState => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Single Devis
    on(DevisPageActions.loadDevis, (state): DevisState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(DevisApiActions.loadDevisSuccess, (state, { devis }): DevisState =>
      devisAdapter.upsertOne(devis, {
        ...state,
        loading: false,
        error: null,
        selectedDevisId: devis.numeroDevis,
      })
    ),

    on(DevisApiActions.loadDevisFailure, (state, { error }): DevisState => ({
      ...state,
      loading: false,
      error,
    })),

    // Create Devis
    on(DevisPageActions.createDevis, (state): DevisState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(DevisApiActions.createDevisSuccess, (state, { devis }): DevisState =>
      devisAdapter.addOne(devis, {
        ...state,
        loading: false,
        error: null,
        pagination: {
          ...state.pagination,
          totalCount: state.pagination.totalCount + 1,
        },
      })
    ),

    on(DevisApiActions.createDevisFailure, (state, { error }): DevisState => ({
      ...state,
      loading: false,
      error,
    })),

    // Update Devis
    on(DevisPageActions.updateDevis, (state): DevisState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(DevisApiActions.updateDevisSuccess, (state, { devis }): DevisState =>
      devisAdapter.updateOne(
        { id: devis.numeroDevis, changes: devis },
        {
          ...state,
          loading: false,
          error: null,
        }
      )
    ),

    on(DevisApiActions.updateDevisFailure, (state, { error }): DevisState => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Devis
    on(DevisPageActions.deleteDevis, (state): DevisState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(DevisApiActions.deleteDevisSuccess, (state, { numeroDevis }): DevisState =>
      devisAdapter.removeOne(numeroDevis, {
        ...state,
        loading: false,
        error: null,
        selectedDevisId:
          state.selectedDevisId === numeroDevis ? null : state.selectedDevisId,
        pagination: {
          ...state.pagination,
          totalCount: Math.max(0, state.pagination.totalCount - 1),
        },
      })
    ),

    on(DevisApiActions.deleteDevisFailure, (state, { error }): DevisState => ({
      ...state,
      loading: false,
      error,
    })),

    // Select Devis
    on(DevisPageActions.selectDevis, (state, { numeroDevis }): DevisState => ({
      ...state,
      selectedDevisId: numeroDevis,
    })),

    on(DevisPageActions.clearSelection, (state): DevisState => ({
      ...state,
      selectedDevisId: null,
    })),

    // Filter
    on(DevisPageActions.setFilter, (state, { filter }): DevisState => ({
      ...state,
      filter,
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    on(DevisPageActions.clearFilter, (state): DevisState => ({
      ...state,
      filter: {},
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    // Pagination
    on(DevisPageActions.setPage, (state, { pageNumber, pageSize }): DevisState => ({
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
  name: devisFeatureKey,
  reducer: devisReducer,
  selectDevisState,
  selectLoading: selectDevisLoading,
  selectError: selectDevisError,
  selectSelectedDevisId,
  selectFilter: selectDevisFilter,
  selectPagination: selectDevisPagination,
} = devisFeature;

/**
 * Entity selectors (private - use selectors from devis.selectors.ts)
 */
const entitySelectors = devisAdapter.getSelectors();
export const selectDevisIds = entitySelectors.selectIds;
export const selectDevisEntities = entitySelectors.selectEntities;
export const selectAllDevisFromAdapter = entitySelectors.selectAll;
export const selectTotalDevis = entitySelectors.selectTotal;
