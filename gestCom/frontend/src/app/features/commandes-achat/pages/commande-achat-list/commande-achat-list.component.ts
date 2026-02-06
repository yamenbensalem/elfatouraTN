import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { CommandesAchatPageActions } from '../../../../store/commandes-achat/commandes-achat.actions';
import {
  selectCommandesAchatListViewModel,
  selectAllCommandesAchat
} from '../../../../store/commandes-achat/commandes-achat.selectors';
import { CommandeAchat } from '../../../../core/models/commande-achat.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-commande-achat-list',
  standalone: false,
  templateUrl: './commande-achat-list.component.html',
  styleUrls: ['./commande-achat-list.component.scss']
})
export class CommandeAchatListComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly destroy$ = new Subject<void>();

  // View model from store
  vm$ = this.store.select(selectCommandesAchatListViewModel);
  commandes$ = this.store.select(selectAllCommandesAchat);

  // Table configuration
  displayedColumns: string[] = ['codeCommande', 'dateCommande', 'raisonSocialeFournisseur', 'montantTTC', 'statut', 'actions'];

  // Local state
  filteredCommandes: CommandeAchat[] = [];
  allCommandes: CommandeAchat[] = [];
  searchTerm: string = '';

  ngOnInit(): void {
    this.loadCommandes();

    this.commandes$.pipe(takeUntil(this.destroy$)).subscribe(commandes => {
      this.allCommandes = commandes;
      this.applyFilter();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCommandes(): void {
    this.store.dispatch(CommandesAchatPageActions.loadCommandesAchat({ params: { pageNumber: 1, pageSize: 100 } }));
  }

  onSearch(searchTerm: string): void {
    this.searchTerm = searchTerm.toLowerCase();
    this.applyFilter();
  }

  private applyFilter(): void {
    if (!this.searchTerm) {
      this.filteredCommandes = [...this.allCommandes];
    } else {
      this.filteredCommandes = this.allCommandes.filter(commande =>
        commande.codeCommande.toLowerCase().includes(this.searchTerm) ||
        (commande.raisonSocialeFournisseur && commande.raisonSocialeFournisseur.toLowerCase().includes(this.searchTerm)) ||
        commande.codeFournisseur.toLowerCase().includes(this.searchTerm) ||
        commande.statut.toLowerCase().includes(this.searchTerm)
      );
    }
  }

  onPageChange(event: { pageNumber: number; pageSize: number }): void {
    this.store.dispatch(CommandesAchatPageActions.setPage({
      pageNumber: event.pageNumber,
      pageSize: event.pageSize
    }));
    this.store.dispatch(CommandesAchatPageActions.loadCommandesAchat({}));
  }

  navigateToNew(): void {
    this.router.navigate(['/commandes-achat/new']);
  }

  viewCommande(commande: CommandeAchat): void {
    this.router.navigate(['/commandes-achat', commande.codeCommande]);
  }

  editCommande(commande: CommandeAchat): void {
    this.router.navigate(['/commandes-achat', commande.codeCommande, 'edit']);
  }

  deleteCommande(commande: CommandeAchat): void {
    const dialogData: ConfirmDialogData = {
      title: "Supprimer la commande d'achat",
      message: `Êtes-vous sûr de vouloir supprimer la commande "${commande.codeCommande}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.store.dispatch(CommandesAchatPageActions.deleteCommandeAchat({ codeCommande: commande.codeCommande }));
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
}
