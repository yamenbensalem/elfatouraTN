import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { authGuard } from '../../core/guards/auth.guard';
import { DevisListComponent } from './pages/devis-list/devis-list.component';
import { DevisFormComponent } from './pages/devis-form/devis-form.component';
import { DevisDetailComponent } from './pages/devis-detail/devis-detail.component';

const routes: Routes = [
  {
    path: '',
    component: DevisListComponent,
    canActivate: [authGuard],
    title: 'Devis - GestCom'
  },
  {
    path: 'new',
    component: DevisFormComponent,
    canActivate: [authGuard],
    title: 'Nouveau Devis - GestCom',
    data: { mode: 'create' }
  },
  {
    path: ':code',
    component: DevisDetailComponent,
    canActivate: [authGuard],
    title: 'Détails Devis - GestCom'
  },
  {
    path: ':code/edit',
    component: DevisFormComponent,
    canActivate: [authGuard],
    title: 'Modifier Devis - GestCom',
    data: { mode: 'edit' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DevisRoutingModule { }
