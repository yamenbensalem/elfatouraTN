import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, filter } from 'rxjs';

import { CommandesVentePageActions, CommandesVenteApiActions } from '../../../../store/commandes-vente/commandes-vente.actions';
import { selectCommandeVenteByCode, selectCommandesVenteLoading, selectCommandesVenteError } from '../../../../store/commandes-vente/commandes-vente.selectors';
import { CommandeVente } from '../../../../core/models/commande-vente.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Actions, ofType } from '@ngrx/effects';

@Component({
  selector: 'app-commande-vente-detail',
  standalone: false,
  templateUrl: './commande-vente-detail.component.html',
  styleUrls: ['./commande-vente-detail.component.scss']
})
export class CommandeVenteDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // State
  commande: CommandeVente | null = null;
  loading$ = this.store.select(selectCommandesVenteLoading);
  error$ = this.store.select(selectCommandesVenteError);
  commandeCode: string | null = null;

  // Lignes table columns
  lignesColumns: string[] = ['codeProduit', 'designation', 'quantite', 'prixUnitaireHT', 'tauxRemise', 'montantHT'];

  ngOnInit(): void {
    this.commandeCode = this.route.snapshot.paramMap.get('code');

    if (this.commandeCode) {
      this.loadCommande(this.commandeCode);
      this.subscribeToCommande(this.commandeCode);
    }

    this.subscribeToDeleteSuccess();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadCommande(code: string): void {
    this.store.dispatch(CommandesVentePageActions.loadCommandeVente({ numeroCommande: code }));
  }

  private subscribeToCommande(code: string): void {
    this.store.select(selectCommandeVenteByCode(code))
      .pipe(
        filter(commande => commande !== null),
        takeUntil(this.destroy$)
      )
      .subscribe(commande => {
        this.commande = commande;
      });
  }

  private subscribeToDeleteSuccess(): void {
    this.actions$.pipe(
      ofType(CommandesVenteApiActions.deleteCommandeVenteSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.router.navigate(['/commandes-vente']);
    });
  }

  goBack(): void {
    this.router.navigate(['/commandes-vente']);
  }

  editCommande(): void {
    if (this.commandeCode) {
      this.router.navigate(['/commandes-vente', this.commandeCode, 'edit']);
    }
  }

  deleteCommande(): void {
    if (!this.commande) return;

    const dialogData: ConfirmDialogData = {
      title: 'Supprimer la commande de vente',
      message: `Êtes-vous sûr de vouloir supprimer la commande "${this.commande.numeroCommande}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.commandeCode) {
        this.store.dispatch(CommandesVentePageActions.deleteCommandeVente({ numeroCommande: this.commandeCode }));
      }
    });
  }

  getStatutClass(statut: string): string {
    switch (statut) {
      case 'Brouillon': return 'statut-brouillon';
      case 'Confirmée': return 'statut-confirmee';
      case 'Livrée': return 'statut-livree';
      case 'Annulée': return 'statut-annulee';
      default: return 'statut-brouillon';
    }
  }

  retry(): void {
    if (this.commandeCode) {
      this.loadCommande(this.commandeCode);
    }
  }
}
