import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, tap, withLatestFrom, switchMap } from 'rxjs/operators';

import { FacturesFournisseurService } from '../../core/services/factures-fournisseur.service';
import { FacturesFournisseurPageActions, FacturesFournisseurApiActions } from './factures-fournisseur.actions';
import { selectFacturesFournisseurFilter, selectFacturesFournisseurPagination } from './factures-fournisseur.reducer';

/**
 * Factures Fournisseur effects for handling side effects
 */
@Injectable()
export class FacturesFournisseurEffects {
  private readonly actions$ = inject(Actions);
  private readonly facturesFournisseurService = inject(FacturesFournisseurService);
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  /**
   * Load factures fournisseur list
   */
  loadFacturesFournisseur$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesFournisseurPageActions.loadFacturesFournisseur),
      withLatestFrom(
        this.store.select(selectFacturesFournisseurFilter),
        this.store.select(selectFacturesFournisseurPagination)
      ),
      switchMap(([action, filter, pagination]) => {
        const params = action.params ?? {
          pageNumber: pagination.pageNumber,
          pageSize: pagination.pageSize,
        };

        return this.facturesFournisseurService.getAll({ ...params, filter }).pipe(
          map((response) => FacturesFournisseurApiActions.loadFacturesFournisseurSuccess({ response })),
          catchError((error) =>
            of(FacturesFournisseurApiActions.loadFacturesFournisseurFailure({
              error: error.error?.message || 'Erreur lors du chargement des factures fournisseur'
            }))
          )
        );
      })
    )
  );

  /**
   * Load single facture fournisseur
   */
  loadFactureFournisseur$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesFournisseurPageActions.loadFactureFournisseur),
      switchMap(({ codeFacture }) =>
        this.facturesFournisseurService.getById(codeFacture).pipe(
          map((facture) => FacturesFournisseurApiActions.loadFactureFournisseurSuccess({ facture })),
          catchError((error) =>
            of(FacturesFournisseurApiActions.loadFactureFournisseurFailure({
              error: error.error?.message || 'Erreur lors du chargement de la facture fournisseur'
            }))
          )
        )
      )
    )
  );

  /**
   * Create facture fournisseur
   */
  createFactureFournisseur$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesFournisseurPageActions.createFactureFournisseur),
      exhaustMap(({ facture }) =>
        this.facturesFournisseurService.create(facture).pipe(
          map((createdFacture) => FacturesFournisseurApiActions.createFactureFournisseurSuccess({ facture: createdFacture })),
          catchError((error) =>
            of(FacturesFournisseurApiActions.createFactureFournisseurFailure({
              error: error.error?.message || 'Erreur lors de la création de la facture fournisseur'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on create
   */
  createFactureFournisseurSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FacturesFournisseurApiActions.createFactureFournisseurSuccess),
        tap(({ facture }) => {
          this.snackBar.open(
            `Facture fournisseur "${facture.codeFacture}" créée avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Update facture fournisseur
   */
  updateFactureFournisseur$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesFournisseurPageActions.updateFactureFournisseur),
      exhaustMap(({ codeFacture, facture }) =>
        this.facturesFournisseurService.update(codeFacture, facture).pipe(
          map((updatedFacture) => FacturesFournisseurApiActions.updateFactureFournisseurSuccess({ facture: updatedFacture })),
          catchError((error) =>
            of(FacturesFournisseurApiActions.updateFactureFournisseurFailure({
              error: error.error?.message || 'Erreur lors de la mise à jour de la facture fournisseur'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on update
   */
  updateFactureFournisseurSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FacturesFournisseurApiActions.updateFactureFournisseurSuccess),
        tap(({ facture }) => {
          this.snackBar.open(
            `Facture fournisseur "${facture.codeFacture}" mise à jour avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Delete facture fournisseur
   */
  deleteFactureFournisseur$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesFournisseurPageActions.deleteFactureFournisseur),
      exhaustMap(({ codeFacture }) =>
        this.facturesFournisseurService.delete(codeFacture).pipe(
          map(() => FacturesFournisseurApiActions.deleteFactureFournisseurSuccess({ codeFacture })),
          catchError((error) =>
            of(FacturesFournisseurApiActions.deleteFactureFournisseurFailure({
              error: error.error?.message || 'Erreur lors de la suppression de la facture fournisseur'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on delete
   */
  deleteFactureFournisseurSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FacturesFournisseurApiActions.deleteFactureFournisseurSuccess),
        tap(() => {
          this.snackBar.open(
            'Facture fournisseur supprimée avec succès',
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Reload on filter change
   */
  reloadOnFilterChange$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FacturesFournisseurPageActions.setFilter, FacturesFournisseurPageActions.clearFilter, FacturesFournisseurPageActions.setPage),
      map(() => FacturesFournisseurPageActions.loadFacturesFournisseur({}))
    )
  );

  /**
   * Show error messages
   */
  showErrors$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          FacturesFournisseurApiActions.loadFacturesFournisseurFailure,
          FacturesFournisseurApiActions.loadFactureFournisseurFailure,
          FacturesFournisseurApiActions.createFactureFournisseurFailure,
          FacturesFournisseurApiActions.updateFactureFournisseurFailure,
          FacturesFournisseurApiActions.deleteFactureFournisseurFailure
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
