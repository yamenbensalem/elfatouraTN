import { createSelector } from '@ngrx/store';
import {
  facturesClientFeature,
  selectFacturesClientState,
  selectFacturesClientLoading,
  selectFacturesClientError,
  selectSelectedFactureClientId,
  selectFacturesClientFilter,
  selectFacturesClientPagination,
  selectAllFacturesClientFromAdapter,
  selectFactureClientEntities,
  selectTotalFacturesClient,
} from './factures-client.reducer';

/**
 * Re-export feature selectors
 */
export {
  selectFacturesClientState,
  selectFacturesClientLoading,
  selectFacturesClientError,
  selectSelectedFactureClientId,
  selectFacturesClientFilter,
  selectFacturesClientPagination,
};

/**
 * Select all factures client from the store
 */
export const selectAllFacturesClient = createSelector(
  selectFacturesClientState,
  selectAllFacturesClientFromAdapter
);

/**
 * Select facture client entities dictionary
 */
export const selectFacturesClientDictionary = createSelector(
  selectFacturesClientState,
  selectFactureClientEntities
);

/**
 * Select total number of factures client in store
 */
export const selectFacturesClientCount = createSelector(
  selectFacturesClientState,
  selectTotalFacturesClient
);

/**
 * Select a single facture client by code
 */
export const selectFactureClientByCode = (codeFacture: string) =>
  createSelector(
    selectFacturesClientDictionary,
    (entities) => entities[codeFacture] ?? null
  );

/**
 * Select currently selected facture client
 */
export const selectSelectedFactureClient = createSelector(
  selectFacturesClientDictionary,
  selectSelectedFactureClientId,
  (entities, selectedId) => (selectedId ? entities[selectedId] ?? null : null)
);

/**
 * Select factures client list view model
 */
export const selectFacturesClientListViewModel = createSelector(
  selectAllFacturesClient,
  selectFacturesClientLoading,
  selectFacturesClientError,
  selectFacturesClientPagination,
  selectFacturesClientFilter,
  (facturesList, loading, error, pagination, filter) => ({
    facturesList,
    loading,
    error,
    pagination,
    filter,
    isEmpty: facturesList.length === 0 && !loading,
    hasFilter: Object.keys(filter).some(key => filter[key as keyof typeof filter] !== undefined),
  })
);

/**
 * Select facture client detail view model
 */
export const selectFactureClientDetailViewModel = createSelector(
  selectSelectedFactureClient,
  selectFacturesClientLoading,
  selectFacturesClientError,
  (facture, loading, error) => ({
    facture,
    loading,
    error,
    exists: facture !== null,
  })
);

/**
 * Select factures client for dropdown/autocomplete
 */
export const selectFacturesClientForDropdown = createSelector(
  selectAllFacturesClient,
  (facturesList) =>
    facturesList.map((facture) => ({
      value: facture.codeFacture,
      label: `${facture.codeFacture} - ${facture.raisonSocialeClient || facture.codeClient}`,
    }))
);

/**
 * Select pagination info for display
 */
export const selectFacturesClientPaginationInfo = createSelector(
  selectFacturesClientPagination,
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
 * Check if there are any factures client
 */
export const selectHasFacturesClient = createSelector(
  selectFacturesClientCount,
  (count) => count > 0
);
