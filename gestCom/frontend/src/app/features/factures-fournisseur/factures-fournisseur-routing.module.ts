import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FactureFournisseurListComponent } from './pages/facture-fournisseur-list/facture-fournisseur-list.component';
import { FactureFournisseurFormComponent } from './pages/facture-fournisseur-form/facture-fournisseur-form.component';
import { FactureFournisseurDetailComponent } from './pages/facture-fournisseur-detail/facture-fournisseur-detail.component';

const routes: Routes = [
  { path: '', component: FactureFournisseurListComponent },
  { path: 'new', component: FactureFournisseurFormComponent },
  { path: ':code', component: FactureFournisseurDetailComponent },
  { path: ':code/edit', component: FactureFournisseurFormComponent, data: { mode: 'edit' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FacturesFournisseurRoutingModule {}
