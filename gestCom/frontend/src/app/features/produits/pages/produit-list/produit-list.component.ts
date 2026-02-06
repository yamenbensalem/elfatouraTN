import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { ProduitsPageActions } from '../../../../store/produits/produits.actions';
import { 
  selectProduitsListViewModel,
  selectAllProduits 
} from '../../../../store/produits/produits.selectors';
import { Produit } from '../../../../core/models/produit.model';
import { TableColumn } from '../../../../shared/components/data-table/data-table.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-produit-list',
  standalone: false,
  templateUrl: './produit-list.component.html',
  styleUrls: ['./produit-list.component.scss']
})
export class ProduitListComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly destroy$ = new Subject<void>();

  // View model from store
  vm$ = this.store.select(selectProduitsListViewModel);
  produits$ = this.store.select(selectAllProduits);

  // Table configuration
  columns: TableColumn[] = [
    { key: 'codeProduit', header: 'Code Produit', sortable: true },
    { key: 'designation', header: 'Désignation', sortable: true },
    { key: 'nomCategorie', header: 'Catégorie', sortable: true },
    { key: 'prixVenteHT', header: 'Prix Vente HT', sortable: true },
    { key: 'stockActuel', header: 'Stock', sortable: true }
  ];

  displayedColumns: string[] = ['codeProduit', 'designation', 'nomCategorie', 'prixVenteHT', 'stockActuel', 'actions'];

  // Local state
  filteredProduits: Produit[] = [];
  allProduits: Produit[] = [];
  searchTerm: string = '';

  ngOnInit(): void {
    this.loadProduits();
    
    // Subscribe to produits for local filtering
    this.produits$.pipe(takeUntil(this.destroy$)).subscribe(produits => {
      this.allProduits = produits;
      this.applyFilter();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProduits(): void {
    this.store.dispatch(ProduitsPageActions.loadProduits({ params: { pageNumber: 1, pageSize: 100 } }));
  }

  onSearch(searchTerm: string): void {
    this.searchTerm = searchTerm.toLowerCase();
    this.applyFilter();
  }

  private applyFilter(): void {
    if (!this.searchTerm) {
      this.filteredProduits = [...this.allProduits];
    } else {
      this.filteredProduits = this.allProduits.filter(produit =>
        produit.codeProduit.toLowerCase().includes(this.searchTerm) ||
        produit.designation.toLowerCase().includes(this.searchTerm) ||
        (produit.nomCategorie && produit.nomCategorie.toLowerCase().includes(this.searchTerm)) ||
        (produit.codeBarres && produit.codeBarres.toLowerCase().includes(this.searchTerm))
      );
    }
  }

  onPageChange(event: { pageNumber: number; pageSize: number }): void {
    this.store.dispatch(ProduitsPageActions.setPage({
      pageNumber: event.pageNumber,
      pageSize: event.pageSize
    }));
    this.store.dispatch(ProduitsPageActions.loadProduits({}));
  }

  onSortChange(event: { sortBy: string; sortDescending: boolean }): void {
    this.store.dispatch(ProduitsPageActions.loadProduits({
      params: {
        pageNumber: 1,
        pageSize: 10,
        sortBy: event.sortBy,
        sortDescending: event.sortDescending
      }
    }));
  }

  navigateToNew(): void {
    this.router.navigate(['/produits/new']);
  }

  viewProduit(produit: Produit): void {
    this.router.navigate(['/produits', produit.codeProduit]);
  }

  editProduit(produit: Produit): void {
    this.router.navigate(['/produits', produit.codeProduit, 'edit']);
  }

  deleteProduit(produit: Produit): void {
    const dialogData: ConfirmDialogData = {
      title: 'Supprimer le produit',
      message: `Êtes-vous sûr de vouloir supprimer le produit "${produit.designation}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.store.dispatch(ProduitsPageActions.deleteProduit({ codeProduit: produit.codeProduit }));
      }
    });
  }

  isLowStock(produit: Produit): boolean {
    return produit.stockActuel < produit.stockMinimum;
  }

  exportToExcel(): void {
    // TODO: Implement Excel export functionality
    console.log('Export to Excel - Coming soon');
  }
}
