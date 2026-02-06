import { createSelector } from '@ngrx/store';
import {
  facturesFournisseurFeature,
  selectFacturesFournisseurState,
  selectFacturesFournisseurLoading,
  selectFacturesFournisseurError,
  selectSelectedFactureFournisseurId,
  selectFacturesFournisseurFilter,
  selectFacturesFournisseurPagination,
  selectAllFacturesFournisseurFromAdapter,
  selectFactureFournisseurEntities,
  selectTotalFacturesFournisseur,
} from './factures-fournisseur.reducer';

/**
 * Re-export feature selectors
 */
export {
  selectFacturesFournisseurState,
  selectFacturesFournisseurLoading,
  selectFacturesFournisseurError,
  selectSelectedFactureFournisseurId,
  selectFacturesFournisseurFilter,
  selectFacturesFournisseurPagination,
};

/**
 * Select all factures fournisseur from the store
 */
export const selectAllFacturesFournisseur = createSelector(
  selectFacturesFournisseurState,
  selectAllFacturesFournisseurFromAdapter
);

/**
 * Select facture fournisseur entities dictionary
 */
export const selectFacturesFournisseurDictionary = createSelector(
  selectFacturesFournisseurState,
  selectFactureFournisseurEntities
);

/**
 * Select total number of factures fournisseur in store
 */
export const selectFacturesFournisseurCount = createSelector(
  selectFacturesFournisseurState,
  selectTotalFacturesFournisseur
);

/**
 * Select a single facture fournisseur by code
 */
export const selectFactureFournisseurByCode = (codeFacture: string) =>
  createSelector(
    selectFacturesFournisseurDictionary,
    (entities) => entities[codeFacture] ?? null
  );

/**
 * Select currently selected facture fournisseur
 */
export const selectSelectedFactureFournisseur = createSelector(
  selectFacturesFournisseurDictionary,
  selectSelectedFactureFournisseurId,
  (entities, selectedId) => (selectedId ? entities[selectedId] ?? null : null)
);

/**
 * Select factures fournisseur list view model
 */
export const selectFacturesFournisseurListViewModel = createSelector(
  selectAllFacturesFournisseur,
  selectFacturesFournisseurLoading,
  selectFacturesFournisseurError,
  selectFacturesFournisseurPagination,
  selectFacturesFournisseurFilter,
  (factures, loading, error, pagination, filter) => ({
    factures,
    loading,
    error,
    pagination,
    filter,
    isEmpty: factures.length === 0 && !loading,
    hasFilter: Object.keys(filter).some(key => filter[key as keyof typeof filter] !== undefined),
  })
);

/**
 * Select facture fournisseur detail view model
 */
export const selectFactureFournisseurDetailViewModel = createSelector(
  selectSelectedFactureFournisseur,
  selectFacturesFournisseurLoading,
  selectFacturesFournisseurError,
  (facture, loading, error) => ({
    facture,
    loading,
    error,
    exists: facture !== null,
  })
);
