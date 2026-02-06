import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, filter } from 'rxjs';

import { ProduitsPageActions, ProduitsApiActions } from '../../../../store/produits/produits.actions';
import { selectProduitByCode, selectProduitsLoading, selectProduitsError } from '../../../../store/produits/produits.selectors';
import { Produit } from '../../../../core/models/produit.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Actions, ofType } from '@ngrx/effects';

@Component({
  selector: 'app-produit-detail',
  standalone: false,
  templateUrl: './produit-detail.component.html',
  styleUrls: ['./produit-detail.component.scss']
})
export class ProduitDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // State
  produit: Produit | null = null;
  loading$ = this.store.select(selectProduitsLoading);
  error$ = this.store.select(selectProduitsError);
  produitCode: string | null = null;

  ngOnInit(): void {
    this.produitCode = this.route.snapshot.paramMap.get('code');
    
    if (this.produitCode) {
      this.loadProduit(this.produitCode);
      this.subscribeToProduit(this.produitCode);
    }

    this.subscribeToDeleteSuccess();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadProduit(code: string): void {
    this.store.dispatch(ProduitsPageActions.loadProduit({ codeProduit: code }));
  }

  private subscribeToProduit(code: string): void {
    this.store.select(selectProduitByCode(code))
      .pipe(
        filter(produit => produit !== null),
        takeUntil(this.destroy$)
      )
      .subscribe(produit => {
        this.produit = produit;
      });
  }

  private subscribeToDeleteSuccess(): void {
    this.actions$.pipe(
      ofType(ProduitsApiActions.deleteProduitSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.router.navigate(['/produits']);
    });
  }

  get isLowStock(): boolean {
    return this.produit ? this.produit.stockActuel < this.produit.stockMinimum : false;
  }

  goBack(): void {
    this.router.navigate(['/produits']);
  }

  editProduit(): void {
    if (this.produitCode) {
      this.router.navigate(['/produits', this.produitCode, 'edit']);
    }
  }

  deleteProduit(): void {
    if (!this.produit) return;

    const dialogData: ConfirmDialogData = {
      title: 'Supprimer le produit',
      message: `Êtes-vous sûr de vouloir supprimer le produit "${this.produit.designation}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.produitCode) {
        this.store.dispatch(ProduitsPageActions.deleteProduit({ codeProduit: this.produitCode }));
      }
    });
  }

  retry(): void {
    if (this.produitCode) {
      this.loadProduit(this.produitCode);
    }
  }
}
