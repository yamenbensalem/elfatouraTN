import { createSelector } from '@ngrx/store';
import {
  clientsFeature,
  selectClientsState,
  selectClientsLoading,
  selectClientsError,
  selectSelectedClientId,
  selectClientsFilter,
  selectClientsPagination,
  selectAllClientsFromAdapter,
  selectClientEntities,
  selectTotalClients,
} from './clients.reducer';

/**
 * Re-export feature selectors
 */
export {
  selectClientsState,
  selectClientsLoading,
  selectClientsError,
  selectSelectedClientId,
  selectClientsFilter,
  selectClientsPagination,
};

/**
 * Select all clients from the store
 */
export const selectAllClients = createSelector(
  selectClientsState,
  selectAllClientsFromAdapter
);

/**
 * Select client entities dictionary
 */
export const selectClientsDictionary = createSelector(
  selectClientsState,
  selectClientEntities
);

/**
 * Select total number of clients in store
 */
export const selectClientsCount = createSelector(
  selectClientsState,
  selectTotalClients
);

/**
 * Select a single client by code
 */
export const selectClientByCode = (codeClient: string) =>
  createSelector(
    selectClientsDictionary,
    (entities) => entities[codeClient] ?? null
  );

/**
 * Select currently selected client
 */
export const selectSelectedClient = createSelector(
  selectClientsDictionary,
  selectSelectedClientId,
  (entities, selectedId) => (selectedId ? entities[selectedId] ?? null : null)
);

/**
 * Select clients list view model
 */
export const selectClientsListViewModel = createSelector(
  selectAllClients,
  selectClientsLoading,
  selectClientsError,
  selectClientsPagination,
  selectClientsFilter,
  (clients, loading, error, pagination, filter) => ({
    clients,
    loading,
    error,
    pagination,
    filter,
    isEmpty: clients.length === 0 && !loading,
    hasFilter: Object.keys(filter).some(key => filter[key as keyof typeof filter] !== undefined),
  })
);

/**
 * Select client detail view model
 */
export const selectClientDetailViewModel = createSelector(
  selectSelectedClient,
  selectClientsLoading,
  selectClientsError,
  (client, loading, error) => ({
    client,
    loading,
    error,
    exists: client !== null,
  })
);

/**
 * Select clients for dropdown/autocomplete
 */
export const selectClientsForDropdown = createSelector(
  selectAllClients,
  (clients) =>
    clients.map((client) => ({
      value: client.codeClient,
      label: `${client.codeClient} - ${client.nom}`,
      raisonSociale: client.nom,
    }))
);

/**
 * Select active (non-blocked) clients
 */
export const selectActiveClients = createSelector(
  selectAllClients,
  (clients) => clients.filter((client) => !client.bloque)
);

/**
 * Select blocked clients
 */
export const selectBlockedClients = createSelector(
  selectAllClients,
  (clients) => clients.filter((client) => client.bloque)
);

/**
 * Select clients with credit exceeded
 */
export const selectClientsWithCreditExceeded = createSelector(
  selectAllClients,
  (clients) =>
    clients.filter(
      (client) => client.plafondCredit > 0 && client.soldeActuel > client.plafondCredit
    )
);

/**
 * Select pagination info for display
 */
export const selectPaginationInfo = createSelector(
  selectClientsPagination,
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
 * Check if there are any clients
 */
export const selectHasClients = createSelector(
  selectClientsCount,
  (count) => count > 0
);

/**
 * Check if currently loading
 */
export const selectIsLoading = selectClientsLoading;

/**
 * Check if there's an error
 */
export const selectHasError = createSelector(
  selectClientsError,
  (error) => error !== null
);
