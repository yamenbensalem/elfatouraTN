import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { FournisseursPageActions } from '../../../../store/fournisseurs/fournisseurs.actions';
import { 
  selectFournisseursListViewModel,
  selectAllFournisseurs 
} from '../../../../store/fournisseurs/fournisseurs.selectors';
import { Fournisseur } from '../../../../core/models/fournisseur.model';
import { TableColumn } from '../../../../shared/components/data-table/data-table.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-fournisseur-list',
  standalone: false,
  templateUrl: './fournisseur-list.component.html',
  styleUrls: ['./fournisseur-list.component.scss']
})
export class FournisseurListComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly destroy$ = new Subject<void>();

  // View model from store
  vm$ = this.store.select(selectFournisseursListViewModel);
  fournisseurs$ = this.store.select(selectAllFournisseurs);

  // Table configuration
  columns: TableColumn[] = [
    { key: 'codeFournisseur', header: 'Code Fournisseur', sortable: true },
    { key: 'raisonSociale', header: 'Raison Sociale', sortable: true },
    { key: 'ville', header: 'Ville', sortable: true },
    { key: 'telephone', header: 'Téléphone', sortable: false },
    { key: 'email', header: 'Email', sortable: false }
  ];

  displayedColumns: string[] = ['codeFournisseur', 'raisonSociale', 'ville', 'telephone', 'email', 'actions'];

  // Local state
  filteredFournisseurs: Fournisseur[] = [];
  allFournisseurs: Fournisseur[] = [];
  searchTerm: string = '';

  ngOnInit(): void {
    this.loadFournisseurs();
    
    // Subscribe to fournisseurs for local filtering
    this.fournisseurs$.pipe(takeUntil(this.destroy$)).subscribe(fournisseurs => {
      this.allFournisseurs = fournisseurs;
      this.applyFilter();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFournisseurs(): void {
    this.store.dispatch(FournisseursPageActions.loadFournisseurs({ params: { pageNumber: 1, pageSize: 100 } }));
  }

  onSearch(searchTerm: string): void {
    this.searchTerm = searchTerm.toLowerCase();
    this.applyFilter();
  }

  private applyFilter(): void {
    if (!this.searchTerm) {
      this.filteredFournisseurs = [...this.allFournisseurs];
    } else {
      this.filteredFournisseurs = this.allFournisseurs.filter(fournisseur =>
        fournisseur.codeFournisseur.toLowerCase().includes(this.searchTerm) ||
        fournisseur.raisonSociale.toLowerCase().includes(this.searchTerm) ||
        (fournisseur.ville && fournisseur.ville.toLowerCase().includes(this.searchTerm)) ||
        (fournisseur.email && fournisseur.email.toLowerCase().includes(this.searchTerm)) ||
        (fournisseur.telephone && fournisseur.telephone.includes(this.searchTerm))
      );
    }
  }

  onPageChange(event: { pageNumber: number; pageSize: number }): void {
    this.store.dispatch(FournisseursPageActions.setPage({
      pageNumber: event.pageNumber,
      pageSize: event.pageSize
    }));
    this.store.dispatch(FournisseursPageActions.loadFournisseurs({}));
  }

  onSortChange(event: { sortBy: string; sortDescending: boolean }): void {
    this.store.dispatch(FournisseursPageActions.loadFournisseurs({
      params: {
        pageNumber: 1,
        pageSize: 10,
        sortBy: event.sortBy,
        sortDescending: event.sortDescending
      }
    }));
  }

  navigateToNew(): void {
    this.router.navigate(['/fournisseurs/new']);
  }

  viewFournisseur(fournisseur: Fournisseur): void {
    this.router.navigate(['/fournisseurs', fournisseur.codeFournisseur]);
  }

  editFournisseur(fournisseur: Fournisseur): void {
    this.router.navigate(['/fournisseurs', fournisseur.codeFournisseur, 'edit']);
  }

  deleteFournisseur(fournisseur: Fournisseur): void {
    const dialogData: ConfirmDialogData = {
      title: 'Supprimer le fournisseur',
      message: `Êtes-vous sûr de vouloir supprimer le fournisseur "${fournisseur.raisonSociale}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.store.dispatch(FournisseursPageActions.deleteFournisseur({ codeFournisseur: fournisseur.codeFournisseur }));
      }
    });
  }

  exportToExcel(): void {
    // TODO: Implement Excel export functionality
    console.log('Export to Excel - Coming soon');
  }
}
