import { createFeature, createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { FactureFournisseur, FacturesFournisseurFilter } from '../../core/models/facture-fournisseur.model';
import { FacturesFournisseurPageActions, FacturesFournisseurApiActions } from './factures-fournisseur.actions';

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
 * Factures Fournisseur state interface extending EntityState
 */
export interface FacturesFournisseurState extends EntityState<FactureFournisseur> {
  loading: boolean;
  error: string | null;
  selectedFactureFournisseurId: string | null;
  filter: FacturesFournisseurFilter;
  pagination: PaginationState;
}

/**
 * Entity adapter for FactureFournisseur using codeFacture as ID
 */
export const facturesFournisseurAdapter: EntityAdapter<FactureFournisseur> = createEntityAdapter<FactureFournisseur>({
  selectId: (facture: FactureFournisseur) => facture.codeFacture,
  sortComparer: (a, b) => new Date(b.dateFacture).getTime() - new Date(a.dateFacture).getTime(),
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
 * Initial factures fournisseur state
 */
export const initialFacturesFournisseurState: FacturesFournisseurState = facturesFournisseurAdapter.getInitialState({
  loading: false,
  error: null,
  selectedFactureFournisseurId: null,
  filter: {},
  pagination: initialPagination,
});

/**
 * Factures Fournisseur feature using createFeature
 */
export const facturesFournisseurFeature = createFeature({
  name: 'facturesFournisseur',
  reducer: createReducer(
    initialFacturesFournisseurState,

    // Load Factures Fournisseur
    on(FacturesFournisseurPageActions.loadFacturesFournisseur, (state): FacturesFournisseurState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FacturesFournisseurApiActions.loadFacturesFournisseurSuccess, (state, { response }): FacturesFournisseurState =>
      facturesFournisseurAdapter.setAll(response.items, {
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

    on(FacturesFournisseurApiActions.loadFacturesFournisseurFailure, (state, { error }): FacturesFournisseurState => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Single Facture Fournisseur
    on(FacturesFournisseurPageActions.loadFactureFournisseur, (state): FacturesFournisseurState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FacturesFournisseurApiActions.loadFactureFournisseurSuccess, (state, { facture }): FacturesFournisseurState =>
      facturesFournisseurAdapter.upsertOne(facture, {
        ...state,
        loading: false,
        error: null,
        selectedFactureFournisseurId: facture.codeFacture,
      })
    ),

    on(FacturesFournisseurApiActions.loadFactureFournisseurFailure, (state, { error }): FacturesFournisseurState => ({
      ...state,
      loading: false,
      error,
    })),

    // Create Facture Fournisseur
    on(FacturesFournisseurPageActions.createFactureFournisseur, (state): FacturesFournisseurState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FacturesFournisseurApiActions.createFactureFournisseurSuccess, (state, { facture }): FacturesFournisseurState =>
      facturesFournisseurAdapter.addOne(facture, {
        ...state,
        loading: false,
        error: null,
        pagination: {
          ...state.pagination,
          totalCount: state.pagination.totalCount + 1,
        },
      })
    ),

    on(FacturesFournisseurApiActions.createFactureFournisseurFailure, (state, { error }): FacturesFournisseurState => ({
      ...state,
      loading: false,
      error,
    })),

    // Update Facture Fournisseur
    on(FacturesFournisseurPageActions.updateFactureFournisseur, (state): FacturesFournisseurState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FacturesFournisseurApiActions.updateFactureFournisseurSuccess, (state, { facture }): FacturesFournisseurState =>
      facturesFournisseurAdapter.updateOne(
        { id: facture.codeFacture, changes: facture },
        {
          ...state,
          loading: false,
          error: null,
        }
      )
    ),

    on(FacturesFournisseurApiActions.updateFactureFournisseurFailure, (state, { error }): FacturesFournisseurState => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Facture Fournisseur
    on(FacturesFournisseurPageActions.deleteFactureFournisseur, (state): FacturesFournisseurState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(FacturesFournisseurApiActions.deleteFactureFournisseurSuccess, (state, { codeFacture }): FacturesFournisseurState =>
      facturesFournisseurAdapter.removeOne(codeFacture, {
        ...state,
        loading: false,
        error: null,
        selectedFactureFournisseurId:
          state.selectedFactureFournisseurId === codeFacture ? null : state.selectedFactureFournisseurId,
        pagination: {
          ...state.pagination,
          totalCount: Math.max(0, state.pagination.totalCount - 1),
        },
      })
    ),

    on(FacturesFournisseurApiActions.deleteFactureFournisseurFailure, (state, { error }): FacturesFournisseurState => ({
      ...state,
      loading: false,
      error,
    })),

    // Select Facture Fournisseur
    on(FacturesFournisseurPageActions.selectFactureFournisseur, (state, { codeFacture }): FacturesFournisseurState => ({
      ...state,
      selectedFactureFournisseurId: codeFacture,
    })),

    on(FacturesFournisseurPageActions.clearSelection, (state): FacturesFournisseurState => ({
      ...state,
      selectedFactureFournisseurId: null,
    })),

    // Filter
    on(FacturesFournisseurPageActions.setFilter, (state, { filter }): FacturesFournisseurState => ({
      ...state,
      filter,
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    on(FacturesFournisseurPageActions.clearFilter, (state): FacturesFournisseurState => ({
      ...state,
      filter: {},
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    // Pagination
    on(FacturesFournisseurPageActions.setPage, (state, { pageNumber, pageSize }): FacturesFournisseurState => ({
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
  name: facturesFournisseurFeatureKey,
  reducer: facturesFournisseurReducer,
  selectFacturesFournisseurState,
  selectLoading: selectFacturesFournisseurLoading,
  selectError: selectFacturesFournisseurError,
  selectSelectedFactureFournisseurId,
  selectFilter: selectFacturesFournisseurFilter,
  selectPagination: selectFacturesFournisseurPagination,
} = facturesFournisseurFeature;

/**
 * Entity selectors
 */
const entitySelectors = facturesFournisseurAdapter.getSelectors();
export const selectFactureFournisseurIds = entitySelectors.selectIds;
export const selectFactureFournisseurEntities = entitySelectors.selectEntities;
export const selectAllFacturesFournisseurFromAdapter = entitySelectors.selectAll;
export const selectTotalFacturesFournisseur = entitySelectors.selectTotal;
