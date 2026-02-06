import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';
import { map, exhaustMap, catchError, tap, withLatestFrom, switchMap } from 'rxjs/operators';

import { CommandesAchatService } from '../../core/services/commandes-achat.service';
import { CommandesAchatPageActions, CommandesAchatApiActions } from './commandes-achat.actions';
import { selectCommandesAchatFilter, selectCommandesAchatPagination } from './commandes-achat.reducer';

/**
 * Commandes Achat effects for handling side effects
 */
@Injectable()
export class CommandesAchatEffects {
  private readonly actions$ = inject(Actions);
  private readonly commandesAchatService = inject(CommandesAchatService);
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  /**
   * Load commandes achat list
   */
  loadCommandesAchat$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommandesAchatPageActions.loadCommandesAchat),
      withLatestFrom(
        this.store.select(selectCommandesAchatFilter),
        this.store.select(selectCommandesAchatPagination)
      ),
      switchMap(([action, filter, pagination]) => {
        const params = action.params ?? {
          pageNumber: pagination.pageNumber,
          pageSize: pagination.pageSize,
        };

        return this.commandesAchatService.getAll({ ...params, filter }).pipe(
          map((response) => CommandesAchatApiActions.loadCommandesAchatSuccess({ response })),
          catchError((error) =>
            of(CommandesAchatApiActions.loadCommandesAchatFailure({
              error: error.error?.message || "Erreur lors du chargement des commandes d'achat"
            }))
          )
        );
      })
    )
  );

  /**
   * Load single commande achat
   */
  loadCommandeAchat$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommandesAchatPageActions.loadCommandeAchat),
      switchMap(({ codeCommande }) =>
        this.commandesAchatService.getById(codeCommande).pipe(
          map((commande) => CommandesAchatApiActions.loadCommandeAchatSuccess({ commande })),
          catchError((error) =>
            of(CommandesAchatApiActions.loadCommandeAchatFailure({
              error: error.error?.message || "Erreur lors du chargement de la commande d'achat"
            }))
          )
        )
      )
    )
  );

  /**
   * Create commande achat
   */
  createCommandeAchat$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommandesAchatPageActions.createCommandeAchat),
      exhaustMap(({ commande }) =>
        this.commandesAchatService.create(commande).pipe(
          map((createdCommande) => CommandesAchatApiActions.createCommandeAchatSuccess({ commande: createdCommande })),
          catchError((error) =>
            of(CommandesAchatApiActions.createCommandeAchatFailure({
              error: error.error?.message || "Erreur lors de la création de la commande d'achat"
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on create
   */
  createCommandeAchatSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CommandesAchatApiActions.createCommandeAchatSuccess),
        tap(({ commande }) => {
          this.snackBar.open(
            `Commande d'achat "${commande.codeCommande}" créée avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Update commande achat
   */
  updateCommandeAchat$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommandesAchatPageActions.updateCommandeAchat),
      exhaustMap(({ codeCommande, commande }) =>
        this.commandesAchatService.update(codeCommande, commande).pipe(
          map((updatedCommande) => CommandesAchatApiActions.updateCommandeAchatSuccess({ commande: updatedCommande })),
          catchError((error) =>
            of(CommandesAchatApiActions.updateCommandeAchatFailure({
              error: error.error?.message || "Erreur lors de la mise à jour de la commande d'achat"
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on update
   */
  updateCommandeAchatSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CommandesAchatApiActions.updateCommandeAchatSuccess),
        tap(({ commande }) => {
          this.snackBar.open(
            `Commande d'achat "${commande.codeCommande}" mise à jour avec succès`,
            'Fermer',
            { duration: 3000, panelClass: 'snackbar-success' }
          );
        })
      ),
    { dispatch: false }
  );

  /**
   * Delete commande achat
   */
  deleteCommandeAchat$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommandesAchatPageActions.deleteCommandeAchat),
      exhaustMap(({ codeCommande }) =>
        this.commandesAchatService.delete(codeCommande).pipe(
          map(() => CommandesAchatApiActions.deleteCommandeAchatSuccess({ codeCommande })),
          catchError((error) =>
            of(CommandesAchatApiActions.deleteCommandeAchatFailure({
              error: error.error?.message || "Erreur lors de la suppression de la commande d'achat"
            }))
          )
        )
      )
    )
  );

  /**
   * Show success message on delete
   */
  deleteCommandeAchatSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CommandesAchatApiActions.deleteCommandeAchatSuccess),
        tap(() => {
          this.snackBar.open(
            "Commande d'achat supprimée avec succès",
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
      ofType(CommandesAchatPageActions.setFilter, CommandesAchatPageActions.clearFilter, CommandesAchatPageActions.setPage),
      map(() => CommandesAchatPageActions.loadCommandesAchat({}))
    )
  );

  /**
   * Show error messages
   */
  showErrors$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          CommandesAchatApiActions.loadCommandesAchatFailure,
          CommandesAchatApiActions.loadCommandeAchatFailure,
          CommandesAchatApiActions.createCommandeAchatFailure,
          CommandesAchatApiActions.updateCommandeAchatFailure,
          CommandesAchatApiActions.deleteCommandeAchatFailure
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
