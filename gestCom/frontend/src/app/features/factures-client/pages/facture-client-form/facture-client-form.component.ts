import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { Subject, takeUntil, filter, take } from 'rxjs';

import { FacturesClientPageActions, FacturesClientApiActions } from '../../../../store/factures-client/factures-client.actions';
import { selectFactureClientByCode, selectFacturesClientLoading } from '../../../../store/factures-client/factures-client.selectors';
import { CreateFactureClientRequest, UpdateFactureClientRequest } from '../../../../core/models/facture-client.model';

@Component({
  selector: 'app-facture-client-form',
  standalone: false,
  templateUrl: './facture-client-form.component.html',
  styleUrls: ['./facture-client-form.component.scss']
})
export class FactureClientFormComponent implements OnInit, OnDestroy {
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
  loading$ = this.store.select(selectFacturesClientLoading);
  submitting = false;

  // Page title
  get pageTitle(): string {
    return this.isEditMode ? 'Modifier Facture Client' : 'Nouvelle Facture Client';
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
      codeClient: ['', [Validators.required, Validators.maxLength(50)]],
      tauxRemiseGlobale: [0, [Validators.min(0), Validators.max(100)]],
      tauxRAS: [0, [Validators.min(0), Validators.max(100)]],
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
    this.store.dispatch(FacturesClientPageActions.loadFactureClient({ codeFacture: code }));

    // Wait for facture to load and populate form
    this.store.select(selectFactureClientByCode(code))
      .pipe(
        filter(facture => facture !== null),
        take(1),
        takeUntil(this.destroy$)
      )
      .subscribe(facture => {
        if (facture) {
          this.factureForm.patchValue({
            codeFacture: facture.codeFacture,
            dateFacture: facture.dateFacture ? new Date(facture.dateFacture) : new Date(),
            codeClient: facture.codeClient,
            tauxRemiseGlobale: facture.tauxRemiseGlobale || 0,
            tauxRAS: facture.tauxRAS || 0,
            notes: facture.notes || ''
          });
        }
      });
  }

  private subscribeToActions(): void {
    // Handle create success
    this.actions$.pipe(
      ofType(FacturesClientApiActions.createFactureClientSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/factures-client']);
    });

    // Handle update success
    this.actions$.pipe(
      ofType(FacturesClientApiActions.updateFactureClientSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/factures-client']);
    });

    // Handle failures
    this.actions$.pipe(
      ofType(
        FacturesClientApiActions.createFactureClientFailure,
        FacturesClientApiActions.updateFactureClientFailure
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
      const updateRequest: UpdateFactureClientRequest = {
        dateFacture: formValue.dateFacture,
        codeClient: formValue.codeClient,
        tauxRemiseGlobale: formValue.tauxRemiseGlobale,
        tauxRAS: formValue.tauxRAS,
        notes: formValue.notes || undefined
      };

      this.store.dispatch(FacturesClientPageActions.updateFactureClient({
        codeFacture: this.factureCode,
        facture: updateRequest
      }));
    } else {
      const createRequest: CreateFactureClientRequest = {
        codeFacture: formValue.codeFacture,
        dateFacture: formValue.dateFacture,
        codeClient: formValue.codeClient,
        tauxRemiseGlobale: formValue.tauxRemiseGlobale,
        tauxRAS: formValue.tauxRAS,
        notes: formValue.notes || undefined,
        lignes: []
      };

      this.store.dispatch(FacturesClientPageActions.createFactureClient({ facture: createRequest }));
    }
  }

  onCancel(): void {
    this.router.navigate(['/factures-client']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.factureForm.controls).forEach(key => {
      const control = this.factureForm.get(key);
      control?.markAsTouched();
      control?.markAsDirty();
    });
  }

  // Helper methods for template
  hasError(fieldName: string, errorType: string): boolean {
    const control = this.factureForm.get(fieldName);
    return control ? control.hasError(errorType) && control.touched : false;
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.factureForm.get(fieldName);
    return control ? control.invalid && control.touched : false;
  }
}
