import { createSelector } from '@ngrx/store';
import {
  commandesVenteFeature,
  selectCommandesVenteState,
  selectCommandesVenteLoading,
  selectCommandesVenteError,
  selectSelectedCommandeVenteId,
  selectCommandesVenteFilter,
  selectCommandesVentePagination,
  selectAllCommandesVenteFromAdapter,
  selectCommandeVenteEntities,
  selectTotalCommandesVente,
} from './commandes-vente.reducer';

/**
 * Re-export feature selectors
 */
export {
  selectCommandesVenteState,
  selectCommandesVenteLoading,
  selectCommandesVenteError,
  selectSelectedCommandeVenteId,
  selectCommandesVenteFilter,
  selectCommandesVentePagination,
};

/**
 * Select all commandes vente from the store
 */
export const selectAllCommandesVente = createSelector(
  selectCommandesVenteState,
  selectAllCommandesVenteFromAdapter
);

/**
 * Select commande vente entities dictionary
 */
export const selectCommandesVenteDictionary = createSelector(
  selectCommandesVenteState,
  selectCommandeVenteEntities
);

/**
 * Select total number of commandes vente in store
 */
export const selectCommandesVenteCount = createSelector(
  selectCommandesVenteState,
  selectTotalCommandesVente
);

/**
 * Select a single commande vente by code
 */
export const selectCommandeVenteByCode = (codeCommande: string) =>
  createSelector(
    selectCommandesVenteDictionary,
    (entities) => entities[codeCommande] ?? null
  );

/**
 * Select currently selected commande vente
 */
export const selectSelectedCommandeVente = createSelector(
  selectCommandesVenteDictionary,
  selectSelectedCommandeVenteId,
  (entities, selectedId) => (selectedId ? entities[selectedId] ?? null : null)
);

/**
 * Select commandes vente list view model
 */
export const selectCommandesVenteListViewModel = createSelector(
  selectAllCommandesVente,
  selectCommandesVenteLoading,
  selectCommandesVenteError,
  selectCommandesVentePagination,
  selectCommandesVenteFilter,
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
 * Select commande vente detail view model
 */
export const selectCommandeVenteDetailViewModel = createSelector(
  selectSelectedCommandeVente,
  selectCommandesVenteLoading,
  selectCommandesVenteError,
  (commande, loading, error) => ({
    commande,
    loading,
    error,
    exists: commande !== null,
  })
);
