import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { DevisPageActions } from '../../../../store/devis/devis.actions';
import {
  selectDevisListViewModel,
  selectAllDevis
} from '../../../../store/devis/devis.selectors';
import { Devis } from '../../../../core/models/devis.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-devis-list',
  standalone: false,
  templateUrl: './devis-list.component.html',
  styleUrls: ['./devis-list.component.scss']
})
export class DevisListComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly destroy$ = new Subject<void>();

  // View model from store
  vm$ = this.store.select(selectDevisListViewModel);
  devisList$ = this.store.select(selectAllDevis);

  // Table configuration
  displayedColumns: string[] = ['numeroDevis', 'dateDevis', 'nomClient', 'montantTTC', 'statut', 'actions'];

  // Local state
  filteredDevis: Devis[] = [];
  allDevis: Devis[] = [];
  searchTerm: string = '';

  ngOnInit(): void {
    this.loadDevis();

    // Subscribe to devis for local filtering
    this.devisList$.pipe(takeUntil(this.destroy$)).subscribe(devis => {
      this.allDevis = devis;
      this.applyFilter();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDevis(): void {
    this.store.dispatch(DevisPageActions.loadDevisList({ params: { pageNumber: 1, pageSize: 100 } }));
  }

  onSearch(searchTerm: string): void {
    this.searchTerm = searchTerm.toLowerCase();
    this.applyFilter();
  }

  private applyFilter(): void {
    if (!this.searchTerm) {
      this.filteredDevis = [...this.allDevis];
    } else {
      this.filteredDevis = this.allDevis.filter(devis =>
        devis.numeroDevis.toLowerCase().includes(this.searchTerm) ||
        (devis.nomClient && devis.nomClient.toLowerCase().includes(this.searchTerm)) ||
        devis.codeClient.toLowerCase().includes(this.searchTerm) ||
        (devis.objet && devis.objet.toLowerCase().includes(this.searchTerm)) ||
        devis.statut.toLowerCase().includes(this.searchTerm)
      );
    }
  }

  onPageChange(event: { pageNumber: number; pageSize: number }): void {
    this.store.dispatch(DevisPageActions.setPage({
      pageNumber: event.pageNumber,
      pageSize: event.pageSize
    }));
    this.store.dispatch(DevisPageActions.loadDevisList({}));
  }

  navigateToNew(): void {
    this.router.navigate(['/devis/new']);
  }

  viewDevis(devis: Devis): void {
    this.router.navigate(['/devis', devis.numeroDevis]);
  }

  editDevis(devis: Devis): void {
    this.router.navigate(['/devis', devis.numeroDevis, 'edit']);
  }

  deleteDevis(devis: Devis): void {
    const dialogData: ConfirmDialogData = {
      title: 'Supprimer le devis',
      message: `Êtes-vous sûr de vouloir supprimer le devis "${devis.numeroDevis}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.store.dispatch(DevisPageActions.deleteDevis({ numeroDevis: devis.numeroDevis }));
      }
    });
  }

  getStatutClass(statut: string): string {
    switch (statut) {
      case 'Brouillon': return 'status-brouillon';
      case 'Envoyé': return 'status-envoye';
      case 'Accepté': return 'status-accepte';
      case 'Refusé': return 'status-refuse';
      case 'Expiré': return 'status-expire';
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
