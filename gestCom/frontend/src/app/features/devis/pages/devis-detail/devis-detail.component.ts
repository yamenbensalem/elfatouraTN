import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, filter } from 'rxjs';

import { DevisPageActions, DevisApiActions } from '../../../../store/devis/devis.actions';
import { selectDevisByCode, selectDevisLoading, selectDevisError } from '../../../../store/devis/devis.selectors';
import { Devis } from '../../../../core/models/devis.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Actions, ofType } from '@ngrx/effects';

@Component({
  selector: 'app-devis-detail',
  standalone: false,
  templateUrl: './devis-detail.component.html',
  styleUrls: ['./devis-detail.component.scss']
})
export class DevisDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // State
  devis: Devis | null = null;
  loading$ = this.store.select(selectDevisLoading);
  error$ = this.store.select(selectDevisError);
  devisCode: string | null = null;

  // Lignes table columns
  lignesColumns: string[] = ['codeProduit', 'designation', 'quantite', 'prixUnitaireHT', 'tauxRemise', 'montantHT', 'montantTVA', 'montantTTC'];

  ngOnInit(): void {
    this.devisCode = this.route.snapshot.paramMap.get('code');

    if (this.devisCode) {
      this.loadDevis(this.devisCode);
      this.subscribeToDevis(this.devisCode);
    }

    this.subscribeToDeleteSuccess();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadDevis(code: string): void {
    this.store.dispatch(DevisPageActions.loadDevis({ codeDevis: code }));
  }

  private subscribeToDevis(code: string): void {
    this.store.select(selectDevisByCode(code))
      .pipe(
        filter(devis => devis !== null),
        takeUntil(this.destroy$)
      )
      .subscribe(devis => {
        this.devis = devis;
      });
  }

  private subscribeToDeleteSuccess(): void {
    this.actions$.pipe(
      ofType(DevisApiActions.deleteDevisSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.router.navigate(['/devis']);
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

  goBack(): void {
    this.router.navigate(['/devis']);
  }

  editDevis(): void {
    if (this.devisCode) {
      this.router.navigate(['/devis', this.devisCode, 'edit']);
    }
  }

  deleteDevis(): void {
    if (!this.devis) return;

    const dialogData: ConfirmDialogData = {
      title: 'Supprimer le devis',
      message: `Êtes-vous sûr de vouloir supprimer le devis "${this.devis.codeDevis}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.devisCode) {
        this.store.dispatch(DevisPageActions.deleteDevis({ codeDevis: this.devisCode }));
      }
    });
  }

  retry(): void {
    if (this.devisCode) {
      this.loadDevis(this.devisCode);
    }
  }
}
