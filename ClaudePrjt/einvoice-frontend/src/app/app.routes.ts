import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'invoices',
    pathMatch: 'full'
  },
  {
    path: 'invoices',
    loadComponent: () => import('./features/invoices/invoice-list.component')
      .then(m => m.InvoiceListComponent)
  },
  {
    path: 'invoices/new',
    loadComponent: () => import('./features/invoices/invoice-form.component')
      .then(m => m.InvoiceFormComponent)
  },
  {
    path: 'invoices/:id',
    loadComponent: () => import('./features/invoices/invoice-detail.component')
      .then(m => m.InvoiceDetailComponent)
  },
  {
    path: 'clients',
    loadComponent: () => import('./features/clients/client-list.component')
      .then(m => m.ClientListComponent)
  },
  {
    path: 'clients/new',
    loadComponent: () => import('./features/clients/client-form.component')
      .then(m => m.ClientFormComponent)
  },
  {
    path: 'clients/:id',
    loadComponent: () => import('./features/clients/client-form.component')
      .then(m => m.ClientFormComponent)
  }
];
