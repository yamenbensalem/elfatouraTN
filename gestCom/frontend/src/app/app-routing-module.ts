import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { authGuard, guestGuard } from './core/guards/auth.guard';

const routes: Routes = [
  // Default redirect to dashboard
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  
  // Auth routes (accessible only to guests)
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  
  // Protected routes with main layout
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      // Dashboard
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule)
      },
      
      // Clients
      {
        path: 'clients',
        loadChildren: () => import('./features/clients/clients.module').then(m => m.ClientsModule)
      },
      
      // Placeholder routes for future modules - redirect to dashboard for now
      { path: 'devis', redirectTo: 'dashboard' },
      { path: 'commandes-vente', redirectTo: 'dashboard' },
      { path: 'factures-client', redirectTo: 'dashboard' },
      { path: 'fournisseurs', redirectTo: 'dashboard' },
      { path: 'commandes-achat', redirectTo: 'dashboard' },
      { path: 'factures-fournisseur', redirectTo: 'dashboard' },
      { path: 'produits', redirectTo: 'dashboard' },
      { path: 'categories', redirectTo: 'dashboard' },
      { path: 'entreprise', redirectTo: 'dashboard' },
      { path: 'parametres', redirectTo: 'dashboard' },
      { path: 'profil', redirectTo: 'dashboard' }
    ]
  },
  
  // Wildcard - redirect to dashboard
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    scrollPositionRestoration: 'enabled',
    anchorScrolling: 'enabled',
    onSameUrlNavigation: 'reload'
  })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
