import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { authGuard } from '../../core/guards/auth.guard';
import { FournisseurListComponent } from './pages/fournisseur-list/fournisseur-list.component';
import { FournisseurFormComponent } from './pages/fournisseur-form/fournisseur-form.component';
import { FournisseurDetailComponent } from './pages/fournisseur-detail/fournisseur-detail.component';

const routes: Routes = [
  {
    path: '',
    component: FournisseurListComponent,
    canActivate: [authGuard],
    title: 'Fournisseurs - GestCom'
  },
  {
    path: 'new',
    component: FournisseurFormComponent,
    canActivate: [authGuard],
    title: 'Nouveau Fournisseur - GestCom',
    data: { mode: 'create' }
  },
  {
    path: ':code',
    component: FournisseurDetailComponent,
    canActivate: [authGuard],
    title: 'Détails Fournisseur - GestCom'
  },
  {
    path: ':code/edit',
    component: FournisseurFormComponent,
    canActivate: [authGuard],
    title: 'Modifier Fournisseur - GestCom',
    data: { mode: 'edit' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FournisseursRoutingModule { }
