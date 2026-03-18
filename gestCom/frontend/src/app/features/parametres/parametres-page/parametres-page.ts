import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ParametresService, ParametresDecimalesDto } from '../parametres.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-parametres-page',
  standalone: false,
  templateUrl: './parametres-page.html',
  styleUrls: ['./parametres-page.scss']
})
export class ParametresPage implements OnInit {
  parametresForm: FormGroup;
  isLoading = false;
  isSaving = false;

  constructor(
    private fb: FormBuilder,
    private parametresService: ParametresService,
    private snackBar: MatSnackBar
  ) {
    this.parametresForm = this.fb.group({
      decimalesQuantite: [0, [Validators.required, Validators.min(0), Validators.max(5)]],
      decimalesPrix: [0, [Validators.required, Validators.min(0), Validators.max(5)]],
      decimalesMontant: [0, [Validators.required, Validators.min(0), Validators.max(5)]],
      decimalesTva: [0, [Validators.required, Validators.min(0), Validators.max(5)]]
    });
  }

  ngOnInit(): void {
    this.loadParametres();
  }

  loadParametres(): void {
    this.isLoading = true;
    this.parametresService.getParametresDecimales().subscribe({
      next: (data) => {
        this.parametresForm.patchValue(data);
        this.isLoading = false;
      },
      error: (err) => {
        this.snackBar.open('Erreur lors du chargement des paramètres', 'Fermer', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.parametresForm.valid) {
      this.isSaving = true;
      const data: ParametresDecimalesDto = this.parametresForm.getRawValue();
      this.parametresService.updateParametresDecimales(data).subscribe({
        next: () => {
          this.snackBar.open('Paramètres mis à jour avec succès', 'Fermer', { duration: 3000 });
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
