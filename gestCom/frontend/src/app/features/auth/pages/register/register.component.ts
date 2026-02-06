import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit, OnDestroy {
  registerForm!: FormGroup;
  hidePassword = true;
  hideConfirmPassword = true;
  isLoading = false;
  errorMessage: string | null = null;
  passwordStrength = 0;
  passwordStrengthLabel = '';
  
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.watchPasswordStrength();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.registerForm = this.fb.group({
      nom: ['', [Validators.required, Validators.minLength(2)]],
      prenom: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), this.passwordValidator]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, [Validators.requiredTrue]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  private watchPasswordStrength(): void {
    this.registerForm.get('password')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(password => {
        this.calculatePasswordStrength(password);
      });
  }

  private calculatePasswordStrength(password: string): void {
    if (!password) {
      this.passwordStrength = 0;
      this.passwordStrengthLabel = '';
      return;
    }

    let strength = 0;

    // Length check
    if (password.length >= 8) strength += 25;
    if (password.length >= 12) strength += 10;

    // Contains lowercase
    if (/[a-z]/.test(password)) strength += 15;

    // Contains uppercase
    if (/[A-Z]/.test(password)) strength += 15;

    // Contains numbers
    if (/\d/.test(password)) strength += 15;

    // Contains special characters
    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) strength += 20;

    this.passwordStrength = Math.min(strength, 100);
    this.passwordStrengthLabel = this.getStrengthLabel(this.passwordStrength);
  }

  private getStrengthLabel(strength: number): string {
    if (strength < 30) return 'Très faible';
    if (strength < 50) return 'Faible';
    if (strength < 70) return 'Moyen';
    if (strength < 90) return 'Fort';
    return 'Très fort';
  }

  getStrengthColor(): string {
    if (this.passwordStrength < 30) return 'warn';
    if (this.passwordStrength < 70) return 'accent';
    return 'primary';
  }

  private passwordValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.value;
    if (!password) return null;

    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumeric = /\d/.test(password);

    const valid = hasUpperCase && hasLowerCase && hasNumeric;
    return valid ? null : { passwordStrength: true };
  }

  private passwordMatchValidator(group: FormGroup): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    if (password && confirmPassword && password !== confirmPassword) {
      group.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.markFormAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    const { nom, prenom, email, password } = this.registerForm.value;

    this.authService.register({ nom, prenom, email, password })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.router.navigate(['/auth/login'], {
            queryParams: { registered: 'true' }
          });
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = this.getErrorMessage(error);
        }
      });
  }

  private markFormAsTouched(): void {
    Object.keys(this.registerForm.controls).forEach(key => {
      this.registerForm.get(key)?.markAsTouched();
    });
  }

  private getErrorMessage(error: any): string {
    if (error.status === 409) {
      return 'Un compte avec cet email existe déjà.';
    }
    if (error.status === 0) {
      return 'Impossible de se connecter au serveur. Veuillez réessayer plus tard.';
    }
    if (error.error?.message) {
      return error.error.message;
    }
    if (error.error?.errors) {
      return Object.values(error.error.errors).flat().join(' ');
    }
    return 'Une erreur est survenue. Veuillez réessayer.';
  }

  // Getters for form validation
  get nomControl() { return this.registerForm.get('nom'); }
  get prenomControl() { return this.registerForm.get('prenom'); }
  get emailControl() { return this.registerForm.get('email'); }
  get passwordControl() { return this.registerForm.get('password'); }
  get confirmPasswordControl() { return this.registerForm.get('confirmPassword'); }
  get acceptTermsControl() { return this.registerForm.get('acceptTerms'); }

  getNomError(): string {
    if (this.nomControl?.hasError('required')) return 'Le nom est requis';
    if (this.nomControl?.hasError('minlength')) return 'Le nom doit contenir au moins 2 caractères';
    return '';
  }

  getPrenomError(): string {
    if (this.prenomControl?.hasError('required')) return 'Le prénom est requis';
    if (this.prenomControl?.hasError('minlength')) return 'Le prénom doit contenir au moins 2 caractères';
    return '';
  }

  getEmailError(): string {
    if (this.emailControl?.hasError('required')) return 'L\'email est requis';
    if (this.emailControl?.hasError('email')) return 'Veuillez entrer un email valide';
    return '';
  }

  getPasswordError(): string {
    if (this.passwordControl?.hasError('required')) return 'Le mot de passe est requis';
    if (this.passwordControl?.hasError('minlength')) return 'Le mot de passe doit contenir au moins 8 caractères';
    if (this.passwordControl?.hasError('passwordStrength')) {
      return 'Le mot de passe doit contenir des majuscules, minuscules et chiffres';
    }
    return '';
  }

  getConfirmPasswordError(): string {
    if (this.confirmPasswordControl?.hasError('required')) return 'La confirmation est requise';
    if (this.confirmPasswordControl?.hasError('passwordMismatch')) {
      return 'Les mots de passe ne correspondent pas';
    }
    return '';
  }

  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.hideConfirmPassword = !this.hideConfirmPassword;
  }

  dismissError(): void {
    this.errorMessage = null;
  }
}
