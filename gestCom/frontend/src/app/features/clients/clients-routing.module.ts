import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { authGuard } from '../../core/guards/auth.guard';
import { ClientListComponent } from './pages/client-list/client-list.component';
import { ClientFormComponent } from './pages/client-form/client-form.component';
import { ClientDetailComponent } from './pages/client-detail/client-detail.component';

const routes: Routes = [
  {
    path: '',
    component: ClientListComponent,
    canActivate: [authGuard],
    title: 'Clients - GestCom'
  },
  {
    path: 'new',
    component: ClientFormComponent,
    canActivate: [authGuard],
    title: 'Nouveau Client - GestCom',
    data: { mode: 'create' }
  },
  {
    path: ':code',
    component: ClientDetailComponent,
    canActivate: [authGuard],
    title: 'Détails Client - GestCom'
  },
  {
    path: ':code/edit',
    component: ClientFormComponent,
    canActivate: [authGuard],
    title: 'Modifier Client - GestCom',
    data: { mode: 'edit' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ClientsRoutingModule { }
