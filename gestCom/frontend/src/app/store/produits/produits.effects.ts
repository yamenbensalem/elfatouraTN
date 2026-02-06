import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, tap, withLatestFrom, switchMap } from 'rxjs/operators';

import { ProduitsService } from '../../core/services/produits.service';
import { ProduitsPageActions, ProduitsApiActions } from './produits.actions';
import { selectProduitsFilter, selectProduitsPagination } from './produits.reducer';

/**
 * Produits effects for handling side effects
 */
@Injectable()
export class ProduitsEffects {
  private readonly actions$ = inject(Actions);
  private readonly produitsService = inject(ProduitsService);
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  /**
   * Load produits list
   */
  loadProduits$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProduitsPageActions.loadProduits),
      withLatestFrom(
        this.store.select(selectProduitsFilter),
        this.store.select(selectProduitsPagination)
      ),
      switchMap(([action, filter, pagination]) => {
        const params = action.params ?? {
          pageNumber: pagination.pageNumber,
          pageSize: pagination.pageSize,
        };

        return this.produitsService.getAll({ ...params, filter }).pipe(
          map((response) => ProduitsApiActions.loadProduitsSuccess({ response })),
          catchError((error) =>
            of(ProduitsApiActions.loadProduitsFailure({
              error: error.error?.message || 'Erreur lors du chargement des produits'
            }))
          )
        );
      })
    )
  );

  /**
   * Load single produit
   */
  loadProduit$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProduitsPageActions.loadProduit),
      switchMap(({ codeProduit }) =>
        this.produitsService.getById(codeProduit).pipe(
          map((produit) => ProduitsApiActions.loadProduitSuccess({ produit })),
          catchError((error) =>
            of(ProduitsApiActions.loadProduitFailure({
              error: error.error?.message || 'Erreur lors du chargement du produit'
            }))
          )
        )
      )
    )
  );

  /**
   * Create produit
   */
  createProduit$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProduitsPageActions.createProduit),
      exhaustMap(({ produit }) =>
        this.produitsService.create(produit).pipe(
          map((createdProduit) => ProduitsApiActions.createProduitSuccess({ produit: createdProduit })),
          catchError((error) =>
            of(ProduitsApiActions.createProduitFailure({
              error: error.error?.message || 'Erreur lors de la création du produit'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on create
   */
  createProduitSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(ProduitsApiActions.createProduitSuccess),
        tap(({ produit }) => {
          this.snackBar.open(
            `Produit "${produit.designation}" créé avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Update produit
   */
  updateProduit$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProduitsPageActions.updateProduit),
      exhaustMap(({ codeProduit, produit }) =>
        this.produitsService.update(codeProduit, produit).pipe(
          map((updatedProduit) => ProduitsApiActions.updateProduitSuccess({ produit: updatedProduit })),
          catchError((error) =>
            of(ProduitsApiActions.updateProduitFailure({
              error: error.error?.message || 'Erreur lors de la mise à jour du produit'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on update
   */
  updateProduitSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(ProduitsApiActions.updateProduitSuccess),
        tap(({ produit }) => {
          this.snackBar.open(
            `Produit "${produit.designation}" mis à jour avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Delete produit
   */
  deleteProduit$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProduitsPageActions.deleteProduit),
      exhaustMap(({ codeProduit }) =>
        this.produitsService.delete(codeProduit).pipe(
          map(() => ProduitsApiActions.deleteProduitSuccess({ codeProduit })),
          catchError((error) =>
            of(ProduitsApiActions.deleteProduitFailure({
              error: error.error?.message || 'Erreur lors de la suppression du produit'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on delete
   */
  deleteProduitSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(ProduitsApiActions.deleteProduitSuccess),
        tap(() => {
          this.snackBar.open(
            'Produit supprimé avec succès',
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Reload produits on filter change
   */
  reloadOnFilterChange$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProduitsPageActions.setFilter, ProduitsPageActions.clearFilter, ProduitsPageActions.setPage),
      map(() => ProduitsPageActions.loadProduits({}))
    )
  );

  /**
   * Show error messages
   */
  showErrors$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          ProduitsApiActions.loadProduitsFailure,
          ProduitsApiActions.loadProduitFailure,
          ProduitsApiActions.createProduitFailure,
          ProduitsApiActions.updateProduitFailure,
          ProduitsApiActions.deleteProduitFailure
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
