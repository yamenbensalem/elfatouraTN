import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { authGuard } from '../../core/guards/auth.guard';
import { CommandeVenteListComponent } from './pages/commande-vente-list/commande-vente-list.component';
import { CommandeVenteFormComponent } from './pages/commande-vente-form/commande-vente-form.component';
import { CommandeVenteDetailComponent } from './pages/commande-vente-detail/commande-vente-detail.component';

const routes: Routes = [
  {
    path: '',
    component: CommandeVenteListComponent,
    canActivate: [authGuard],
    title: 'Commandes de Vente - GestCom'
  },
  {
    path: 'new',
    component: CommandeVenteFormComponent,
    canActivate: [authGuard],
    title: 'Nouvelle Commande de Vente - GestCom',
    data: { mode: 'create' }
  },
  {
    path: ':code',
    component: CommandeVenteDetailComponent,
    canActivate: [authGuard],
    title: 'Détails Commande de Vente - GestCom'
  },
  {
    path: ':code/edit',
    component: CommandeVenteFormComponent,
    canActivate: [authGuard],
    title: 'Modifier Commande de Vente - GestCom',
    data: { mode: 'edit' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CommandesVenteRoutingModule { }
