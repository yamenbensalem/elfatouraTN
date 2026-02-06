import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, filter } from 'rxjs';

import { ClientsPageActions, ClientsApiActions } from '../../../../store/clients/clients.actions';
import { selectClientByCode, selectClientsLoading, selectClientsError } from '../../../../store/clients/clients.selectors';
import { Client } from '../../../../core/models/client.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Actions, ofType } from '@ngrx/effects';

@Component({
  selector: 'app-client-detail',
  standalone: false,
  templateUrl: './client-detail.component.html',
  styleUrls: ['./client-detail.component.scss']
})
export class ClientDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly actions$ = inject(Actions);
  private readonly destroy$ = new Subject<void>();

  // State
  client: Client | null = null;
  loading$ = this.store.select(selectClientsLoading);
  error$ = this.store.select(selectClientsError);
  clientCode: string | null = null;

  ngOnInit(): void {
    this.clientCode = this.route.snapshot.paramMap.get('code');
    
    if (this.clientCode) {
      this.loadClient(this.clientCode);
      this.subscribeToClient(this.clientCode);
    }

    this.subscribeToDeleteSuccess();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadClient(code: string): void {
    this.store.dispatch(ClientsPageActions.loadClient({ codeClient: code }));
  }

  private subscribeToClient(code: string): void {
    this.store.select(selectClientByCode(code))
      .pipe(
        filter(client => client !== null),
        takeUntil(this.destroy$)
      )
      .subscribe(client => {
        this.client = client;
      });
  }

  private subscribeToDeleteSuccess(): void {
    this.actions$.pipe(
      ofType(ClientsApiActions.deleteClientSuccess),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.router.navigate(['/clients']);
    });
  }

  goBack(): void {
    this.router.navigate(['/clients']);
  }

  editClient(): void {
    if (this.clientCode) {
      this.router.navigate(['/clients', this.clientCode, 'edit']);
    }
  }

  deleteClient(): void {
    if (!this.client) return;

    const dialogData: ConfirmDialogData = {
      title: 'Supprimer le client',
      message: `Êtes-vous sûr de vouloir supprimer le client "${this.client.nom}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.clientCode) {
        this.store.dispatch(ClientsPageActions.deleteClient({ codeClient: this.clientCode }));
      }
    });
  }

  retry(): void {
    if (this.clientCode) {
      this.loadClient(this.clientCode);
    }
  }
}
