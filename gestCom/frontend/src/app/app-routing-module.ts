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
      
      // Devis
      {
        path: 'devis',
        loadChildren: () => import('./features/devis/devis.module').then(m => m.DevisModule)
      },
      {
        path: 'commandes-vente',
        loadChildren: () => import('./features/commandes-vente/commandes-vente.module').then(m => m.CommandesVenteModule)
      },
      // Factures Client
      {
        path: 'factures-client',
        loadChildren: () => import('./features/factures-client/factures-client.module').then(m => m.FacturesClientModule)
      },
      // Fournisseurs
      {
        path: 'fournisseurs',
        loadChildren: () => import('./features/fournisseurs/fournisseurs.module').then(m => m.FournisseursModule)
      },
      {
        path: 'commandes-achat',
        loadChildren: () => import('./features/commandes-achat/commandes-achat.module').then(m => m.CommandesAchatModule)
      },
      {
        path: 'factures-fournisseur',
        loadChildren: () => import('./features/factures-fournisseur/factures-fournisseur.module').then(m => m.FacturesFournisseurModule)
      },
      // Produits
      {
        path: 'produits',
        loadChildren: () => import('./features/produits/produits.module').then(m => m.ProduitsModule)
      },
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
