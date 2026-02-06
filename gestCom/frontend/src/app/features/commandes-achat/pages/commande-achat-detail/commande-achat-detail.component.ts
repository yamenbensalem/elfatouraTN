import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, filter } from 'rxjs';

import { CommandesAchatPageActions, CommandesAchatApiActions } from '../../../../store/commandes-achat/commandes-achat.actions';
import { selectCommandeAchatByCode, selectCommandesAchatLoading, selectCommandesAchatError } from '../../../../store/commandes-achat/commandes-achat.selectors';
import { CommandeAchat } from '../../../../core/models/commande-achat.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Actions, ofType } from '@ngrx/effects';

@Component({
  selector: 'app-commande-achat-detail',
  standalone: false,
  templateUrl: './commande-achat-detail.component.html',
  styleUrls: ['./commande-achat-detail.component.scss']
})
export class CommandeAchatDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // State
  commande: CommandeAchat | null = null;
  loading$ = this.store.select(selectCommandesAchatLoading);
  error$ = this.store.select(selectCommandesAchatError);
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
    this.store.dispatch(CommandesAchatPageActions.loadCommandeAchat({ codeCommande: code }));
  }

  private subscribeToCommande(code: string): void {
    this.store.select(selectCommandeAchatByCode(code))
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
      ofType(CommandesAchatApiActions.deleteCommandeAchatSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.router.navigate(['/commandes-achat']);
    });
  }

  goBack(): void {
    this.router.navigate(['/commandes-achat']);
  }

  editCommande(): void {
    if (this.commandeCode) {
      this.router.navigate(['/commandes-achat', this.commandeCode, 'edit']);
    }
  }

  deleteCommande(): void {
    if (!this.commande) return;

    const dialogData: ConfirmDialogData = {
      title: "Supprimer la commande d'achat",
      message: `Êtes-vous sûr de vouloir supprimer la commande "${this.commande.codeCommande}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.commandeCode) {
        this.store.dispatch(CommandesAchatPageActions.deleteCommandeAchat({ codeCommande: this.commandeCode }));
      }
    });
  }

  getStatutClass(statut: string): string {
    switch (statut) {
      case 'Brouillon': return 'statut-brouillon';
      case 'Confirmée': return 'statut-confirmee';
      case 'Reçue': return 'statut-recue';
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
