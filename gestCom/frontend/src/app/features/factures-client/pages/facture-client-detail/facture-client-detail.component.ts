import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, filter } from 'rxjs';

import { FacturesClientPageActions, FacturesClientApiActions } from '../../../../store/factures-client/factures-client.actions';
import { selectFactureClientByCode, selectFacturesClientLoading, selectFacturesClientError } from '../../../../store/factures-client/factures-client.selectors';
import { FactureClient } from '../../../../core/models/facture-client.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Actions, ofType } from '@ngrx/effects';

@Component({
  selector: 'app-facture-client-detail',
  standalone: false,
  templateUrl: './facture-client-detail.component.html',
  styleUrls: ['./facture-client-detail.component.scss']
})
export class FactureClientDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // State
  facture: FactureClient | null = null;
  loading$ = this.store.select(selectFacturesClientLoading);
  error$ = this.store.select(selectFacturesClientError);
  factureCode: string | null = null;

  // Lignes table columns
  lignesColumns: string[] = ['codeProduit', 'designation', 'quantite', 'prixUnitaireHT', 'tauxRemise', 'montantHT', 'montantTVA', 'montantTTC'];

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
    this.store.dispatch(FacturesClientPageActions.loadFactureClient({ codeFacture: code }));
  }

  private subscribeToFacture(code: string): void {
    this.store.select(selectFactureClientByCode(code))
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
      ofType(FacturesClientApiActions.deleteFactureClientSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.router.navigate(['/factures-client']);
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

  goBack(): void {
    this.router.navigate(['/factures-client']);
  }

  editFacture(): void {
    if (this.factureCode) {
      this.router.navigate(['/factures-client', this.factureCode, 'edit']);
    }
  }

  deleteFacture(): void {
    if (!this.facture) return;

    const dialogData: ConfirmDialogData = {
      title: 'Supprimer la facture',
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
        this.store.dispatch(FacturesClientPageActions.deleteFactureClient({ codeFacture: this.factureCode }));
      }
    });
  }

  retry(): void {
    if (this.factureCode) {
      this.loadFacture(this.factureCode);
    }
  }
}
