import { createSelector } from '@ngrx/store';
import {
  devisFeature,
  selectDevisState,
  selectDevisLoading,
  selectDevisError,
  selectSelectedDevisId,
  selectDevisFilter,
  selectDevisPagination,
  selectAllDevisFromAdapter,
  selectDevisEntities,
  selectTotalDevis,
} from './devis.reducer';

/**
 * Re-export feature selectors
 */
export {
  selectDevisState,
  selectDevisLoading,
  selectDevisError,
  selectSelectedDevisId,
  selectDevisFilter,
  selectDevisPagination,
};

/**
 * Select all devis from the store
 */
export const selectAllDevis = createSelector(
  selectDevisState,
  selectAllDevisFromAdapter
);

/**
 * Select devis entities dictionary
 */
export const selectDevisDictionary = createSelector(
  selectDevisState,
  selectDevisEntities
);

/**
 * Select total number of devis in store
 */
export const selectDevisCount = createSelector(
  selectDevisState,
  selectTotalDevis
);

/**
 * Select a single devis by code
 */
export const selectDevisByCode = (codeDevis: string) =>
  createSelector(
    selectDevisDictionary,
    (entities) => entities[codeDevis] ?? null
  );

/**
 * Select currently selected devis
 */
export const selectSelectedDevis = createSelector(
  selectDevisDictionary,
  selectSelectedDevisId,
  (entities, selectedId) => (selectedId ? entities[selectedId] ?? null : null)
);

/**
 * Select devis list view model
 */
export const selectDevisListViewModel = createSelector(
  selectAllDevis,
  selectDevisLoading,
  selectDevisError,
  selectDevisPagination,
  selectDevisFilter,
  (devisList, loading, error, pagination, filter) => ({
    devisList,
    loading,
    error,
    pagination,
    filter,
    isEmpty: devisList.length === 0 && !loading,
    hasFilter: Object.keys(filter).some(key => filter[key as keyof typeof filter] !== undefined),
  })
);

/**
 * Select devis detail view model
 */
export const selectDevisDetailViewModel = createSelector(
  selectSelectedDevis,
  selectDevisLoading,
  selectDevisError,
  (devis, loading, error) => ({
    devis,
    loading,
    error,
    exists: devis !== null,
  })
);

/**
 * Select devis for dropdown/autocomplete
 */
export const selectDevisForDropdown = createSelector(
  selectAllDevis,
  (devisList) =>
    devisList.map((devis) => ({
      value: devis.codeDevis,
      label: `${devis.codeDevis} - ${devis.raisonSocialeClient || devis.codeClient}`,
    }))
);

/**
 * Select pagination info for display
 */
export const selectDevisPaginationInfo = createSelector(
  selectDevisPagination,
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
 * Check if there are any devis
 */
export const selectHasDevis = createSelector(
  selectDevisCount,
  (count) => count > 0
);
