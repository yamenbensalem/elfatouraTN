import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, tap, withLatestFrom, switchMap } from 'rxjs/operators';

import { CommandesVenteService } from '../../core/services/commandes-vente.service';
import { CommandesVentePageActions, CommandesVenteApiActions } from './commandes-vente.actions';
import { selectCommandesVenteFilter, selectCommandesVentePagination } from './commandes-vente.reducer';

/**
 * Commandes Vente effects for handling side effects
 */
@Injectable()
export class CommandesVenteEffects {
  private readonly actions$ = inject(Actions);
  private readonly commandesVenteService = inject(CommandesVenteService);
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  /**
   * Load commandes vente list
   */
  loadCommandesVente$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommandesVentePageActions.loadCommandesVente),
      withLatestFrom(
        this.store.select(selectCommandesVenteFilter),
        this.store.select(selectCommandesVentePagination)
      ),
      switchMap(([action, filter, pagination]) => {
        const params = action.params ?? {
          pageNumber: pagination.pageNumber,
          pageSize: pagination.pageSize,
        };

        return this.commandesVenteService.getAll({ ...params, filter }).pipe(
          map((response) => CommandesVenteApiActions.loadCommandesVenteSuccess({ response })),
          catchError((error) =>
            of(CommandesVenteApiActions.loadCommandesVenteFailure({
              error: error.error?.message || 'Erreur lors du chargement des commandes de vente'
            }))
          )
        );
      })
    )
  );

  /**
   * Load single commande vente
   */
  loadCommandeVente$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommandesVentePageActions.loadCommandeVente),
      switchMap(({ codeCommande }) =>
        this.commandesVenteService.getById(codeCommande).pipe(
          map((commande) => CommandesVenteApiActions.loadCommandeVenteSuccess({ commande })),
          catchError((error) =>
            of(CommandesVenteApiActions.loadCommandeVenteFailure({
              error: error.error?.message || 'Erreur lors du chargement de la commande de vente'
            }))
          )
        )
      )
    )
  );

  /**
   * Create commande vente
   */
  createCommandeVente$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommandesVentePageActions.createCommandeVente),
      exhaustMap(({ commande }) =>
        this.commandesVenteService.create(commande).pipe(
          map((createdCommande) => CommandesVenteApiActions.createCommandeVenteSuccess({ commande: createdCommande })),
          catchError((error) =>
            of(CommandesVenteApiActions.createCommandeVenteFailure({
              error: error.error?.message || 'Erreur lors de la création de la commande de vente'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on create
   */
  createCommandeVenteSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CommandesVenteApiActions.createCommandeVenteSuccess),
        tap(({ commande }) => {
          this.snackBar.open(
            `Commande de vente "${commande.codeCommande}" créée avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Update commande vente
   */
  updateCommandeVente$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommandesVentePageActions.updateCommandeVente),
      exhaustMap(({ codeCommande, commande }) =>
        this.commandesVenteService.update(codeCommande, commande).pipe(
          map((updatedCommande) => CommandesVenteApiActions.updateCommandeVenteSuccess({ commande: updatedCommande })),
          catchError((error) =>
            of(CommandesVenteApiActions.updateCommandeVenteFailure({
              error: error.error?.message || 'Erreur lors de la mise à jour de la commande de vente'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on update
   */
  updateCommandeVenteSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CommandesVenteApiActions.updateCommandeVenteSuccess),
        tap(({ commande }) => {
          this.snackBar.open(
            `Commande de vente "${commande.codeCommande}" mise à jour avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Delete commande vente
   */
  deleteCommandeVente$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommandesVentePageActions.deleteCommandeVente),
      exhaustMap(({ codeCommande }) =>
        this.commandesVenteService.delete(codeCommande).pipe(
          map(() => CommandesVenteApiActions.deleteCommandeVenteSuccess({ codeCommande })),
          catchError((error) =>
            of(CommandesVenteApiActions.deleteCommandeVenteFailure({
              error: error.error?.message || 'Erreur lors de la suppression de la commande de vente'
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on delete
   */
  deleteCommandeVenteSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CommandesVenteApiActions.deleteCommandeVenteSuccess),
        tap(() => {
          this.snackBar.open(
            'Commande de vente supprimée avec succès',
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
      ofType(CommandesVentePageActions.setFilter, CommandesVentePageActions.clearFilter, CommandesVentePageActions.setPage),
      map(() => CommandesVentePageActions.loadCommandesVente({}))
    )
  );

  /**
   * Show error messages
   */
  showErrors$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          CommandesVenteApiActions.loadCommandesVenteFailure,
          CommandesVenteApiActions.loadCommandeVenteFailure,
          CommandesVenteApiActions.createCommandeVenteFailure,
          CommandesVenteApiActions.updateCommandeVenteFailure,
          CommandesVenteApiActions.deleteCommandeVenteFailure
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
