import { NgModule } from '@angular/core';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { SharedModule } from '../../shared/shared.module';
import { FacturesFournisseurRoutingModule } from './factures-fournisseur-routing.module';
import { facturesFournisseurReducer } from '../../store/factures-fournisseur/factures-fournisseur.reducer';
import { FacturesFournisseurEffects } from '../../store/factures-fournisseur/factures-fournisseur.effects';

import { FactureFournisseurListComponent } from './pages/facture-fournisseur-list/facture-fournisseur-list.component';
import { FactureFournisseurFormComponent } from './pages/facture-fournisseur-form/facture-fournisseur-form.component';
import { FactureFournisseurDetailComponent } from './pages/facture-fournisseur-detail/facture-fournisseur-detail.component';

@NgModule({
  declarations: [
    FactureFournisseurListComponent,
    FactureFournisseurFormComponent,
    FactureFournisseurDetailComponent
  ],
  imports: [
    SharedModule,
    FacturesFournisseurRoutingModule,
    StoreModule.forFeature('facturesFournisseur', facturesFournisseurReducer),
    EffectsModule.forFeature([FacturesFournisseurEffects])
  ]
})
export class FacturesFournisseurModule {}
