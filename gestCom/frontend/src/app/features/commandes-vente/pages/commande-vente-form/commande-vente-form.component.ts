import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { Subject, takeUntil, filter, take } from 'rxjs';

import { CommandesVentePageActions, CommandesVenteApiActions } from '../../../../store/commandes-vente/commandes-vente.actions';
import { selectCommandeVenteByCode, selectCommandesVenteLoading } from '../../../../store/commandes-vente/commandes-vente.selectors';
import { CreateCommandeVenteRequest, UpdateCommandeVenteRequest } from '../../../../core/models/commande-vente.model';

@Component({
  selector: 'app-commande-vente-form',
  standalone: false,
  templateUrl: './commande-vente-form.component.html',
  styleUrls: ['./commande-vente-form.component.scss']
})
export class CommandeVenteFormComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // Form
  commandeForm!: FormGroup;

  // State
  isEditMode = false;
  commandeCode: string | null = null;
  loading$ = this.store.select(selectCommandesVenteLoading);
  submitting = false;

  // Page title
  get pageTitle(): string {
    return this.isEditMode ? 'Modifier Commande de Vente' : 'Nouvelle Commande de Vente';
  }

  ngOnInit(): void {
    this.initForm();
    this.checkEditMode();
    this.subscribeToActions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.commandeForm = this.fb.group({
      codeCommande: ['', [Validators.required, Validators.maxLength(50)]],
      dateCommande: [new Date(), [Validators.required]],
      codeClient: ['', [Validators.required, Validators.maxLength(50)]],
      dateLivraisonPrevue: [null],
      notes: ['', [Validators.maxLength(1000)]]
    });
  }

  private checkEditMode(): void {
    const code = this.route.snapshot.paramMap.get('code');
    const mode = this.route.snapshot.data['mode'];

    if (code && mode === 'edit') {
      this.isEditMode = true;
      this.commandeCode = code;
      this.loadCommande(code);

      // Disable code field in edit mode
      this.commandeForm.get('codeCommande')?.disable();
    }
  }

  private loadCommande(code: string): void {
    this.store.dispatch(CommandesVentePageActions.loadCommandeVente({ codeCommande: code }));

    this.store.select(selectCommandeVenteByCode(code))
      .pipe(
        filter(commande => commande !== null),
        take(1),
        takeUntil(this.destroy$)
      )
      .subscribe(commande => {
        if (commande) {
          this.commandeForm.patchValue({
            codeCommande: commande.codeCommande,
            dateCommande: commande.dateCommande ? new Date(commande.dateCommande) : null,
            codeClient: commande.codeClient,
            dateLivraisonPrevue: commande.dateLivraisonPrevue ? new Date(commande.dateLivraisonPrevue) : null,
            notes: commande.notes || ''
          });
        }
      });
  }

  private subscribeToActions(): void {
    this.actions$.pipe(
      ofType(CommandesVenteApiActions.createCommandeVenteSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/commandes-vente']);
    });

    this.actions$.pipe(
      ofType(CommandesVenteApiActions.updateCommandeVenteSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/commandes-vente']);
    });

    this.actions$.pipe(
      ofType(
        CommandesVenteApiActions.createCommandeVenteFailure,
        CommandesVenteApiActions.updateCommandeVenteFailure
      ),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
    });
  }

  onSubmit(): void {
    if (this.commandeForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.submitting = true;
    const formValue = this.commandeForm.getRawValue();

    if (this.isEditMode && this.commandeCode) {
      const updateRequest: UpdateCommandeVenteRequest = {
        dateCommande: formValue.dateCommande || undefined,
        codeClient: formValue.codeClient || undefined,
        dateLivraisonPrevue: formValue.dateLivraisonPrevue || undefined,
        notes: formValue.notes || undefined
      };

      this.store.dispatch(CommandesVentePageActions.updateCommandeVente({
        codeCommande: this.commandeCode,
        commande: updateRequest
      }));
    } else {
      const createRequest: CreateCommandeVenteRequest = {
        codeCommande: formValue.codeCommande,
        dateCommande: formValue.dateCommande,
        codeClient: formValue.codeClient,
        dateLivraisonPrevue: formValue.dateLivraisonPrevue || undefined,
        notes: formValue.notes || undefined,
        lignes: []
      };

      this.store.dispatch(CommandesVentePageActions.createCommandeVente({ commande: createRequest }));
    }
  }

  onCancel(): void {
    this.router.navigate(['/commandes-vente']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.commandeForm.controls).forEach(key => {
      const control = this.commandeForm.get(key);
      control?.markAsTouched();
      control?.markAsDirty();
    });
  }

  hasError(fieldName: string, errorType: string): boolean {
    const control = this.commandeForm.get(fieldName);
    return control ? control.hasError(errorType) && control.touched : false;
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.commandeForm.get(fieldName);
    return control ? control.invalid && control.touched : false;
  }
}
