import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  loginForm!: FormGroup;
  hidePassword = true;
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.checkRememberedEmail();
    this.checkSuccessMessage();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  private checkRememberedEmail(): void {
    const rememberedEmail = localStorage.getItem('gestcom_remembered_email');
    if (rememberedEmail) {
      this.loginForm.patchValue({
        email: rememberedEmail,
        rememberMe: true
      });
    }
  }

  private checkSuccessMessage(): void {
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        if (params['registered']) {
          this.successMessage = 'Inscription réussie ! Vous pouvez maintenant vous connecter.';
        }
      });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.markFormAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const { email, password, rememberMe } = this.loginForm.value;

    // Handle remember me
    if (rememberMe) {
      localStorage.setItem('gestcom_remembered_email', email);
    } else {
      localStorage.removeItem('gestcom_remembered_email');
    }

    this.authService.login(email, password)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = this.getErrorMessage(error);
        }
      });
  }

  private markFormAsTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      this.loginForm.get(key)?.markAsTouched();
    });
  }

  private getErrorMessage(error: any): string {
    if (error.status === 401) {
      return 'Email ou mot de passe incorrect.';
    }
    if (error.status === 0) {
      return 'Impossible de se connecter au serveur. Veuillez réessayer plus tard.';
    }
    if (error.error?.message) {
      return error.error.message;
    }
    return 'Une erreur est survenue. Veuillez réessayer.';
  }

  // Getters for form validation
  get emailControl() {
    return this.loginForm.get('email');
  }

  get passwordControl() {
    return this.loginForm.get('password');
  }

  getEmailError(): string {
    if (this.emailControl?.hasError('required')) {
      return 'L\'email est requis';
    }
    if (this.emailControl?.hasError('email')) {
      return 'Veuillez entrer un email valide';
    }
    return '';
  }

  getPasswordError(): string {
    if (this.passwordControl?.hasError('required')) {
      return 'Le mot de passe est requis';
    }
    if (this.passwordControl?.hasError('minlength')) {
      return 'Le mot de passe doit contenir au moins 6 caractères';
    }
    return '';
  }

  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }

  dismissError(): void {
    this.errorMessage = null;
  }

  dismissSuccess(): void {
    this.successMessage = null;
  }
}
