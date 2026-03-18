import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EntrepriseService, EntrepriseDto } from '../entreprise.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-entreprise-page',
  standalone: false,
  templateUrl: './entreprise-page.html',
  styleUrls: ['./entreprise-page.scss']
})
export class EntreprisePage implements OnInit {
  entrepriseForm: FormGroup;
  isLoading = false;
  isSaving = false;

  constructor(
    private fb: FormBuilder,
    private entrepriseService: EntrepriseService,
    private snackBar: MatSnackBar
  ) {
    this.entrepriseForm = this.fb.group({
      codeEntreprise: [{ value: '', disabled: true }, Validators.required],
      raisonSociale: ['', Validators.required],
      matriculeFiscal: ['', Validators.required],
      adresse: [''],
      codePostal: [''],
      ville: [''],
      telephone: [''],
      fax: [''],
      email: ['', Validators.email],
      siteWeb: [''],
      rib: [''],
      nomBanque: [''],
      codeDevise: ['']
    });
  }

  ngOnInit(): void {
    this.loadEntreprise();
  }

  loadEntreprise(): void {
    this.isLoading = true;
    this.entrepriseService.getEntreprise().subscribe({
      next: (data) => {
        this.entrepriseForm.patchValue(data);
        this.isLoading = false;
      },
      error: (err) => {
        this.snackBar.open('Erreur lors du chargement des données', 'Fermer', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.entrepriseForm.valid) {
      this.isSaving = true;
      const data: EntrepriseDto = this.entrepriseForm.getRawValue();
      this.entrepriseService.updateEntreprise(data).subscribe({
        next: () => {
          this.snackBar.open('Entreprise mise à jour avec succès', 'Fermer', { duration: 3000 });
          this.isSaving = false;
        },
        error: (err) => {
          this.snackBar.open('Erreur lors de la mise à jour', 'Fermer', { duration: 3000 });
          this.isSaving = false;
        }
      });
    }
  }
}
