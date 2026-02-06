import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { Subject, takeUntil, filter, take } from 'rxjs';

import { CommandesAchatPageActions, CommandesAchatApiActions } from '../../../../store/commandes-achat/commandes-achat.actions';
import { selectCommandeAchatByCode, selectCommandesAchatLoading } from '../../../../store/commandes-achat/commandes-achat.selectors';
import { CreateCommandeAchatRequest, UpdateCommandeAchatRequest } from '../../../../core/models/commande-achat.model';

@Component({
  selector: 'app-commande-achat-form',
  standalone: false,
  templateUrl: './commande-achat-form.component.html',
  styleUrls: ['./commande-achat-form.component.scss']
})
export class CommandeAchatFormComponent implements OnInit, OnDestroy {
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
  loading$ = this.store.select(selectCommandesAchatLoading);
  submitting = false;

  // Page title
  get pageTitle(): string {
    return this.isEditMode ? "Modifier Commande d'Achat" : "Nouvelle Commande d'Achat";
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
      codeFournisseur: ['', [Validators.required, Validators.maxLength(50)]],
      dateReceptionPrevue: [null],
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
    this.store.dispatch(CommandesAchatPageActions.loadCommandeAchat({ codeCommande: code }));

    this.store.select(selectCommandeAchatByCode(code))
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
            codeFournisseur: commande.codeFournisseur,
            dateReceptionPrevue: commande.dateReceptionPrevue ? new Date(commande.dateReceptionPrevue) : null,
            notes: commande.notes || ''
          });
        }
      });
  }

  private subscribeToActions(): void {
    this.actions$.pipe(
      ofType(CommandesAchatApiActions.createCommandeAchatSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/commandes-achat']);
    });

    this.actions$.pipe(
      ofType(CommandesAchatApiActions.updateCommandeAchatSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
      this.router.navigate(['/commandes-achat']);
    });

    this.actions$.pipe(
      ofType(
        CommandesAchatApiActions.createCommandeAchatFailure,
        CommandesAchatApiActions.updateCommandeAchatFailure
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
      const updateRequest: UpdateCommandeAchatRequest = {
        dateCommande: formValue.dateCommande || undefined,
        codeFournisseur: formValue.codeFournisseur || undefined,
        dateReceptionPrevue: formValue.dateReceptionPrevue || undefined,
        notes: formValue.notes || undefined
      };

      this.store.dispatch(CommandesAchatPageActions.updateCommandeAchat({
        codeCommande: this.commandeCode,
        commande: updateRequest
      }));
    } else {
      const createRequest: CreateCommandeAchatRequest = {
        codeCommande: formValue.codeCommande,
        dateCommande: formValue.dateCommande,
        codeFournisseur: formValue.codeFournisseur,
        dateReceptionPrevue: formValue.dateReceptionPrevue || undefined,
        notes: formValue.notes || undefined,
        lignes: []
      };

      this.store.dispatch(CommandesAchatPageActions.createCommandeAchat({ commande: createRequest }));
    }
  }

  onCancel(): void {
    this.router.navigate(['/commandes-achat']);
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
