import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { authGuard } from '../../core/guards/auth.guard';
import { FactureClientListComponent } from './pages/facture-client-list/facture-client-list.component';
import { FactureClientFormComponent } from './pages/facture-client-form/facture-client-form.component';
import { FactureClientDetailComponent } from './pages/facture-client-detail/facture-client-detail.component';

const routes: Routes = [
  {
    path: '',
    component: FactureClientListComponent,
    canActivate: [authGuard],
    title: 'Factures Client - GestCom'
  },
  {
    path: 'new',
    component: FactureClientFormComponent,
    canActivate: [authGuard],
    title: 'Nouvelle Facture Client - GestCom',
    data: { mode: 'create' }
  },
  {
    path: ':code',
    component: FactureClientDetailComponent,
    canActivate: [authGuard],
    title: 'Détails Facture Client - GestCom'
  },
  {
    path: ':code/edit',
    component: FactureClientFormComponent,
    canActivate: [authGuard],
    title: 'Modifier Facture Client - GestCom',
    data: { mode: 'edit' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FacturesClientRoutingModule { }
