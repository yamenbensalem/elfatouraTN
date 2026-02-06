/**
 * Store barrel export
 * Central export point for all store-related modules
 */

// App state and reducers
export * from './app.state';
export * from './app.reducer';

// Auth feature
export * from './auth';

// Clients feature (PaginationState is exported from here)
export * from './clients';

// Devis feature - specific exports to avoid PaginationState conflict
export { DevisPageActions, DevisApiActions } from './devis/devis.actions';
export type { DevisState } from './devis/devis.reducer';
export {
  devisFeature,
  devisAdapter,
  initialDevisState,
  devisFeatureKey,
  devisReducer,
  selectDevisState,
  selectDevisLoading,
  selectDevisError,
  selectSelectedDevisId,
  selectDevisFilter,
  selectDevisPagination,
  selectDevisIds,
  selectDevisEntities,
  selectAllDevisFromAdapter,
  selectTotalDevis,
} from './devis/devis.reducer';
export { DevisEffects } from './devis/devis.effects';
export {
  selectAllDevis,
  selectDevisDictionary,
  selectDevisCount,
  selectDevisByCode,
  selectSelectedDevis,
  selectDevisListViewModel,
  selectDevisDetailViewModel,
  selectDevisForDropdown,
  selectDevisPaginationInfo,
  selectHasDevis,
} from './devis/devis.selectors';

// Factures Client feature - specific exports to avoid PaginationState conflict
export { FacturesClientPageActions, FacturesClientApiActions } from './factures-client/factures-client.actions';
export type { FacturesClientState } from './factures-client/factures-client.reducer';
export {
  facturesClientFeature,
  facturesClientAdapter,
  initialFacturesClientState,
  facturesClientFeatureKey,
  facturesClientReducer,
  selectFacturesClientState,
  selectFacturesClientLoading,
  selectFacturesClientError,
  selectSelectedFactureClientId,
  selectFacturesClientFilter,
  selectFacturesClientPagination,
  selectFactureClientIds,
  selectFactureClientEntities,
  selectAllFacturesClientFromAdapter,
  selectTotalFacturesClient,
} from './factures-client/factures-client.reducer';
export { FacturesClientEffects } from './factures-client/factures-client.effects';
export {
  selectAllFacturesClient,
  selectFacturesClientDictionary,
  selectFacturesClientCount,
  selectFactureClientByCode,
  selectSelectedFactureClient,
  selectFacturesClientListViewModel,
  selectFactureClientDetailViewModel,
  selectFacturesClientForDropdown,
  selectFacturesClientPaginationInfo,
  selectHasFacturesClient,
} from './factures-client/factures-client.selectors';
