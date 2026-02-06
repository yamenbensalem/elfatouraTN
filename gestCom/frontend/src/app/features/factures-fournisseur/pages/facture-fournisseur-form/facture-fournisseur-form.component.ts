import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { Subject, takeUntil, filter, take } from 'rxjs';

import { FacturesFournisseurPageActions, FacturesFournisseurApiActions } from '../../../../store/factures-fournisseur/factures-fournisseur.actions';
import { selectFactureFournisseurByCode, selectFacturesFournisseurLoading } from '../../../../store/factures-fournisseur/factures-fournisseur.selectors';
import { CreateFactureFournisseurRequest, UpdateFactureFournisseurRequest } from '../../../../core/models/facture-fournisseur.model';

@Component({
  selector: 'app-facture-fournisseur-form',
  standalone: false,
  templateUrl: './facture-fournisseur-form.component.html',
  styleUrls: ['./facture-fournisseur-form.component.scss']
})
export class FactureFournisseurFormComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // Form
  factureForm!: FormGroup;

  // State
  isEditMode = false;
  factureCode: string | null = null;
  loading$ = this.store.select(selectFacturesFournisseurLoading);
  submitting = false;

  // Page title
  get pageTitle(): string {
    return this.isEditMode ? 'Modifier Facture Fournisseur' : 'Nouvelle Facture Fournisseur';
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
    this.factureForm = this.fb.group({
      codeFacture: ['', [Validators.required, Validators.maxLength(50)]],
      dateFacture: [new Date(), [Validators.required]],
      codeFournisseur: ['', [Validators.required, Validators.maxLength(50)]],
      numeroFactureFournisseur: ['', [Validators.maxLength(100)]],
      tauxRemiseGlobale: [0, [Validators.min(0), Validators.max(100)]],
      montantTimbre: [0, [Validators.min(0)]],
      notes: ['', [Validators.maxLength(1000)]]
    });
  }

  private checkEditMode(): void {
    const code = this.route.snapshot.paramMap.get('code');
    const mode = this.route.snapshot.data['mode'];

    if (code && mode === 'edit') {
      this.isEditMode = true;
      this.factureCode = code;
      this.loadFacture(code);

      // Disable code field in edit mode
      this.factureForm.get('codeFacture')?.disable();
    }
  }

  private loadFacture(code: string): void {
    this.store.dispatch(FacturesFournisseurPageActions.loadFactureFournisseur({ codeFacture: code }));

    this.store.select(selectFactureFournisseurByCode(code))
      .pipe(
        filter(facture => facture !== null),
        take(1),
        takeUntil(this.destroy$)
      )
      .subscribe(facture => {
        if (facture) {
          this.factureForm.patchValue({
            codeFacture: facture.codeFacture,
            dateFacture: facture.dateFacture ? new Date(facture.dateFacture) : null,
            codeFournisseur: facture.codeFournisseur,
            numeroFactureFournisseur: facture.numeroFactureFournisseur || '',
            tauxRemiseGlobale: facture.tauxRemiseGlobale || 0,
            montantTimbre: facture.montantTimbre || 0,
            notes: facture.notes || ''
          });
        }
      });
  }

  private subscribeToActions(): void {
    this.actions$.pipe(
      ofType(FacturesFournisseurApiActions.createFactureFournisseurSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/factures-fournisseur']);
    });

    this.actions$.pipe(
      ofType(FacturesFournisseurApiActions.updateFactureFournisseurSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/factures-fournisseur']);
    });

    this.actions$.pipe(
      ofType(
        FacturesFournisseurApiActions.createFactureFournisseurFailure,
        FacturesFournisseurApiActions.updateFactureFournisseurFailure
      ),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
    });
  }

  onSubmit(): void {
    if (this.factureForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.submitting = true;
    const formValue = this.factureForm.getRawValue();

    if (this.isEditMode && this.factureCode) {
      const updateRequest: UpdateFactureFournisseurRequest = {
        dateFacture: formValue.dateFacture || undefined,
        codeFournisseur: formValue.codeFournisseur || undefined,
        numeroFactureFournisseur: formValue.numeroFactureFournisseur || undefined,
        tauxRemiseGlobale: formValue.tauxRemiseGlobale,
        notes: formValue.notes || undefined
      };

      this.store.dispatch(FacturesFournisseurPageActions.updateFactureFournisseur({
        codeFacture: this.factureCode,
        facture: updateRequest
      }));
    } else {
      const createRequest: CreateFactureFournisseurRequest = {
        codeFacture: formValue.codeFacture,
        dateFacture: formValue.dateFacture,
        codeFournisseur: formValue.codeFournisseur,
        numeroFactureFournisseur: formValue.numeroFactureFournisseur || undefined,
        tauxRemiseGlobale: formValue.tauxRemiseGlobale || 0,
        notes: formValue.notes || undefined,
        lignes: []
      };

      this.store.dispatch(FacturesFournisseurPageActions.createFactureFournisseur({ facture: createRequest }));
    }
  }

  onCancel(): void {
    this.router.navigate(['/factures-fournisseur']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.factureForm.controls).forEach(key => {
      const control = this.factureForm.get(key);
      control?.markAsTouched();
      control?.markAsDirty();
    });
  }

  hasError(fieldName: string, errorType: string): boolean {
    const control = this.factureForm.get(fieldName);
    return control ? control.hasError(errorType) && control.touched : false;
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.factureForm.get(fieldName);
    return control ? control.invalid && control.touched : false;
  }
}
