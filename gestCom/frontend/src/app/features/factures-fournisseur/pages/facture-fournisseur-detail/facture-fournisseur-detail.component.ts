import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, filter } from 'rxjs';

import { FacturesFournisseurPageActions, FacturesFournisseurApiActions } from '../../../../store/factures-fournisseur/factures-fournisseur.actions';
import { selectFactureFournisseurByCode, selectFacturesFournisseurLoading, selectFacturesFournisseurError } from '../../../../store/factures-fournisseur/factures-fournisseur.selectors';
import { FactureFournisseur } from '../../../../core/models/facture-fournisseur.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Actions, ofType } from '@ngrx/effects';

@Component({
  selector: 'app-facture-fournisseur-detail',
  standalone: false,
  templateUrl: './facture-fournisseur-detail.component.html',
  styleUrls: ['./facture-fournisseur-detail.component.scss']
})
export class FactureFournisseurDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // State
  facture: FactureFournisseur | null = null;
  loading$ = this.store.select(selectFacturesFournisseurLoading);
  error$ = this.store.select(selectFacturesFournisseurError);
  factureCode: string | null = null;

  // Lignes table columns
  lignesColumns: string[] = ['codeProduit', 'designation', 'quantite', 'prixUnitaireHT', 'tauxRemise', 'tauxTVA', 'montantHT', 'montantTTC'];

  ngOnInit(): void {
    this.factureCode = this.route.snapshot.paramMap.get('code');

    if (this.factureCode) {
      this.loadFacture(this.factureCode);
      this.subscribeToFacture(this.factureCode);
    }

    this.subscribeToDeleteSuccess();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadFacture(code: string): void {
    this.store.dispatch(FacturesFournisseurPageActions.loadFactureFournisseur({ codeFacture: code }));
  }

  private subscribeToFacture(code: string): void {
    this.store.select(selectFactureFournisseurByCode(code))
      .pipe(
        filter(facture => facture !== null),
        takeUntil(this.destroy$)
      )
      .subscribe(facture => {
        this.facture = facture;
      });
  }

  private subscribeToDeleteSuccess(): void {
    this.actions$.pipe(
      ofType(FacturesFournisseurApiActions.deleteFactureFournisseurSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.router.navigate(['/factures-fournisseur']);
    });
  }

  goBack(): void {
    this.router.navigate(['/factures-fournisseur']);
  }

  editFacture(): void {
    if (this.factureCode) {
      this.router.navigate(['/factures-fournisseur', this.factureCode, 'edit']);
    }
  }

  deleteFacture(): void {
    if (!this.facture) return;

    const dialogData: ConfirmDialogData = {
      title: 'Supprimer la facture fournisseur',
      message: `Êtes-vous sûr de vouloir supprimer la facture "${this.facture.codeFacture}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.factureCode) {
        this.store.dispatch(FacturesFournisseurPageActions.deleteFactureFournisseur({ codeFacture: this.factureCode }));
      }
    });
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
    if (this.factureCode) {
      this.loadFacture(this.factureCode);
    }
  }
}
