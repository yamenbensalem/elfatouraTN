import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { SharedModule } from '../../shared/shared.module';
import { FacturesClientRoutingModule } from './factures-client-routing.module';
import { facturesClientFeature } from '../../store/factures-client/factures-client.reducer';
import { FacturesClientEffects } from '../../store/factures-client/factures-client.effects';
import { FacturesClientService } from '../../core/services/factures-client.service';

// Components
import { FactureClientListComponent } from './pages/facture-client-list/facture-client-list.component';
import { FactureClientFormComponent } from './pages/facture-client-form/facture-client-form.component';
import { FactureClientDetailComponent } from './pages/facture-client-detail/facture-client-detail.component';

@NgModule({
  declarations: [
    FactureClientListComponent,
    FactureClientFormComponent,
    FactureClientDetailComponent
  ],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    FacturesClientRoutingModule,
    StoreModule.forFeature(facturesClientFeature),
    EffectsModule.forFeature([FacturesClientEffects])
  ],
  providers: [
    FacturesClientService
  ]
})
export class FacturesClientModule { }
