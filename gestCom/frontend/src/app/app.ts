import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';

import { AppState } from './store/app.state';
import { AuthActions } from './store/auth/auth.actions';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrls: ['./app.scss']
})
export class App implements OnInit {
  
  constructor(private store: Store<AppState>) {}

  ngOnInit(): void {
    // Attempt to restore session from localStorage on app startup
    this.store.dispatch(AuthActions.restoreSession());
  }
}
