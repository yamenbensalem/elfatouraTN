import { Component, EventEmitter, Output, Input } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';

import { AppState } from '../../store/app.state';
import { AuthActions } from '../../store/auth/auth.actions';
import { selectUser, selectUserFullName } from '../../store/auth/auth.selectors';
import { User } from '../../core/models/auth.models';

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  @Output() toggleSidenav = new EventEmitter<void>();
  @Input() title = 'GestCom';

  user$: Observable<User | null>;
  userFullName$: Observable<string | null>;

  constructor(private store: Store<AppState>) {
    this.user$ = this.store.select(selectUser);
    this.userFullName$ = this.store.select(selectUserFullName);
  }

  onToggleSidenav(): void {
    this.toggleSidenav.emit();
  }

  onLogout(): void {
    this.store.dispatch(AuthActions.logout());
  }
}
