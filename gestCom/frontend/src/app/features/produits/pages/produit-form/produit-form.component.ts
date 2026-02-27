import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { Subject, takeUntil, filter, take } from 'rxjs';

import { ProduitsPageActions, ProduitsApiActions } from '../../../../store/produits/produits.actions';
import { selectProduitByCode, selectProduitsLoading } from '../../../../store/produits/produits.selectors';
import { CreateProduitRequest, UpdateProduitRequest } from '../../../../core/models/produit.model';

@Component({
  selector: 'app-produit-form',
  standalone: false,
  templateUrl: './produit-form.component.html',
  styleUrls: ['./produit-form.component.scss']
})
export class ProduitFormComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // Form
  produitForm!: FormGroup;
  
  // State
  isEditMode = false;
  produitCode: string | null = null;
  loading$ = this.store.select(selectProduitsLoading);
  submitting = false;

  // Page title
  get pageTitle(): string {
    return this.isEditMode ? 'Modifier Produit' : 'Nouveau Produit';
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
    this.produitForm = this.fb.group({
      // indicates whether the item is a service (no stock management)
      isService: [false],

      codeProduit: ['', [Validators.required, Validators.maxLength(50)]],
      designation: ['', [Validators.required, Validators.maxLength(200)]],
      codeCategorie: ['', [Validators.maxLength(50)]],
      prixAchatHT: [0, [Validators.min(0)]],
      prixVenteHT: [0, [Validators.min(0)]],
      tauxTVA: [19, [Validators.min(0), Validators.max(100)]],
      tauxFODEC: [0, [Validators.min(0), Validators.max(100)]],
      unite: ['', [Validators.maxLength(20)]],
      codeBarres: ['', [Validators.maxLength(50)]],
      stockMinimum: [0, [Validators.min(0)]],
      stockMaximum: [0, [Validators.min(0)]],
      emplacement: ['', [Validators.maxLength(100)]],
      notes: ['', [Validators.maxLength(1000)]]
    });

    // reset stock values when toggling service flag
    this.produitForm.get('isService')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(isSvc => {
        if (isSvc) {
          this.produitForm.patchValue({ stockMinimum: 0, stockMaximum: 0, emplacement: '' });
        }
      });

  }

  private checkEditMode(): void {
    const code = this.route.snapshot.paramMap.get('code');
    const mode = this.route.snapshot.data['mode'];

    if (code && mode === 'edit') {
      this.isEditMode = true;
      this.produitCode = code;
      this.loadProduit(code);
      
      // Disable code field in edit mode
      this.produitForm.get('codeProduit')?.disable();
    }
  }

  private loadProduit(code: string): void {
    this.store.dispatch(ProduitsPageActions.loadProduit({ codeProduit: code }));
    
    // Wait for produit to load and populate form
    this.store.select(selectProduitByCode(code))
      .pipe(
        filter(produit => produit !== null),
        take(1),
        takeUntil(this.destroy$)
      )
      .subscribe(produit => {
        if (produit) {
          this.produitForm.patchValue({
            isService: produit['isService'] || false,
            codeProduit: produit.codeProduit,
            designation: produit.designation,
            codeCategorie: produit.codeCategorie || '',
            prixAchatHT: produit.prixAchatHT || 0,
            prixVenteHT: produit.prixVenteHT || 0,
            tauxTVA: produit.tauxTVA || 0,
            tauxFODEC: produit.tauxFODEC || 0,
            unite: produit.unite || '',
            codeBarres: produit.codeBarres || '',
            stockMinimum: produit.stockMinimum || 0,
            stockMaximum: produit.stockMaximum || 0,
            emplacement: produit.emplacement || '',
            notes: produit.notes || ''
          });
        }
      });
  }

  private subscribeToActions(): void {
    // Handle create success
    this.actions$.pipe(
      ofType(ProduitsApiActions.createProduitSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/produits']);
    });

    // Handle update success
    this.actions$.pipe(
      ofType(ProduitsApiActions.updateProduitSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/produits']);
    });

    // Handle failures
    this.actions$.pipe(
      ofType(
        ProduitsApiActions.createProduitFailure,
        ProduitsApiActions.updateProduitFailure
      ),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
    });
  }

  onSubmit(): void {
    if (this.produitForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.submitting = true;
    const formValue = this.produitForm.getRawValue();
    const isService = formValue.isService;

    // for services, ignore/zero out stock fields
    const stockMinimum = isService ? 0 : formValue.stockMinimum;
    const stockMaximum = isService ? 0 : formValue.stockMaximum;
    const emplacement = isService ? undefined : formValue.emplacement || undefined;

    if (this.isEditMode && this.produitCode) {
      const updateRequest: UpdateProduitRequest = {
        designation: formValue.designation,
        codeCategorie: formValue.codeCategorie || undefined,
        prixAchatHT: formValue.prixAchatHT,
        prixVenteHT: formValue.prixVenteHT,
        tauxTVA: formValue.tauxTVA,
        tauxFODEC: formValue.tauxFODEC,
        unite: formValue.unite || undefined,
        codeBarres: formValue.codeBarres || undefined,
        stockMinimum,
        stockMaximum,
        emplacement,
        notes: formValue.notes || undefined,
        // preserve flag, backend may ignore extra properties
        isService
      } as any;

      this.store.dispatch(ProduitsPageActions.updateProduit({
        codeProduit: this.produitCode,
        produit: updateRequest
      }));
    } else {
      const createRequest: CreateProduitRequest = {
        codeProduit: formValue.codeProduit,
        designation: formValue.designation,
        codeCategorie: formValue.codeCategorie || undefined,
        prixAchatHT: formValue.prixAchatHT,
        prixVenteHT: formValue.prixVenteHT,
        tauxTVA: formValue.tauxTVA,
        tauxFODEC: formValue.tauxFODEC,
        unite: formValue.unite || undefined,
        codeBarres: formValue.codeBarres || undefined,
        stockMinimum,
        stockMaximum,
        emplacement,
        notes: formValue.notes || undefined,
        isService
      } as any;

      this.store.dispatch(ProduitsPageActions.createProduit({ produit: createRequest }));
    }
  }

  onCancel(): void {
    this.router.navigate(['/produits']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.produitForm.controls).forEach(key => {
      const control = this.produitForm.get(key);
      control?.markAsTouched();
      control?.markAsDirty();
    });
  }

  // Helper methods for template
  hasError(fieldName: string, errorType: string): boolean {
    const control = this.produitForm.get(fieldName);
    return control ? control.hasError(errorType) && control.touched : false;
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.produitForm.get(fieldName);
    return control ? control.invalid && control.touched : false;
  }

  // read-only helpers
  get isService(): boolean {
    return this.produitForm.get('isService')?.value;
  }
}
