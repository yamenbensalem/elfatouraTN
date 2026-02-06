import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { authGuard } from '../../core/guards/auth.guard';
import { CommandeAchatListComponent } from './pages/commande-achat-list/commande-achat-list.component';
import { CommandeAchatFormComponent } from './pages/commande-achat-form/commande-achat-form.component';
import { CommandeAchatDetailComponent } from './pages/commande-achat-detail/commande-achat-detail.component';

const routes: Routes = [
  {
    path: '',
    component: CommandeAchatListComponent,
    canActivate: [authGuard],
    title: "Commandes d'Achat - GestCom"
  },
  {
    path: 'new',
    component: CommandeAchatFormComponent,
    canActivate: [authGuard],
    title: "Nouvelle Commande d'Achat - GestCom",
    data: { mode: 'create' }
  },
  {
    path: ':code',
    component: CommandeAchatDetailComponent,
    canActivate: [authGuard],
    title: "Détails Commande d'Achat - GestCom"
  },
  {
    path: ':code/edit',
    component: CommandeAchatFormComponent,
    canActivate: [authGuard],
    title: "Modifier Commande d'Achat - GestCom",
    data: { mode: 'edit' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CommandesAchatRoutingModule { }
