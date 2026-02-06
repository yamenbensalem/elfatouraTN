import { createFeature, createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Client, ClientsFilter, PaginationParams } from '../../core/models/client.model';
import { ClientsPageActions, ClientsApiActions } from './clients.actions';

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
 * Clients state interface extending EntityState
 */
export interface ClientsState extends EntityState<Client> {
  loading: boolean;
  error: string | null;
  selectedClientId: string | null;
  filter: ClientsFilter;
  pagination: PaginationState;
}

/**
 * Entity adapter for Client using codeClient as ID
 */
export const clientsAdapter: EntityAdapter<Client> = createEntityAdapter<Client>({
  selectId: (client: Client) => client.codeClient,
  sortComparer: (a, b) => (a.nom || '').localeCompare(b.nom || ''),
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
 * Initial clients state
 */
export const initialClientsState: ClientsState = clientsAdapter.getInitialState({
  loading: false,
  error: null,
  selectedClientId: null,
  filter: {},
  pagination: initialPagination,
});

/**
 * Clients feature using createFeature
 */
export const clientsFeature = createFeature({
  name: 'clients',
  reducer: createReducer(
    initialClientsState,

    // Load Clients
    on(ClientsPageActions.loadClients, (state): ClientsState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(ClientsApiActions.loadClientsSuccess, (state, { response }): ClientsState =>
      clientsAdapter.setAll(response.items, {
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

    on(ClientsApiActions.loadClientsFailure, (state, { error }): ClientsState => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Single Client
    on(ClientsPageActions.loadClient, (state): ClientsState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(ClientsApiActions.loadClientSuccess, (state, { client }): ClientsState =>
      clientsAdapter.upsertOne(client, {
        ...state,
        loading: false,
        error: null,
        selectedClientId: client.codeClient,
      })
    ),

    on(ClientsApiActions.loadClientFailure, (state, { error }): ClientsState => ({
      ...state,
      loading: false,
      error,
    })),

    // Create Client
    on(ClientsPageActions.createClient, (state): ClientsState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(ClientsApiActions.createClientSuccess, (state, { client }): ClientsState =>
      clientsAdapter.addOne(client, {
        ...state,
        loading: false,
        error: null,
        pagination: {
          ...state.pagination,
          totalCount: state.pagination.totalCount + 1,
        },
      })
    ),

    on(ClientsApiActions.createClientFailure, (state, { error }): ClientsState => ({
      ...state,
      loading: false,
      error,
    })),

    // Update Client
    on(ClientsPageActions.updateClient, (state): ClientsState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(ClientsApiActions.updateClientSuccess, (state, { client }): ClientsState =>
      clientsAdapter.updateOne(
        { id: client.codeClient, changes: client },
        {
          ...state,
          loading: false,
          error: null,
        }
      )
    ),

    on(ClientsApiActions.updateClientFailure, (state, { error }): ClientsState => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Client
    on(ClientsPageActions.deleteClient, (state): ClientsState => ({
      ...state,
      loading: true,
      error: null,
    })),

    on(ClientsApiActions.deleteClientSuccess, (state, { codeClient }): ClientsState =>
      clientsAdapter.removeOne(codeClient, {
        ...state,
        loading: false,
        error: null,
        selectedClientId:
          state.selectedClientId === codeClient ? null : state.selectedClientId,
        pagination: {
          ...state.pagination,
          totalCount: Math.max(0, state.pagination.totalCount - 1),
        },
      })
    ),

    on(ClientsApiActions.deleteClientFailure, (state, { error }): ClientsState => ({
      ...state,
      loading: false,
      error,
    })),

    // Select Client
    on(ClientsPageActions.selectClient, (state, { codeClient }): ClientsState => ({
      ...state,
      selectedClientId: codeClient,
    })),

    on(ClientsPageActions.clearSelection, (state): ClientsState => ({
      ...state,
      selectedClientId: null,
    })),

    // Filter
    on(ClientsPageActions.setFilter, (state, { filter }): ClientsState => ({
      ...state,
      filter,
      pagination: {
        ...state.pagination,
        pageNumber: 1, // Reset to first page on filter change
      },
    })),

    on(ClientsPageActions.clearFilter, (state): ClientsState => ({
      ...state,
      filter: {},
      pagination: {
        ...state.pagination,
        pageNumber: 1,
      },
    })),

    // Pagination
    on(ClientsPageActions.setPage, (state, { pageNumber, pageSize }): ClientsState => ({
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
  name: clientsFeatureKey,
  reducer: clientsReducer,
  selectClientsState,
  selectLoading: selectClientsLoading,
  selectError: selectClientsError,
  selectSelectedClientId,
  selectFilter: selectClientsFilter,
  selectPagination: selectClientsPagination,
} = clientsFeature;

/**
 * Entity selectors (private - use selectors from clients.selectors.ts)
 */
const entitySelectors = clientsAdapter.getSelectors();
export const selectClientIds = entitySelectors.selectIds;
export const selectClientEntities = entitySelectors.selectEntities;
export const selectAllClientsFromAdapter = entitySelectors.selectAll;
export const selectTotalClients = entitySelectors.selectTotal;
