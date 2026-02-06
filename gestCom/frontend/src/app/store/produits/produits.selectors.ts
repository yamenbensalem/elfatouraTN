import { createSelector } from '@ngrx/store';
import {
  produitsFeature,
  selectProduitsState,
  selectProduitsLoading,
  selectProduitsError,
  selectSelectedProduitId,
  selectProduitsFilter,
  selectProduitsPagination,
  selectAllProduitsFromAdapter,
  selectProduitEntities,
  selectTotalProduits,
} from './produits.reducer';

/**
 * Re-export feature selectors
 */
export {
  selectProduitsState,
  selectProduitsLoading,
  selectProduitsError,
  selectSelectedProduitId,
  selectProduitsFilter,
  selectProduitsPagination,
};

/**
 * Select all produits from the store
 */
export const selectAllProduits = createSelector(
  selectProduitsState,
  selectAllProduitsFromAdapter
);

/**
 * Select produit entities dictionary
 */
export const selectProduitsDictionary = createSelector(
  selectProduitsState,
  selectProduitEntities
);

/**
 * Select total number of produits in store
 */
export const selectProduitsCount = createSelector(
  selectProduitsState,
  selectTotalProduits
);

/**
 * Select a single produit by code
 */
export const selectProduitByCode = (codeProduit: string) =>
  createSelector(
    selectProduitsDictionary,
    (entities) => entities[codeProduit] ?? null
  );

/**
 * Select currently selected produit
 */
export const selectSelectedProduit = createSelector(
  selectProduitsDictionary,
  selectSelectedProduitId,
  (entities, selectedId) => (selectedId ? entities[selectedId] ?? null : null)
);

/**
 * Select produits list view model
 */
export const selectProduitsListViewModel = createSelector(
  selectAllProduits,
  selectProduitsLoading,
  selectProduitsError,
  selectProduitsPagination,
  selectProduitsFilter,
  (produits, loading, error, pagination, filter) => ({
    produits,
    loading,
    error,
    pagination,
    filter,
    isEmpty: produits.length === 0 && !loading,
    hasFilter: Object.keys(filter).some(key => filter[key as keyof typeof filter] !== undefined),
  })
);

/**
 * Select produit detail view model
 */
export const selectProduitDetailViewModel = createSelector(
  selectSelectedProduit,
  selectProduitsLoading,
  selectProduitsError,
  (produit, loading, error) => ({
    produit,
    loading,
    error,
    exists: produit !== null,
  })
);

/**
 * Select produits for dropdown/autocomplete
 */
export const selectProduitsForDropdown = createSelector(
  selectAllProduits,
  (produits) =>
    produits.map((produit) => ({
      value: produit.codeProduit,
      label: `${produit.codeProduit} - ${produit.designation}`,
      designation: produit.designation,
    }))
);

/**
 * Select active produits
 */
export const selectActiveProduits = createSelector(
  selectAllProduits,
  (produits) => produits.filter((produit) => produit.actif)
);

/**
 * Select low stock produits (stockActuel < stockMinimum)
 */
export const selectLowStockProduits = createSelector(
  selectAllProduits,
  (produits) =>
    produits.filter(
      (produit) => produit.stockActuel < produit.stockMinimum
    )
);

/**
 * Select pagination info for display
 */
export const selectPaginationInfo = createSelector(
  selectProduitsPagination,
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
 * Check if there are any produits
 */
export const selectHasProduits = createSelector(
  selectProduitsCount,
  (count) => count > 0
);

/**
 * Check if currently loading
 */
export const selectIsLoading = selectProduitsLoading;

/**
 * Check if there's an error
 */
export const selectHasError = createSelector(
  selectProduitsError,
  (error) => error !== null
);
