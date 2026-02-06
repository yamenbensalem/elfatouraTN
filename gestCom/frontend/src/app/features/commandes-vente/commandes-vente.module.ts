import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { SharedModule } from '../../shared/shared.module';
import { CommandesVenteRoutingModule } from './commandes-vente-routing.module';
import { commandesVenteFeature } from '../../store/commandes-vente/commandes-vente.reducer';
import { CommandesVenteEffects } from '../../store/commandes-vente/commandes-vente.effects';
import { CommandesVenteService } from '../../core/services/commandes-vente.service';

// Components
import { CommandeVenteListComponent } from './pages/commande-vente-list/commande-vente-list.component';
import { CommandeVenteFormComponent } from './pages/commande-vente-form/commande-vente-form.component';
import { CommandeVenteDetailComponent } from './pages/commande-vente-detail/commande-vente-detail.component';

@NgModule({
  declarations: [
    CommandeVenteListComponent,
    CommandeVenteFormComponent,
    CommandeVenteDetailComponent
  ],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    CommandesVenteRoutingModule,
    StoreModule.forFeature(commandesVenteFeature),
    EffectsModule.forFeature([CommandesVenteEffects])
  ],
  providers: [
    CommandesVenteService
  ]
})
export class CommandesVenteModule { }
