import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { Subject, takeUntil, filter, take } from 'rxjs';

import { FournisseursPageActions, FournisseursApiActions } from '../../../../store/fournisseurs/fournisseurs.actions';
import { selectFournisseurByCode, selectFournisseursLoading } from '../../../../store/fournisseurs/fournisseurs.selectors';
import { CreateFournisseurRequest, UpdateFournisseurRequest } from '../../../../core/models/fournisseur.model';

@Component({
  selector: 'app-fournisseur-form',
  standalone: false,
  templateUrl: './fournisseur-form.component.html',
  styleUrls: ['./fournisseur-form.component.scss']
})
export class FournisseurFormComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // Form
  fournisseurForm!: FormGroup;
  
  // State
  isEditMode = false;
  fournisseurCode: string | null = null;
  loading$ = this.store.select(selectFournisseursLoading);
  submitting = false;

  // Page title
  get pageTitle(): string {
    return this.isEditMode ? 'Modifier Fournisseur' : 'Nouveau Fournisseur';
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
    this.fournisseurForm = this.fb.group({
      codeFournisseur: ['', [Validators.required, Validators.maxLength(50)]],
      raisonSociale: ['', [Validators.required, Validators.maxLength(200)]],
      adresse: ['', [Validators.maxLength(500)]],
      codePostal: ['', [Validators.maxLength(10)]],
      ville: ['', [Validators.maxLength(100)]],
      telephone: ['', [Validators.maxLength(20)]],
      fax: ['', [Validators.maxLength(20)]],
      email: ['', [Validators.email, Validators.maxLength(100)]],
      matriculeFiscale: ['', [Validators.maxLength(50)]],
      exonere: [false],
      notes: ['', [Validators.maxLength(1000)]]
    });
  }

  private checkEditMode(): void {
    const code = this.route.snapshot.paramMap.get('code');
    const mode = this.route.snapshot.data['mode'];

    if (code && mode === 'edit') {
      this.isEditMode = true;
      this.fournisseurCode = code;
      this.loadFournisseur(code);
      
      // Disable code field in edit mode
      this.fournisseurForm.get('codeFournisseur')?.disable();
    }
  }

  private loadFournisseur(code: string): void {
    this.store.dispatch(FournisseursPageActions.loadFournisseur({ codeFournisseur: code }));
    
    // Wait for fournisseur to load and populate form
    this.store.select(selectFournisseurByCode(code))
      .pipe(
        filter(fournisseur => fournisseur !== null),
        take(1),
        takeUntil(this.destroy$)
      )
      .subscribe(fournisseur => {
        if (fournisseur) {
          this.fournisseurForm.patchValue({
            codeFournisseur: fournisseur.codeFournisseur,
            raisonSociale: fournisseur.raisonSociale,
            adresse: fournisseur.adresse || '',
            codePostal: fournisseur.codePostal || '',
            ville: fournisseur.ville || '',
            telephone: fournisseur.telephone || '',
            fax: fournisseur.fax || '',
            email: fournisseur.email || '',
            matriculeFiscale: fournisseur.matriculeFiscale || '',
            exonere: fournisseur.exonere || false,
            notes: fournisseur.notes || ''
          });
        }
      });
  }

  private subscribeToActions(): void {
    // Handle create success
    this.actions$.pipe(
      ofType(FournisseursApiActions.createFournisseurSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/fournisseurs']);
    });

    // Handle update success
    this.actions$.pipe(
      ofType(FournisseursApiActions.updateFournisseurSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/fournisseurs']);
    });

    // Handle failures
    this.actions$.pipe(
      ofType(
        FournisseursApiActions.createFournisseurFailure,
        FournisseursApiActions.updateFournisseurFailure
      ),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
    });
  }

  onSubmit(): void {
    if (this.fournisseurForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.submitting = true;
    const formValue = this.fournisseurForm.getRawValue();

    if (this.isEditMode && this.fournisseurCode) {
      const updateRequest: UpdateFournisseurRequest = {
        raisonSociale: formValue.raisonSociale,
        adresse: formValue.adresse || undefined,
        codePostal: formValue.codePostal || undefined,
        ville: formValue.ville || undefined,
        telephone: formValue.telephone || undefined,
        fax: formValue.fax || undefined,
        email: formValue.email || undefined,
        matriculeFiscale: formValue.matriculeFiscale || undefined,
        exonere: formValue.exonere,
        notes: formValue.notes || undefined
      };

      this.store.dispatch(FournisseursPageActions.updateFournisseur({
        codeFournisseur: this.fournisseurCode,
        fournisseur: updateRequest
      }));
    } else {
      const createRequest: CreateFournisseurRequest = {
        codeFournisseur: formValue.codeFournisseur,
        raisonSociale: formValue.raisonSociale,
        adresse: formValue.adresse || undefined,
        codePostal: formValue.codePostal || undefined,
        ville: formValue.ville || undefined,
        telephone: formValue.telephone || undefined,
        fax: formValue.fax || undefined,
        email: formValue.email || undefined,
        matriculeFiscale: formValue.matriculeFiscale || undefined,
        exonere: formValue.exonere,
        notes: formValue.notes || undefined
      };

      this.store.dispatch(FournisseursPageActions.createFournisseur({ fournisseur: createRequest }));
    }
  }

  onCancel(): void {
    this.router.navigate(['/fournisseurs']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.fournisseurForm.controls).forEach(key => {
      const control = this.fournisseurForm.get(key);
      control?.markAsTouched();
      control?.markAsDirty();
    });
  }

  // Helper methods for template
  hasError(fieldName: string, errorType: string): boolean {
    const control = this.fournisseurForm.get(fieldName);
    return control ? control.hasError(errorType) && control.touched : false;
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.fournisseurForm.get(fieldName);
    return control ? control.invalid && control.touched : false;
  }
}
