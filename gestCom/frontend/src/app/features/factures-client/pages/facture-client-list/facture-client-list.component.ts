import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { FacturesClientPageActions } from '../../../../store/factures-client/factures-client.actions';
import {
  selectFacturesClientListViewModel,
  selectAllFacturesClient
} from '../../../../store/factures-client/factures-client.selectors';
import { FactureClient } from '../../../../core/models/facture-client.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-facture-client-list',
  standalone: false,
  templateUrl: './facture-client-list.component.html',
  styleUrls: ['./facture-client-list.component.scss']
})
export class FactureClientListComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly destroy$ = new Subject<void>();

  // View model from store
  vm$ = this.store.select(selectFacturesClientListViewModel);
  facturesList$ = this.store.select(selectAllFacturesClient);

  // Table configuration
  displayedColumns: string[] = ['codeFacture', 'dateFacture', 'raisonSocialeClient', 'montantTTC', 'resteAPayer', 'statut', 'actions'];

  // Local state
  filteredFactures: FactureClient[] = [];
  allFactures: FactureClient[] = [];
  searchTerm: string = '';

  ngOnInit(): void {
    this.loadFactures();

    // Subscribe to factures for local filtering
    this.facturesList$.pipe(takeUntil(this.destroy$)).subscribe(factures => {
      this.allFactures = factures;
      this.applyFilter();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFactures(): void {
    this.store.dispatch(FacturesClientPageActions.loadFacturesClient({ params: { pageNumber: 1, pageSize: 100 } }));
  }

  onSearch(searchTerm: string): void {
    this.searchTerm = searchTerm.toLowerCase();
    this.applyFilter();
  }

  private applyFilter(): void {
    if (!this.searchTerm) {
      this.filteredFactures = [...this.allFactures];
    } else {
      this.filteredFactures = this.allFactures.filter(facture =>
        facture.codeFacture.toLowerCase().includes(this.searchTerm) ||
        (facture.raisonSocialeClient && facture.raisonSocialeClient.toLowerCase().includes(this.searchTerm)) ||
        facture.codeClient.toLowerCase().includes(this.searchTerm) ||
        facture.statut.toLowerCase().includes(this.searchTerm)
      );
    }
  }

  onPageChange(event: { pageNumber: number; pageSize: number }): void {
    this.store.dispatch(FacturesClientPageActions.setPage({
      pageNumber: event.pageNumber,
      pageSize: event.pageSize
    }));
    this.store.dispatch(FacturesClientPageActions.loadFacturesClient({}));
  }

  navigateToNew(): void {
    this.router.navigate(['/factures-client/new']);
  }

  viewFacture(facture: FactureClient): void {
    this.router.navigate(['/factures-client', facture.codeFacture]);
  }

  editFacture(facture: FactureClient): void {
    this.router.navigate(['/factures-client', facture.codeFacture, 'edit']);
  }

  deleteFacture(facture: FactureClient): void {
    const dialogData: ConfirmDialogData = {
      title: 'Supprimer la facture',
      message: `Êtes-vous sûr de vouloir supprimer la facture "${facture.codeFacture}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.store.dispatch(FacturesClientPageActions.deleteFactureClient({ codeFacture: facture.codeFacture }));
      }
    });
  }

  getStatutClass(statut: string): string {
    switch (statut) {
      case 'Brouillon': return 'status-brouillon';
      case 'Validée': return 'status-validee';
      case 'Payée': return 'status-payee';
      case 'Partiellement Payée': return 'status-partiellement-payee';
      case 'Annulée': return 'status-annulee';
      default: return 'status-brouillon';
    }
  }

  formatDate(date: Date): string {
    if (!date) return '-';
    const d = new Date(date);
    const day = d.getDate().toString().padStart(2, '0');
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
  }
}
