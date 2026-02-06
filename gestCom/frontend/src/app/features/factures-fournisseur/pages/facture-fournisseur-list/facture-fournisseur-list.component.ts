import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { PageEvent } from '@angular/material/paginator';
import { Subject, takeUntil } from 'rxjs';

import { FacturesFournisseurPageActions } from '../../../../store/factures-fournisseur/factures-fournisseur.actions';
import {
  selectFacturesFournisseurListViewModel,
  selectAllFacturesFournisseur
} from '../../../../store/factures-fournisseur/factures-fournisseur.selectors';
import { FactureFournisseur } from '../../../../core/models/facture-fournisseur.model';

@Component({
  selector: 'app-facture-fournisseur-list',
  standalone: false,
  templateUrl: './facture-fournisseur-list.component.html',
  styleUrls: ['./facture-fournisseur-list.component.scss']
})
export class FactureFournisseurListComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();

  // View model from store
  vm$ = this.store.select(selectFacturesFournisseurListViewModel);
  factures$ = this.store.select(selectAllFacturesFournisseur);

  // Table
  displayedColumns: string[] = ['codeFacture', 'dateFacture', 'raisonSocialeFournisseur', 'numeroFactureFournisseur', 'montantTTC', 'resteAPayer', 'statut', 'actions'];

  // Local state
  filteredFactures: FactureFournisseur[] = [];
  allFactures: FactureFournisseur[] = [];
  searchTerm: string = '';

  // Pagination
  pageSizeOptions = [10, 25, 50];

  ngOnInit(): void {
    this.loadFactures();

    // Subscribe to factures for local filtering
    this.factures$.pipe(takeUntil(this.destroy$)).subscribe(factures => {
      this.allFactures = factures;
      this.applyFilter();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFactures(): void {
    this.store.dispatch(FacturesFournisseurPageActions.loadFacturesFournisseur({ params: { pageNumber: 1, pageSize: 100 } }));
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
        (facture.raisonSocialeFournisseur && facture.raisonSocialeFournisseur.toLowerCase().includes(this.searchTerm)) ||
        (facture.numeroFactureFournisseur && facture.numeroFactureFournisseur.toLowerCase().includes(this.searchTerm)) ||
        (facture.codeFournisseur && facture.codeFournisseur.toLowerCase().includes(this.searchTerm)) ||
        (facture.statut && facture.statut.toLowerCase().includes(this.searchTerm))
      );
    }
  }

  onPageChange(event: PageEvent): void {
    this.store.dispatch(FacturesFournisseurPageActions.setPage({
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize
    }));
    this.store.dispatch(FacturesFournisseurPageActions.loadFacturesFournisseur({ params: { pageNumber: event.pageIndex + 1, pageSize: event.pageSize } }));
  }

  viewFacture(facture: FactureFournisseur): void {
    this.router.navigate(['/factures-fournisseur', facture.codeFacture]);
  }

  editFacture(facture: FactureFournisseur): void {
    this.router.navigate(['/factures-fournisseur', facture.codeFacture, 'edit']);
  }

  createFacture(): void {
    this.router.navigate(['/factures-fournisseur/new']);
  }

  getStatutClass(statut: string): string {
    switch (statut) {
      case 'Brouillon': return 'statut-brouillon';
      case 'Validée': return 'statut-validee';
      case 'Payée': return 'statut-payee';
      case 'Partiellement Payée': return 'statut-partielle';
      case 'Annulée': return 'statut-annulee';
      default: return 'statut-brouillon';
    }
  }

  retry(): void {
    this.loadFactures();
  }
}
