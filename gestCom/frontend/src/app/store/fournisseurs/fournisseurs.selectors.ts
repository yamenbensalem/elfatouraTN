import { createSelector } from '@ngrx/store';
import {
  fournisseursFeature,
  selectFournisseursState,
  selectFournisseursLoading,
  selectFournisseursError,
  selectSelectedFournisseurId,
  selectFournisseursFilter,
  selectFournisseursPagination,
  selectAllFournisseursFromAdapter,
  selectFournisseurEntities,
  selectTotalFournisseurs,
} from './fournisseurs.reducer';

/**
 * Re-export feature selectors
 */
export {
  selectFournisseursState,
  selectFournisseursLoading,
  selectFournisseursError,
  selectSelectedFournisseurId,
  selectFournisseursFilter,
  selectFournisseursPagination,
};

/**
 * Select all fournisseurs from the store
 */
export const selectAllFournisseurs = createSelector(
  selectFournisseursState,
  selectAllFournisseursFromAdapter
);

/**
 * Select fournisseur entities dictionary
 */
export const selectFournisseursDictionary = createSelector(
  selectFournisseursState,
  selectFournisseurEntities
);

/**
 * Select total number of fournisseurs in store
 */
export const selectFournisseursCount = createSelector(
  selectFournisseursState,
  selectTotalFournisseurs
);

/**
 * Select a single fournisseur by code
 */
export const selectFournisseurByCode = (codeFournisseur: string) =>
  createSelector(
    selectFournisseursDictionary,
    (entities) => entities[codeFournisseur] ?? null
  );

/**
 * Select currently selected fournisseur
 */
export const selectSelectedFournisseur = createSelector(
  selectFournisseursDictionary,
  selectSelectedFournisseurId,
  (entities, selectedId) => (selectedId ? entities[selectedId] ?? null : null)
);

/**
 * Select fournisseurs list view model
 */
export const selectFournisseursListViewModel = createSelector(
  selectAllFournisseurs,
  selectFournisseursLoading,
  selectFournisseursError,
  selectFournisseursPagination,
  selectFournisseursFilter,
  (fournisseurs, loading, error, pagination, filter) => ({
    fournisseurs,
    loading,
    error,
    pagination,
    filter,
    isEmpty: fournisseurs.length === 0 && !loading,
    hasFilter: Object.keys(filter).some(key => filter[key as keyof typeof filter] !== undefined),
  })
);

/**
 * Select fournisseur detail view model
 */
export const selectFournisseurDetailViewModel = createSelector(
  selectSelectedFournisseur,
  selectFournisseursLoading,
  selectFournisseursError,
  (fournisseur, loading, error) => ({
    fournisseur,
    loading,
    error,
    exists: fournisseur !== null,
  })
);

/**
 * Select fournisseurs for dropdown/autocomplete
 */
export const selectFournisseursForDropdown = createSelector(
  selectAllFournisseurs,
  (fournisseurs) =>
    fournisseurs.map((fournisseur) => ({
      value: fournisseur.codeFournisseur,
      label: `${fournisseur.codeFournisseur} - ${fournisseur.raisonSociale}`,
      raisonSociale: fournisseur.raisonSociale,
    }))
);

/**
 * Select active (non-blocked) fournisseurs
 */
export const selectActiveFournisseurs = createSelector(
  selectAllFournisseurs,
  (fournisseurs) => fournisseurs.filter((fournisseur) => !fournisseur.bloque)
);

/**
 * Select blocked fournisseurs
 */
export const selectBlockedFournisseurs = createSelector(
  selectAllFournisseurs,
  (fournisseurs) => fournisseurs.filter((fournisseur) => fournisseur.bloque)
);

/**
 * Select pagination info for display
 */
export const selectPaginationInfo = createSelector(
  selectFournisseursPagination,
  (pagination) => ({
    ...pagination,
    startIndex: (pagination.pageNumber - 1) * pagination.pageSize + 1,
    endIndex: Math.min(
      pagination.pageNumber * pagination.pageSize,
      pagination.totalCount
    ),
    displayText: `${(pagination.pageNumber - 1) * pagination.pageSize + 1}-${Math.min(
      pagination.pageNumber * pagination.pageSize,
      pagination.totalCount
    )} sur ${pagination.totalCount}`,
  })
);

/**
 * Check if there are any fournisseurs
 */
export const selectHasFournisseurs = createSelector(
  selectFournisseursCount,
  (count) => count > 0
);

/**
 * Check if currently loading
 */
export const selectIsLoading = selectFournisseursLoading;

/**
 * Check if there's an error
 */
export const selectHasError = createSelector(
  selectFournisseursError,
  (error) => error !== null
);
