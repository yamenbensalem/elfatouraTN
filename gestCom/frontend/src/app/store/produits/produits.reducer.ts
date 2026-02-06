import { createFeature, createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Produit, ProduitsFilter } from '../../core/models/produit.model';
import { PaginationParams } from '../../core/models/client.model';
import { ProduitsPageActions, ProduitsApiActions } from './produits.actions';

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
 * Produits state interface extending EntityState
 */
export interface ProduitsState extends EntityState<Produit> {
  loading: boolean;
  error: string | null;
  selectedProduitId: string | null;
  filter: ProduitsFilter;
  pagination: PaginationState;
}

/**
 * Entity adapter for Produit using codeProduit as ID
 */
export const produitsAdapter: EntityAdapter<Produit> = createEntityAdapter<Produit>({
  selectId: (produit: Produit) => produit.codeProduit,
  sortComparer: (a, b) => a.designation.localeCompare(b.designation),
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
 * Initial produits state
 */
export const initialProduitsState: ProduitsState = produitsAdapter.getInitialState({
  loading: false,
  error: null,
  selectedProduitId: null,
  filter: {},
  pagination: initialPagination,
});

/**
 * Produits feature using createFeature
 */
export const produitsFeature = createFeature({
  name: 'produits',
  reducer: createReducer(
    initialProduitsState,

    // Load Produits
    on(ProduitsPageActions.loadProduits, (state): ProduitsState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(ProduitsApiActions.loadProduitsSuccess, (state, { response }): ProduitsState =>
      produitsAdapter.setAll(response.items, {
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

    on(ProduitsApiActions.loadProduitsFailure, (state, { error }): ProduitsState => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Single Produit
    on(ProduitsPageActions.loadProduit, (state): ProduitsState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(ProduitsApiActions.loadProduitSuccess, (state, { produit }): ProduitsState =>
      produitsAdapter.upsertOne(produit, {
        ...state,
        loading: false,
        error: null,
        selectedProduitId: produit.codeProduit,
      })
    ),

    on(ProduitsApiActions.loadProduitFailure, (state, { error }): ProduitsState => ({
      ...state,
      loading: false,
      error,
    })),

    // Create Produit
    on(ProduitsPageActions.createProduit, (state): ProduitsState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(ProduitsApiActions.createProduitSuccess, (state, { produit }): ProduitsState =>
      produitsAdapter.addOne(produit, {
        ...state,
        loading: false,
        error: null,
        pagination: {
          ...state.pagination,
          totalCount: state.pagination.totalCount + 1,
        },
      })
    ),

    on(ProduitsApiActions.createProduitFailure, (state, { error }): ProduitsState => ({
      ...state,
      loading: false,
      error,
    })),

    // Update Produit
    on(ProduitsPageActions.updateProduit, (state): ProduitsState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(ProduitsApiActions.updateProduitSuccess, (state, { produit }): ProduitsState =>
      produitsAdapter.updateOne(
        { id: produit.codeProduit, changes: produit },
        {
          ...state,
          loading: false,
          error: null,
        }
      )
    ),

    on(ProduitsApiActions.updateProduitFailure, (state, { error }): ProduitsState => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Produit
    on(ProduitsPageActions.deleteProduit, (state): ProduitsState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(ProduitsApiActions.deleteProduitSuccess, (state, { codeProduit }): ProduitsState =>
      produitsAdapter.removeOne(codeProduit, {
        ...state,
        loading: false,
        error: null,
        selectedProduitId:
          state.selectedProduitId === codeProduit ? null : state.selectedProduitId,
        pagination: {
          ...state.pagination,
          totalCount: Math.max(0, state.pagination.totalCount - 1),
        },
      })
    ),

    on(ProduitsApiActions.deleteProduitFailure, (state, { error }): ProduitsState => ({
      ...state,
      loading: false,
      error,
    })),

    // Select Produit
    on(ProduitsPageActions.selectProduit, (state, { codeProduit }): ProduitsState => ({
      ...state,
      selectedProduitId: codeProduit,
    })),

    on(ProduitsPageActions.clearSelection, (state): ProduitsState => ({
      ...state,
      selectedProduitId: null,
    })),

    // Filter
    on(ProduitsPageActions.setFilter, (state, { filter }): ProduitsState => ({
      ...state,
      filter,
      pagination: {
        ...state.pagination,
        pageNumber: 1, // Reset to first page on filter change
      },
    })),

    on(ProduitsPageActions.clearFilter, (state): ProduitsState => ({
      ...state,
      filter: {},
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    // Pagination
    on(ProduitsPageActions.setPage, (state, { pageNumber, pageSize }): ProduitsState => ({
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
  name: produitsFeatureKey,
  reducer: produitsReducer,
  selectProduitsState,
  selectLoading: selectProduitsLoading,
  selectError: selectProduitsError,
  selectSelectedProduitId,
  selectFilter: selectProduitsFilter,
  selectPagination: selectProduitsPagination,
} = produitsFeature;

/**
 * Entity selectors (private - use selectors from produits.selectors.ts)
 */
const entitySelectors = produitsAdapter.getSelectors();
export const selectProduitIds = entitySelectors.selectIds;
export const selectProduitEntities = entitySelectors.selectEntities;
export const selectAllProduitsFromAdapter = entitySelectors.selectAll;
export const selectTotalProduits = entitySelectors.selectTotal;
