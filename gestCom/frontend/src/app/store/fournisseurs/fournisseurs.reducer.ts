import { createFeature, createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Fournisseur, FournisseursFilter, PaginationParams } from '../../core/models/fournisseur.model';
import { FournisseursPageActions, FournisseursApiActions } from './fournisseurs.actions';

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
 * Fournisseurs state interface extending EntityState
 */
export interface FournisseursState extends EntityState<Fournisseur> {
  loading: boolean;
  error: string | null;
  selectedFournisseurId: string | null;
  filter: FournisseursFilter;
  pagination: PaginationState;
}

/**
 * Entity adapter for Fournisseur using codeFournisseur as ID
 */
export const fournisseursAdapter: EntityAdapter<Fournisseur> = createEntityAdapter<Fournisseur>({
  selectId: (fournisseur: Fournisseur) => fournisseur.codeFournisseur,
  sortComparer: (a, b) => a.raisonSociale.localeCompare(b.raisonSociale),
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
 * Initial fournisseurs state
 */
export const initialFournisseursState: FournisseursState = fournisseursAdapter.getInitialState({
  loading: false,
  error: null,
  selectedFournisseurId: null,
  filter: {},
  pagination: initialPagination,
});

/**
 * Fournisseurs feature using createFeature
 */
export const fournisseursFeature = createFeature({
  name: 'fournisseurs',
  reducer: createReducer(
    initialFournisseursState,

    // Load Fournisseurs
    on(FournisseursPageActions.loadFournisseurs, (state): FournisseursState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FournisseursApiActions.loadFournisseursSuccess, (state, { response }): FournisseursState =>
      fournisseursAdapter.setAll(response.items, {
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

    on(FournisseursApiActions.loadFournisseursFailure, (state, { error }): FournisseursState => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Single Fournisseur
    on(FournisseursPageActions.loadFournisseur, (state): FournisseursState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FournisseursApiActions.loadFournisseurSuccess, (state, { fournisseur }): FournisseursState =>
      fournisseursAdapter.upsertOne(fournisseur, {
        ...state,
        loading: false,
        error: null,
        selectedFournisseurId: fournisseur.codeFournisseur,
      })
    ),

    on(FournisseursApiActions.loadFournisseurFailure, (state, { error }): FournisseursState => ({
      ...state,
      loading: false,
      error,
    })),

    // Create Fournisseur
    on(FournisseursPageActions.createFournisseur, (state): FournisseursState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FournisseursApiActions.createFournisseurSuccess, (state, { fournisseur }): FournisseursState =>
      fournisseursAdapter.addOne(fournisseur, {
        ...state,
        loading: false,
        error: null,
        pagination: {
          ...state.pagination,
          totalCount: state.pagination.totalCount + 1,
        },
      })
    ),

    on(FournisseursApiActions.createFournisseurFailure, (state, { error }): FournisseursState => ({
      ...state,
      loading: false,
      error,
    })),

    // Update Fournisseur
    on(FournisseursPageActions.updateFournisseur, (state): FournisseursState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FournisseursApiActions.updateFournisseurSuccess, (state, { fournisseur }): FournisseursState =>
      fournisseursAdapter.updateOne(
        { id: fournisseur.codeFournisseur, changes: fournisseur },
        {
          ...state,
          loading: false,
          error: null,
        }
      )
    ),

    on(FournisseursApiActions.updateFournisseurFailure, (state, { error }): FournisseursState => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Fournisseur
    on(FournisseursPageActions.deleteFournisseur, (state): FournisseursState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FournisseursApiActions.deleteFournisseurSuccess, (state, { codeFournisseur }): FournisseursState =>
      fournisseursAdapter.removeOne(codeFournisseur, {
        ...state,
        loading: false,
        error: null,
        selectedFournisseurId:
          state.selectedFournisseurId === codeFournisseur ? null : state.selectedFournisseurId,
        pagination: {
          ...state.pagination,
          totalCount: Math.max(0, state.pagination.totalCount - 1),
        },
      })
    ),

    on(FournisseursApiActions.deleteFournisseurFailure, (state, { error }): FournisseursState => ({
      ...state,
      loading: false,
      error,
    })),

    // Select Fournisseur
    on(FournisseursPageActions.selectFournisseur, (state, { codeFournisseur }): FournisseursState => ({
      ...state,
      selectedFournisseurId: codeFournisseur,
    })),

    on(FournisseursPageActions.clearSelection, (state): FournisseursState => ({
      ...state,
      selectedFournisseurId: null,
    })),

    // Filter
    on(FournisseursPageActions.setFilter, (state, { filter }): FournisseursState => ({
      ...state,
      filter,
      pagination: {
        ...state.pagination,
        pageNumber: 1, // Reset to first page on filter change
      },
    })),

    on(FournisseursPageActions.clearFilter, (state): FournisseursState => ({
      ...state,
      filter: {},
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    // Pagination
    on(FournisseursPageActions.setPage, (state, { pageNumber, pageSize }): FournisseursState => ({
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
  name: fournisseursFeatureKey,
  reducer: fournisseursReducer,
  selectFournisseursState,
  selectLoading: selectFournisseursLoading,
  selectError: selectFournisseursError,
  selectSelectedFournisseurId,
  selectFilter: selectFournisseursFilter,
  selectPagination: selectFournisseursPagination,
} = fournisseursFeature;

/**
 * Entity selectors (private - use selectors from fournisseurs.selectors.ts)
 */
const entitySelectors = fournisseursAdapter.getSelectors();
export const selectFournisseurIds = entitySelectors.selectIds;
export const selectFournisseurEntities = entitySelectors.selectEntities;
export const selectAllFournisseursFromAdapter = entitySelectors.selectAll;
export const selectTotalFournisseurs = entitySelectors.selectTotal;
