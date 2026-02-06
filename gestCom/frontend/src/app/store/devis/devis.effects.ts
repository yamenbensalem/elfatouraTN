import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, tap, withLatestFrom, switchMap } from 'rxjs/operators';

import { DevisService } from '../../core/services/devis.service';
import { DevisPageActions, DevisApiActions } from './devis.actions';
import { selectDevisFilter, selectDevisPagination } from './devis.reducer';

/**
 * Devis effects for handling side effects
 */
@Injectable()
export class DevisEffects {
  private readonly actions$ = inject(Actions);
  private readonly devisService = inject(DevisService);
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  /**
   * Load devis list
   */
  loadDevisList$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DevisPageActions.loadDevisList),
      withLatestFrom(
        this.store.select(selectDevisFilter),
        this.store.select(selectDevisPagination)
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

        return this.devisService.getAll(queryParams).pipe(
          map((response) => DevisApiActions.loadDevisListSuccess({ response })),
          catchError((error) =>
            of(DevisApiActions.loadDevisListFailure({
              error: error.error?.message || 'Erreur lors du chargement des devis'
            }))
          )
        );
      })
    )
  );

  /**
   * Load single devis
   */
  loadDevis$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DevisPageActions.loadDevis),
      switchMap(({ codeDevis }) =>
        this.devisService.getById(codeDevis).pipe(
          map((devis) => DevisApiActions.loadDevisSuccess({ devis })),
          catchError((error) =>
            of(DevisApiActions.loadDevisFailure({
              error: error.error?.message || 'Erreur lors du chargement du devis'
            }))
          )
        )
      )
    )
  );

  /**
   * Create devis
   */
  createDevis$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DevisPageActions.createDevis),
      exhaustMap(({ devis }) =>
        this.devisService.create(devis).pipe(
          map((createdDevis) => DevisApiActions.createDevisSuccess({ devis: createdDevis })),
          catchError((error) =>
            of(DevisApiActions.createDevisFailure({
              error: error.error?.message || 'Erreur lors de la création du devis'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on create
   */
  createDevisSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(DevisApiActions.createDevisSuccess),
        tap(({ devis }) => {
          this.snackBar.open(
            `Devis "${devis.codeDevis}" créé avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Update devis
   */
  updateDevis$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DevisPageActions.updateDevis),
      exhaustMap(({ codeDevis, devis }) =>
        this.devisService.update(codeDevis, devis).pipe(
          map((updatedDevis) => DevisApiActions.updateDevisSuccess({ devis: updatedDevis })),
          catchError((error) =>
            of(DevisApiActions.updateDevisFailure({
              error: error.error?.message || 'Erreur lors de la mise à jour du devis'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on update
   */
  updateDevisSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(DevisApiActions.updateDevisSuccess),
        tap(({ devis }) => {
          this.snackBar.open(
            `Devis "${devis.codeDevis}" mis à jour avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Delete devis
   */
  deleteDevis$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DevisPageActions.deleteDevis),
      exhaustMap(({ codeDevis }) =>
        this.devisService.delete(codeDevis).pipe(
          map(() => DevisApiActions.deleteDevisSuccess({ codeDevis })),
          catchError((error) =>
            of(DevisApiActions.deleteDevisFailure({
              error: error.error?.message || 'Erreur lors de la suppression du devis'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on delete
   */
  deleteDevisSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(DevisApiActions.deleteDevisSuccess),
        tap(() => {
          this.snackBar.open(
            'Devis supprimé avec succès',
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Reload devis on filter change
   */
  reloadOnFilterChange$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DevisPageActions.setFilter, DevisPageActions.clearFilter, DevisPageActions.setPage),
      map(() => DevisPageActions.loadDevisList({}))
    )
  );

  /**
   * Show error messages
   */
  showErrors$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          DevisApiActions.loadDevisListFailure,
          DevisApiActions.loadDevisFailure,
          DevisApiActions.createDevisFailure,
          DevisApiActions.updateDevisFailure,
          DevisApiActions.deleteDevisFailure
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
