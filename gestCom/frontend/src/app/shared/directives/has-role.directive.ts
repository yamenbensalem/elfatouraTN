import { Directive, Input, TemplateRef, ViewContainerRef, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';

@Directive({
  selector: '[hasRole]',
  standalone: false
})
export class HasRoleDirective implements OnInit, OnDestroy {
  private roles: string[] = [];
  private isVisible = false;
  private destroy$ = new Subject<void>();

  @Input()
  set hasRole(roles: string | string[]) {
    this.roles = Array.isArray(roles) ? roles : [roles];
    this.updateView();
  }

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateView();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateView(): void {
    const hasRole = this.checkRole();
    
    if (hasRole && !this.isVisible) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.isVisible = true;
    } else if (!hasRole && this.isVisible) {
      this.viewContainer.clear();
      this.isVisible = false;
    }
  }

  private checkRole(): boolean {
    if (!this.roles || this.roles.length === 0) {
      return true;
    }

    const currentUser = this.authService.getCurrentUser();
    if (!currentUser || !currentUser.role) {
      return false;
    }

    return this.roles.some(role => 
      currentUser.role.toLowerCase() === role.toLowerCase()
    );
  }
}
