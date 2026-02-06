import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, tap, withLatestFrom, switchMap } from 'rxjs/operators';

import { FournisseursService } from '../../core/services/fournisseurs.service';
import { FournisseursPageActions, FournisseursApiActions } from './fournisseurs.actions';
import { selectFournisseursFilter, selectFournisseursPagination } from './fournisseurs.reducer';

/**
 * Fournisseurs effects for handling side effects
 */
@Injectable()
export class FournisseursEffects {
  private readonly actions$ = inject(Actions);
  private readonly fournisseursService = inject(FournisseursService);
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  /**
   * Load fournisseurs list
   */
  loadFournisseurs$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FournisseursPageActions.loadFournisseurs),
      withLatestFrom(
        this.store.select(selectFournisseursFilter),
        this.store.select(selectFournisseursPagination)
      ),
      switchMap(([action, filter, pagination]) => {
        const params = action.params ?? {
          pageNumber: pagination.pageNumber,
          pageSize: pagination.pageSize,
        };

        return this.fournisseursService.getAll({ ...params, filter }).pipe(
          map((response) => FournisseursApiActions.loadFournisseursSuccess({ response })),
          catchError((error) =>
            of(FournisseursApiActions.loadFournisseursFailure({
              error: error.error?.message || 'Erreur lors du chargement des fournisseurs'
            }))
          )
        );
      })
    )
  );

  /**
   * Load single fournisseur
   */
  loadFournisseur$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FournisseursPageActions.loadFournisseur),
      switchMap(({ codeFournisseur }) =>
        this.fournisseursService.getById(codeFournisseur).pipe(
          map((fournisseur) => FournisseursApiActions.loadFournisseurSuccess({ fournisseur })),
          catchError((error) =>
            of(FournisseursApiActions.loadFournisseurFailure({
              error: error.error?.message || 'Erreur lors du chargement du fournisseur'
            }))
          )
        )
      )
    )
  );

  /**
   * Create fournisseur
   */
  createFournisseur$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FournisseursPageActions.createFournisseur),
      exhaustMap(({ fournisseur }) =>
        this.fournisseursService.create(fournisseur).pipe(
          map((createdFournisseur) => FournisseursApiActions.createFournisseurSuccess({ fournisseur: createdFournisseur })),
          catchError((error) =>
            of(FournisseursApiActions.createFournisseurFailure({
              error: error.error?.message || 'Erreur lors de la création du fournisseur'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on create
   */
  createFournisseurSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FournisseursApiActions.createFournisseurSuccess),
        tap(({ fournisseur }) => {
          this.snackBar.open(
            `Fournisseur "${fournisseur.raisonSociale}" créé avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Update fournisseur
   */
  updateFournisseur$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FournisseursPageActions.updateFournisseur),
      exhaustMap(({ codeFournisseur, fournisseur }) =>
        this.fournisseursService.update(codeFournisseur, fournisseur).pipe(
          map((updatedFournisseur) => FournisseursApiActions.updateFournisseurSuccess({ fournisseur: updatedFournisseur })),
          catchError((error) =>
            of(FournisseursApiActions.updateFournisseurFailure({
              error: error.error?.message || 'Erreur lors de la mise à jour du fournisseur'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on update
   */
  updateFournisseurSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FournisseursApiActions.updateFournisseurSuccess),
        tap(({ fournisseur }) => {
          this.snackBar.open(
            `Fournisseur "${fournisseur.raisonSociale}" mis à jour avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Delete fournisseur
   */
  deleteFournisseur$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FournisseursPageActions.deleteFournisseur),
      exhaustMap(({ codeFournisseur }) =>
        this.fournisseursService.delete(codeFournisseur).pipe(
          map(() => FournisseursApiActions.deleteFournisseurSuccess({ codeFournisseur })),
          catchError((error) =>
            of(FournisseursApiActions.deleteFournisseurFailure({
              error: error.error?.message || 'Erreur lors de la suppression du fournisseur'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on delete
   */
  deleteFournisseurSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FournisseursApiActions.deleteFournisseurSuccess),
        tap(() => {
          this.snackBar.open(
            'Fournisseur supprimé avec succès',
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Reload fournisseurs on filter change
   */
  reloadOnFilterChange$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FournisseursPageActions.setFilter, FournisseursPageActions.clearFilter, FournisseursPageActions.setPage),
      map(() => FournisseursPageActions.loadFournisseurs({}))
    )
  );

  /**
   * Show error messages
   */
  showErrors$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          FournisseursApiActions.loadFournisseursFailure,
          FournisseursApiActions.loadFournisseurFailure,
          FournisseursApiActions.createFournisseurFailure,
          FournisseursApiActions.updateFournisseurFailure,
          FournisseursApiActions.deleteFournisseurFailure
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
