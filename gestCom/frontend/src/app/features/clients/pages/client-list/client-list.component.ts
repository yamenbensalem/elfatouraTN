import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { ClientsPageActions } from '../../../../store/clients/clients.actions';
import { 
  selectClientsListViewModel,
  selectAllClients 
} from '../../../../store/clients/clients.selectors';
import { Client } from '../../../../core/models/client.model';
import { TableColumn } from '../../../../shared/components/data-table/data-table.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-client-list',
  standalone: false,
  templateUrl: './client-list.component.html',
  styleUrls: ['./client-list.component.scss']
})
export class ClientListComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly destroy$ = new Subject<void>();

  // View model from store
  vm$ = this.store.select(selectClientsListViewModel);
  clients$ = this.store.select(selectAllClients);

  // Table configuration
  columns: TableColumn[] = [
    { key: 'codeClient', header: 'Code Client', sortable: true },
    { key: 'raisonSociale', header: 'Raison Sociale', sortable: true },
    { key: 'ville', header: 'Ville', sortable: true },
    { key: 'telephone', header: 'Téléphone', sortable: false },
    { key: 'email', header: 'Email', sortable: false }
  ];

  displayedColumns: string[] = ['codeClient', 'raisonSociale', 'ville', 'telephone', 'email', 'actions'];

  // Local state
  filteredClients: Client[] = [];
  allClients: Client[] = [];
  searchTerm: string = '';

  ngOnInit(): void {
    this.loadClients();
    
    // Subscribe to clients for local filtering
    this.clients$.pipe(takeUntil(this.destroy$)).subscribe(clients => {
      this.allClients = clients;
      this.applyFilter();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadClients(): void {
    this.store.dispatch(ClientsPageActions.loadClients({ params: { pageNumber: 1, pageSize: 100 } }));
  }

  onSearch(searchTerm: string): void {
    this.searchTerm = searchTerm.toLowerCase();
    this.applyFilter();
  }

  private applyFilter(): void {
    if (!this.searchTerm) {
      this.filteredClients = [...this.allClients];
    } else {
      this.filteredClients = this.allClients.filter(client =>
        client.codeClient.toLowerCase().includes(this.searchTerm) ||
        client.raisonSociale.toLowerCase().includes(this.searchTerm) ||
        (client.ville && client.ville.toLowerCase().includes(this.searchTerm)) ||
        (client.email && client.email.toLowerCase().includes(this.searchTerm)) ||
        (client.telephone && client.telephone.includes(this.searchTerm))
      );
    }
  }

  onPageChange(event: { pageNumber: number; pageSize: number }): void {
    this.store.dispatch(ClientsPageActions.setPage({
      pageNumber: event.pageNumber,
      pageSize: event.pageSize
    }));
    this.store.dispatch(ClientsPageActions.loadClients({}));
  }

  onSortChange(event: { sortBy: string; sortDescending: boolean }): void {
    this.store.dispatch(ClientsPageActions.loadClients({
      params: {
        pageNumber: 1,
        pageSize: 10,
        sortBy: event.sortBy,
        sortDescending: event.sortDescending
      }
    }));
  }

  navigateToNew(): void {
    this.router.navigate(['/clients/new']);
  }

  viewClient(client: Client): void {
    this.router.navigate(['/clients', client.codeClient]);
  }

  editClient(client: Client): void {
    this.router.navigate(['/clients', client.codeClient, 'edit']);
  }

  deleteClient(client: Client): void {
    const dialogData: ConfirmDialogData = {
      title: 'Supprimer le client',
      message: `Êtes-vous sûr de vouloir supprimer le client "${client.raisonSociale}" ?`,
      confirmText: 'Supprimer',
      cancelText: 'Annuler'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.store.dispatch(ClientsPageActions.deleteClient({ codeClient: client.codeClient }));
      }
    });
  }

  exportToExcel(): void {
    // TODO: Implement Excel export functionality
    console.log('Export to Excel - Coming soon');
  }
}
