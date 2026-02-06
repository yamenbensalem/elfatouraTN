import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, tap, withLatestFrom, switchMap } from 'rxjs/operators';

import { ClientsService } from '../../core/services/clients.service';
import { ClientsPageActions, ClientsApiActions } from './clients.actions';
import { selectClientsFilter, selectClientsPagination } from './clients.reducer';

/**
 * Clients effects for handling side effects
 */
@Injectable()
export class ClientsEffects {
  private readonly actions$ = inject(Actions);
  private readonly clientsService = inject(ClientsService);
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  /**
   * Load clients list
   */
  loadClients$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ClientsPageActions.loadClients),
      withLatestFrom(
        this.store.select(selectClientsFilter),
        this.store.select(selectClientsPagination)
      ),
      switchMap(([action, filter, pagination]) => {
        const params = action.params ?? {
          pageNumber: pagination.pageNumber,
          pageSize: pagination.pageSize,
        };

        return this.clientsService.getAll({ ...params, filter }).pipe(
          map((response) => ClientsApiActions.loadClientsSuccess({ response })),
          catchError((error) =>
            of(ClientsApiActions.loadClientsFailure({
              error: error.error?.message || 'Erreur lors du chargement des clients'
            }))
          )
        );
      })
    )
  );

  /**
   * Load single client
   */
  loadClient$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ClientsPageActions.loadClient),
      switchMap(({ codeClient }) =>
        this.clientsService.getById(codeClient).pipe(
          map((client) => ClientsApiActions.loadClientSuccess({ client })),
          catchError((error) =>
            of(ClientsApiActions.loadClientFailure({
              error: error.error?.message || 'Erreur lors du chargement du client'
            }))
          )
        )
      )
    )
  );

  /**
   * Create client
   */
  createClient$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ClientsPageActions.createClient),
      exhaustMap(({ client }) =>
        this.clientsService.create(client).pipe(
          map((createdClient) => ClientsApiActions.createClientSuccess({ client: createdClient })),
          catchError((error) =>
            of(ClientsApiActions.createClientFailure({
              error: error.error?.message || 'Erreur lors de la création du client'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on create
   */
  createClientSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(ClientsApiActions.createClientSuccess),
        tap(({ client }) => {
          this.snackBar.open(
            `Client "${client.raisonSociale}" créé avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Update client
   */
  updateClient$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ClientsPageActions.updateClient),
      exhaustMap(({ codeClient, client }) =>
        this.clientsService.update(codeClient, client).pipe(
          map((updatedClient) => ClientsApiActions.updateClientSuccess({ client: updatedClient })),
          catchError((error) =>
            of(ClientsApiActions.updateClientFailure({
              error: error.error?.message || 'Erreur lors de la mise à jour du client'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on update
   */
  updateClientSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(ClientsApiActions.updateClientSuccess),
        tap(({ client }) => {
          this.snackBar.open(
            `Client "${client.raisonSociale}" mis à jour avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Delete client
   */
  deleteClient$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ClientsPageActions.deleteClient),
      exhaustMap(({ codeClient }) =>
        this.clientsService.delete(codeClient).pipe(
          map(() => ClientsApiActions.deleteClientSuccess({ codeClient })),
          catchError((error) =>
            of(ClientsApiActions.deleteClientFailure({
              error: error.error?.message || 'Erreur lors de la suppression du client'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on delete
   */
  deleteClientSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(ClientsApiActions.deleteClientSuccess),
        tap(() => {
          this.snackBar.open(
            'Client supprimé avec succès',
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Reload clients on filter change
   */
  reloadOnFilterChange$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ClientsPageActions.setFilter, ClientsPageActions.clearFilter, ClientsPageActions.setPage),
      map(() => ClientsPageActions.loadClients({}))
    )
  );

  /**
   * Show error messages
   */
  showErrors$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          ClientsApiActions.loadClientsFailure,
          ClientsApiActions.loadClientFailure,
          ClientsApiActions.createClientFailure,
          ClientsApiActions.updateClientFailure,
          ClientsApiActions.deleteClientFailure
        ),
        tap(({ error }) => {
          this.snackBar.open(error, 'Fermer', {
            duration: 5000,
            panelClass: 'snackbar-error',
          });
        })
      ),
    { dispatch: false }
  );
}
