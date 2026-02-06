import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { SharedModule } from '../../shared/shared.module';
import { CommandesAchatRoutingModule } from './commandes-achat-routing.module';
import { commandesAchatFeature } from '../../store/commandes-achat/commandes-achat.reducer';
import { CommandesAchatEffects } from '../../store/commandes-achat/commandes-achat.effects';
import { CommandesAchatService } from '../../core/services/commandes-achat.service';

// Components
import { CommandeAchatListComponent } from './pages/commande-achat-list/commande-achat-list.component';
import { CommandeAchatFormComponent } from './pages/commande-achat-form/commande-achat-form.component';
import { CommandeAchatDetailComponent } from './pages/commande-achat-detail/commande-achat-detail.component';

@NgModule({
  declarations: [
    CommandeAchatListComponent,
    CommandeAchatFormComponent,
    CommandeAchatDetailComponent
  ],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    CommandesAchatRoutingModule,
    StoreModule.forFeature(commandesAchatFeature),
    EffectsModule.forFeature([CommandesAchatEffects])
  ],
  providers: [
    CommandesAchatService
  ]
})
export class CommandesAchatModule { }
