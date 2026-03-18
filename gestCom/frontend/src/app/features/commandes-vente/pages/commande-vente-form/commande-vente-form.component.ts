import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
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
      numeroCommande: ['', [Validators.required, Validators.maxLength(50)]],
      dateCommande: [new Date(), [Validators.required]],
      codeClient: ['', [Validators.required, Validators.maxLength(50)]],
      dateLivraisonPrevue: [null],
      notes: ['', [Validators.maxLength(1000)]],
      lignes: this.fb.array([])
    });
    this.addLigne();
  }

  get lignes(): FormArray {
    return this.commandeForm.get('lignes') as FormArray;
  }

  addLigne(): void {
    const ligneForm = this.fb.group({
      codeProduit: ['', Validators.required],
      quantite: [1, [Validators.required, Validators.min(0.01)]],
      prixUnitaireHT: [0, [Validators.required, Validators.min(0)]],
      tauxRemise: [0]
    });
    this.lignes.push(ligneForm);
  }

  removeLigne(index: number): void {
    this.lignes.removeAt(index);
  }

  private checkEditMode(): void {
    const code = this.route.snapshot.paramMap.get('code');
    const mode = this.route.snapshot.data['mode'];

    if (code && mode === 'edit') {
      this.isEditMode = true;
      this.commandeCode = code;
      this.loadCommande(code);

      // Disable code field in edit mode
      this.commandeForm.get('numeroCommande')?.disable();
    }
  }

  private loadCommande(code: string): void {
    this.store.dispatch(CommandesVentePageActions.loadCommandeVente({ numeroCommande: code }));

    this.store.select(selectCommandeVenteByCode(code))
      .pipe(
        filter(commande => commande !== null),
        take(1),
        takeUntil(this.destroy$)
      )
      .subscribe(commande => {
        if (commande) {
          this.commandeForm.patchValue({
            numeroCommande: commande.numeroCommande,
            dateCommande: commande.dateCommande ? new Date(commande.dateCommande) : null,
            codeClient: commande.codeClient,
            dateLivraisonPrevue: commande.dateLivraisonPrevue ? new Date(commande.dateLivraisonPrevue) : null,
            notes: commande.notes || ''
          });

          this.lignes.clear();
          if (commande.lignes && commande.lignes.length > 0) {
            commande.lignes.forEach(ligne => {
              this.lignes.push(this.fb.group({
                codeProduit: [ligne.codeProduit, Validators.required],
                quantite: [ligne.quantite, [Validators.required, Validators.min(0.01)]],
                prixUnitaireHT: [ligne.prixUnitaireHT, [Validators.required, Validators.min(0)]],
                tauxRemise: [ligne.tauxRemise || 0]
              }));
            });
          } else {
             this.addLigne();
          }
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
    const preparedLignes = formValue.lignes.map((l: any) => ({
        codeProduit: l.codeProduit,
        quantite: l.quantite,
        prixUnitaireHT: l.prixUnitaireHT,
        tauxRemise: l.tauxRemise || 0,
        tauxTVA: 19 // Defaulting to 19% for dev context
    }));

    if (this.isEditMode && this.commandeCode) {
      const updateRequest: any = {
        dateCommande: formValue.dateCommande || undefined,
        codeClient: formValue.codeClient || undefined,
        dateLivraisonPrevue: formValue.dateLivraisonPrevue || undefined,
        notes: formValue.notes || undefined,
        lignes: preparedLignes
      };

      this.store.dispatch(CommandesVentePageActions.updateCommandeVente({
        numeroCommande: this.commandeCode,
        commande: updateRequest
      }));
    } else {
      const createRequest: any = {
        numeroCommande: formValue.numeroCommande,
        dateCommande: formValue.dateCommande,
        codeClient: formValue.codeClient,
        dateLivraisonPrevue: formValue.dateLivraisonPrevue || undefined,
        notes: formValue.notes || undefined,
        lignes: preparedLignes
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
