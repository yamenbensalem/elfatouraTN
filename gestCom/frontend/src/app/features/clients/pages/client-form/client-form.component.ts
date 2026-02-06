import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { Subject, takeUntil, filter, take } from 'rxjs';

import { ClientsPageActions, ClientsApiActions } from '../../../../store/clients/clients.actions';
import { selectClientByCode, selectClientsLoading } from '../../../../store/clients/clients.selectors';
import { CreateClientRequest, UpdateClientRequest } from '../../../../core/models/client.model';

@Component({
  selector: 'app-client-form',
  standalone: false,
  templateUrl: './client-form.component.html',
  styleUrls: ['./client-form.component.scss']
})
export class ClientFormComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // Form
  clientForm!: FormGroup;
  
  // State
  isEditMode = false;
  clientCode: string | null = null;
  loading$ = this.store.select(selectClientsLoading);
  submitting = false;

  // Page title
  get pageTitle(): string {
    return this.isEditMode ? 'Modifier Client' : 'Nouveau Client';
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
    this.clientForm = this.fb.group({
      codeClient: ['', [Validators.required, Validators.maxLength(50)]],
      nom: ['', [Validators.required, Validators.maxLength(200)]],
      adresse: ['', [Validators.maxLength(500)]],
      codePostal: ['', [Validators.maxLength(10)]],
      ville: ['', [Validators.maxLength(100)]],
      telephone: ['', [Validators.maxLength(20)]],
      fax: ['', [Validators.maxLength(20)]],
      email: ['', [Validators.email, Validators.maxLength(100)]],
      matriculeFiscale: ['', [Validators.maxLength(50)]],
      exonere: [false],
      timbreFiscal: [false],
      notes: ['', [Validators.maxLength(1000)]]
    });
  }

  private checkEditMode(): void {
    const code = this.route.snapshot.paramMap.get('code');
    const mode = this.route.snapshot.data['mode'];

    if (code && mode === 'edit') {
      this.isEditMode = true;
      this.clientCode = code;
      this.loadClient(code);
      
      // Disable code field in edit mode
      this.clientForm.get('codeClient')?.disable();
    }
  }

  private loadClient(code: string): void {
    this.store.dispatch(ClientsPageActions.loadClient({ codeClient: code }));
    
    // Wait for client to load and populate form
    this.store.select(selectClientByCode(code))
      .pipe(
        filter(client => client !== null),
        take(1),
        takeUntil(this.destroy$)
      )
      .subscribe(client => {
        if (client) {
          this.clientForm.patchValue({
            codeClient: client.codeClient,
            nom: client.nom,
            adresse: client.adresse || '',
            codePostal: client.codePostal || '',
            ville: client.ville || '',
            telephone: client.telephone || '',
            fax: client.fax || '',
            email: client.email || '',
            matriculeFiscale: client.matriculeFiscale || '',
            exonere: client.exonere || false,
            timbreFiscal: false, // Not in model, but requested in UI
            notes: client.notes || ''
          });
        }
      });
  }

  private subscribeToActions(): void {
    // Handle create success
    this.actions$.pipe(
      ofType(ClientsApiActions.createClientSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/clients']);
    });

    // Handle update success
    this.actions$.pipe(
      ofType(ClientsApiActions.updateClientSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/clients']);
    });

    // Handle failures
    this.actions$.pipe(
      ofType(
        ClientsApiActions.createClientFailure,
        ClientsApiActions.updateClientFailure
      ),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
    });
  }

  onSubmit(): void {
    if (this.clientForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.submitting = true;
    const formValue = this.clientForm.getRawValue();

    if (this.isEditMode && this.clientCode) {
      const updateRequest: UpdateClientRequest = {
        nom: formValue.nom,
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

      this.store.dispatch(ClientsPageActions.updateClient({
        codeClient: this.clientCode,
        client: updateRequest
      }));
    } else {
      const createRequest: CreateClientRequest = {
        codeClient: formValue.codeClient,
        nom: formValue.nom,
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

      this.store.dispatch(ClientsPageActions.createClient({ client: createRequest }));
    }
  }

  onCancel(): void {
    this.router.navigate(['/clients']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.clientForm.controls).forEach(key => {
      const control = this.clientForm.get(key);
      control?.markAsTouched();
      control?.markAsDirty();
    });
  }

  // Helper methods for template
  hasError(fieldName: string, errorType: string): boolean {
    const control = this.clientForm.get(fieldName);
    return control ? control.hasError(errorType) && control.touched : false;
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.clientForm.get(fieldName);
    return control ? control.invalid && control.touched : false;
  }
}
