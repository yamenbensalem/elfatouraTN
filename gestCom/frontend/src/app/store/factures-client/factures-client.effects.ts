import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, tap, withLatestFrom, switchMap } from 'rxjs/operators';

import { FacturesClientService } from '../../core/services/factures-client.service';
import { FacturesClientPageActions, FacturesClientApiActions } from './factures-client.actions';
import { selectFacturesClientFilter, selectFacturesClientPagination } from './factures-client.reducer';

/**
 * Factures Client effects for handling side effects
 */
@Injectable()
export class FacturesClientEffects {
  private readonly actions$ = inject(Actions);
  private readonly facturesClientService = inject(FacturesClientService);
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  /**
   * Load factures client list
   */
  loadFacturesClient$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesClientPageActions.loadFacturesClient),
      withLatestFrom(
        this.store.select(selectFacturesClientFilter),
        this.store.select(selectFacturesClientPagination)
      ),
      switchMap(([action, filter, pagination]) => {
        const queryParams: { [param: string]: string | string[] } = {};
        const params = action.params ?? {
          pageNumber: pagination.pageNumber,
          pageSize: pagination.pageSize,
        };

        if (params.pageNumber) queryParams['pageNumber'] = params.pageNumber.toString();
        if (params.pageSize) queryParams['pageSize'] = params.pageSize.toString();
        if (params.sortBy) queryParams['sortBy'] = params.sortBy;
        if (params.sortDescending !== undefined) queryParams['sortDescending'] = params.sortDescending.toString();

        if (filter) {
          if (filter.search) queryParams['search'] = filter.search;
          if (filter.codeClient) queryParams['codeClient'] = filter.codeClient;
          if (filter.statut) queryParams['statut'] = filter.statut;
        }

        return this.facturesClientService.getAll(queryParams).pipe(
          map((response) => FacturesClientApiActions.loadFacturesClientSuccess({ response })),
          catchError((error) =>
            of(FacturesClientApiActions.loadFacturesClientFailure({
              error: error.error?.message || 'Erreur lors du chargement des factures client'
            }))
          )
        );
      })
    )
  );

  /**
   * Load single facture client
   */
  loadFactureClient$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesClientPageActions.loadFactureClient),
      switchMap(({ codeFacture }) =>
        this.facturesClientService.getById(codeFacture).pipe(
          map((facture) => FacturesClientApiActions.loadFactureClientSuccess({ facture })),
          catchError((error) =>
            of(FacturesClientApiActions.loadFactureClientFailure({
              error: error.error?.message || 'Erreur lors du chargement de la facture client'
            }))
          )
        )
      )
    )
  );

  /**
   * Create facture client
   */
  createFactureClient$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesClientPageActions.createFactureClient),
      exhaustMap(({ facture }) =>
        this.facturesClientService.create(facture).pipe(
          map((createdFacture) => FacturesClientApiActions.createFactureClientSuccess({ facture: createdFacture })),
          catchError((error) =>
            of(FacturesClientApiActions.createFactureClientFailure({
              error: error.error?.message || 'Erreur lors de la création de la facture client'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on create
   */
  createFactureClientSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FacturesClientApiActions.createFactureClientSuccess),
        tap(({ facture }) => {
          this.snackBar.open(
            `Facture "${facture.codeFacture}" créée avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Update facture client
   */
  updateFactureClient$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesClientPageActions.updateFactureClient),
      exhaustMap(({ codeFacture, facture }) =>
        this.facturesClientService.update(codeFacture, facture).pipe(
          map((updatedFacture) => FacturesClientApiActions.updateFactureClientSuccess({ facture: updatedFacture })),
          catchError((error) =>
            of(FacturesClientApiActions.updateFactureClientFailure({
              error: error.error?.message || 'Erreur lors de la mise à jour de la facture client'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on update
   */
  updateFactureClientSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FacturesClientApiActions.updateFactureClientSuccess),
        tap(({ facture }) => {
          this.snackBar.open(
            `Facture "${facture.codeFacture}" mise à jour avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Delete facture client
   */
  deleteFactureClient$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesClientPageActions.deleteFactureClient),
      exhaustMap(({ codeFacture }) =>
        this.facturesClientService.delete(codeFacture).pipe(
          map(() => FacturesClientApiActions.deleteFactureClientSuccess({ codeFacture })),
          catchError((error) =>
            of(FacturesClientApiActions.deleteFactureClientFailure({
              error: error.error?.message || 'Erreur lors de la suppression de la facture client'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on delete
   */
  deleteFactureClientSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FacturesClientApiActions.deleteFactureClientSuccess),
        tap(() => {
          this.snackBar.open(
            'Facture client supprimée avec succès',
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Reload factures client on filter change
   */
  reloadOnFilterChange$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesClientPageActions.setFilter, FacturesClientPageActions.clearFilter, FacturesClientPageActions.setPage),
      map(() => FacturesClientPageActions.loadFacturesClient({}))
    )
  );

  /**
   * Show error messages
   */
  showErrors$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          FacturesClientApiActions.loadFacturesClientFailure,
          FacturesClientApiActions.loadFactureClientFailure,
          FacturesClientApiActions.createFactureClientFailure,
          FacturesClientApiActions.updateFactureClientFailure,
          FacturesClientApiActions.deleteFactureClientFailure
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
