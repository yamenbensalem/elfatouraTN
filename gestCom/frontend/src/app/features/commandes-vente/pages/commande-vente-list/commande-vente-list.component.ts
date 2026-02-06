import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { CommandesVentePageActions } from '../../../../store/commandes-vente/commandes-vente.actions';
import {
  selectCommandesVenteListViewModel,
  selectAllCommandesVente
} from '../../../../store/commandes-vente/commandes-vente.selectors';
import { CommandeVente } from '../../../../core/models/commande-vente.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-commande-vente-list',
  standalone: false,
  templateUrl: './commande-vente-list.component.html',
  styleUrls: ['./commande-vente-list.component.scss']
})
export class CommandeVenteListComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly destroy$ = new Subject<void>();

  // View model from store
  vm$ = this.store.select(selectCommandesVenteListViewModel);
  commandes$ = this.store.select(selectAllCommandesVente);

  // Table configuration
  displayedColumns: string[] = ['codeCommande', 'dateCommande', 'raisonSocialeClient', 'montantTTC', 'statut', 'actions'];

  // Local state
  filteredCommandes: CommandeVente[] = [];
  allCommandes: CommandeVente[] = [];
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
    this.store.dispatch(CommandesVentePageActions.loadCommandesVente({ params: { pageNumber: 1, pageSize: 100 } }));
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
        (commande.raisonSocialeClient && commande.raisonSocialeClient.toLowerCase().includes(this.searchTerm)) ||
        commande.codeClient.toLowerCase().includes(this.searchTerm) ||
        commande.statut.toLowerCase().includes(this.searchTerm)
      );
    }
  }

  onPageChange(event: { pageNumber: number; pageSize: number }): void {
    this.store.dispatch(CommandesVentePageActions.setPage({
      pageNumber: event.pageNumber,
      pageSize: event.pageSize
    }));
    this.store.dispatch(CommandesVentePageActions.loadCommandesVente({}));
  }

  navigateToNew(): void {
    this.router.navigate(['/commandes-vente/new']);
  }

  viewCommande(commande: CommandeVente): void {
    this.router.navigate(['/commandes-vente', commande.codeCommande]);
  }

  editCommande(commande: CommandeVente): void {
    this.router.navigate(['/commandes-vente', commande.codeCommande, 'edit']);
  }

  deleteCommande(commande: CommandeVente): void {
    const dialogData: ConfirmDialogData = {
      title: 'Supprimer la commande de vente',
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
        this.store.dispatch(CommandesVentePageActions.deleteCommandeVente({ codeCommande: commande.codeCommande }));
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
}
