import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { authGuard } from '../../core/guards/auth.guard';
import { ProduitListComponent } from './pages/produit-list/produit-list.component';
import { ProduitFormComponent } from './pages/produit-form/produit-form.component';
import { ProduitDetailComponent } from './pages/produit-detail/produit-detail.component';

const routes: Routes = [
  {
    path: '',
    component: ProduitListComponent,
    canActivate: [authGuard],
    title: 'Produits - GestCom'
  },
  {
    path: 'new',
    component: ProduitFormComponent,
    canActivate: [authGuard],
    title: 'Nouveau Produit - GestCom',
    data: { mode: 'create' }
  },
  {
    path: ':code',
    component: ProduitDetailComponent,
    canActivate: [authGuard],
    title: 'Détails Produit - GestCom'
  },
  {
    path: ':code/edit',
    component: ProduitFormComponent,
    canActivate: [authGuard],
    title: 'Modifier Produit - GestCom',
    data: { mode: 'edit' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProduitsRoutingModule { }
