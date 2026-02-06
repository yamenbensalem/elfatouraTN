import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { Subject, takeUntil, filter, take } from 'rxjs';

import { DevisPageActions, DevisApiActions } from '../../../../store/devis/devis.actions';
import { selectDevisByCode, selectDevisLoading } from '../../../../store/devis/devis.selectors';
import { CreateDevisRequest, UpdateDevisRequest } from '../../../../core/models/devis.model';

@Component({
  selector: 'app-devis-form',
  standalone: false,
  templateUrl: './devis-form.component.html',
  styleUrls: ['./devis-form.component.scss']
})
export class DevisFormComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // Form
  devisForm!: FormGroup;

  // State
  isEditMode = false;
  devisCode: string | null = null;
  loading$ = this.store.select(selectDevisLoading);
  submitting = false;

  // Page title
  get pageTitle(): string {
    return this.isEditMode ? 'Modifier Devis' : 'Nouveau Devis';
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
    this.devisForm = this.fb.group({
      codeDevis: ['', [Validators.required, Validators.maxLength(50)]],
      dateDevis: [new Date(), [Validators.required]],
      codeClient: ['', [Validators.required, Validators.maxLength(50)]],
      objet: ['', [Validators.maxLength(500)]],
      tauxRemiseGlobale: [0, [Validators.min(0), Validators.max(100)]],
      validite: [30, [Validators.min(1)]],
      notes: ['', [Validators.maxLength(1000)]]
    });
  }

  private checkEditMode(): void {
    const code = this.route.snapshot.paramMap.get('code');
    const mode = this.route.snapshot.data['mode'];

    if (code && mode === 'edit') {
      this.isEditMode = true;
      this.devisCode = code;
      this.loadDevis(code);

      // Disable code field in edit mode
      this.devisForm.get('codeDevis')?.disable();
    }
  }

  private loadDevis(code: string): void {
    this.store.dispatch(DevisPageActions.loadDevis({ codeDevis: code }));

    // Wait for devis to load and populate form
    this.store.select(selectDevisByCode(code))
      .pipe(
        filter(devis => devis !== null),
        take(1),
        takeUntil(this.destroy$)
      )
      .subscribe(devis => {
        if (devis) {
          this.devisForm.patchValue({
            codeDevis: devis.codeDevis,
            dateDevis: devis.dateDevis ? new Date(devis.dateDevis) : new Date(),
            codeClient: devis.codeClient,
            objet: devis.objet || '',
            tauxRemiseGlobale: devis.tauxRemiseGlobale || 0,
            validite: devis.validite || 30,
            notes: devis.notes || ''
          });
        }
      });
  }

  private subscribeToActions(): void {
    // Handle create success
    this.actions$.pipe(
      ofType(DevisApiActions.createDevisSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/devis']);
    });

    // Handle update success
    this.actions$.pipe(
      ofType(DevisApiActions.updateDevisSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/devis']);
    });

    // Handle failures
    this.actions$.pipe(
      ofType(
        DevisApiActions.createDevisFailure,
        DevisApiActions.updateDevisFailure
      ),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
    });
  }

  onSubmit(): void {
    if (this.devisForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.submitting = true;
    const formValue = this.devisForm.getRawValue();

    if (this.isEditMode && this.devisCode) {
      const updateRequest: UpdateDevisRequest = {
        dateDevis: formValue.dateDevis,
        codeClient: formValue.codeClient,
        objet: formValue.objet || undefined,
        tauxRemiseGlobale: formValue.tauxRemiseGlobale,
        validite: formValue.validite,
        notes: formValue.notes || undefined
      };

      this.store.dispatch(DevisPageActions.updateDevis({
        codeDevis: this.devisCode,
        devis: updateRequest
      }));
    } else {
      const createRequest: CreateDevisRequest = {
        codeDevis: formValue.codeDevis,
        dateDevis: formValue.dateDevis,
        codeClient: formValue.codeClient,
        objet: formValue.objet || undefined,
        tauxRemiseGlobale: formValue.tauxRemiseGlobale,
        validite: formValue.validite,
        notes: formValue.notes || undefined,
        lignes: []
      };

      this.store.dispatch(DevisPageActions.createDevis({ devis: createRequest }));
    }
  }

  onCancel(): void {
    this.router.navigate(['/devis']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.devisForm.controls).forEach(key => {
      const control = this.devisForm.get(key);
      control?.markAsTouched();
      control?.markAsDirty();
    });
  }

  // Helper methods for template
  hasError(fieldName: string, errorType: string): boolean {
    const control = this.devisForm.get(fieldName);
    return control ? control.hasError(errorType) && control.touched : false;
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.devisForm.get(fieldName);
    return control ? control.invalid && control.touched : false;
  }
}
