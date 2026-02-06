import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, filter } from 'rxjs';

import { FournisseursPageActions, FournisseursApiActions } from '../../../../store/fournisseurs/fournisseurs.actions';
import { selectFournisseurByCode, selectFournisseursLoading, selectFournisseursError } from '../../../../store/fournisseurs/fournisseurs.selectors';
import { Fournisseur } from '../../../../core/models/fournisseur.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Actions, ofType } from '@ngrx/effects';

@Component({
  selector: 'app-fournisseur-detail',
  standalone: false,
  templateUrl: './fournisseur-detail.component.html',
  styleUrls: ['./fournisseur-detail.component.scss']
})
export class FournisseurDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // State
  fournisseur: Fournisseur | null = null;
  loading$ = this.store.select(selectFournisseursLoading);
  error$ = this.store.select(selectFournisseursError);
  fournisseurCode: string | null = null;

  ngOnInit(): void {
    this.fournisseurCode = this.route.snapshot.paramMap.get('code');
    
    if (this.fournisseurCode) {
      this.loadFournisseur(this.fournisseurCode);
      this.subscribeToFournisseur(this.fournisseurCode);
    }

    this.subscribeToDeleteSuccess();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadFournisseur(code: string): void {
    this.store.dispatch(FournisseursPageActions.loadFournisseur({ codeFournisseur: code }));
  }

  private subscribeToFournisseur(code: string): void {
    this.store.select(selectFournisseurByCode(code))
      .pipe(
        filter(fournisseur => fournisseur !== null),
        takeUntil(this.destroy$)
      )
      .subscribe(fournisseur => {
        this.fournisseur = fournisseur;
      });
  }

  private subscribeToDeleteSuccess(): void {
    this.actions$.pipe(
      ofType(FournisseursApiActions.deleteFournisseurSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.router.navigate(['/fournisseurs']);
    });
  }

  goBack(): void {
    this.router.navigate(['/fournisseurs']);
  }

  editFournisseur(): void {
    if (this.fournisseurCode) {
      this.router.navigate(['/fournisseurs', this.fournisseurCode, 'edit']);
    }
  }

  deleteFournisseur(): void {
    if (!this.fournisseur) return;

    const dialogData: ConfirmDialogData = {
      title: 'Supprimer le fournisseur',
      message: `Êtes-vous sûr de vouloir supprimer le fournisseur "${this.fournisseur.raisonSociale}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.fournisseurCode) {
        this.store.dispatch(FournisseursPageActions.deleteFournisseur({ codeFournisseur: this.fournisseurCode }));
      }
    });
  }

  retry(): void {
    if (this.fournisseurCode) {
      this.loadFournisseur(this.fournisseurCode);
    }
  }
}
