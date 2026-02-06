import { createSelector } from '@ngrx/store';
import {
  commandesAchatFeature,
  selectCommandesAchatState,
  selectCommandesAchatLoading,
  selectCommandesAchatError,
  selectSelectedCommandeAchatId,
  selectCommandesAchatFilter,
  selectCommandesAchatPagination,
  selectAllCommandesAchatFromAdapter,
  selectCommandeAchatEntities,
  selectTotalCommandesAchat,
} from './commandes-achat.reducer';

/**
 * Re-export feature selectors
 */
export {
  selectCommandesAchatState,
  selectCommandesAchatLoading,
  selectCommandesAchatError,
  selectSelectedCommandeAchatId,
  selectCommandesAchatFilter,
  selectCommandesAchatPagination,
};

/**
 * Select all commandes achat from the store
 */
export const selectAllCommandesAchat = createSelector(
  selectCommandesAchatState,
  selectAllCommandesAchatFromAdapter
);

/**
 * Select commande achat entities dictionary
 */
export const selectCommandesAchatDictionary = createSelector(
  selectCommandesAchatState,
  selectCommandeAchatEntities
);

/**
 * Select total number of commandes achat in store
 */
export const selectCommandesAchatCount = createSelector(
  selectCommandesAchatState,
  selectTotalCommandesAchat
);

/**
 * Select a single commande achat by code
 */
export const selectCommandeAchatByCode = (codeCommande: string) =>
  createSelector(
    selectCommandesAchatDictionary,
    (entities) => entities[codeCommande] ?? null
  );

/**
 * Select currently selected commande achat
 */
export const selectSelectedCommandeAchat = createSelector(
  selectCommandesAchatDictionary,
  selectSelectedCommandeAchatId,
  (entities, selectedId) => (selectedId ? entities[selectedId] ?? null : null)
);

/**
 * Select commandes achat list view model
 */
export const selectCommandesAchatListViewModel = createSelector(
  selectAllCommandesAchat,
  selectCommandesAchatLoading,
  selectCommandesAchatError,
  selectCommandesAchatPagination,
  selectCommandesAchatFilter,
  (commandes, loading, error, pagination, filter) => ({
    commandes,
    loading,
    error,
    pagination,
    filter,
    isEmpty: commandes.length === 0 && !loading,
    hasFilter: Object.keys(filter).some(key => filter[key as keyof typeof filter] !== undefined),
  })
);

/**
 * Select commande achat detail view model
 */
export const selectCommandeAchatDetailViewModel = createSelector(
  selectSelectedCommandeAchat,
  selectCommandesAchatLoading,
  selectCommandesAchatError,
  (commande, loading, error) => ({
    commande,
    loading,
    error,
    exists: commande !== null,
  })
);
