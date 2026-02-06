import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { SharedModule } from '../../shared/shared.module';
import { ClientsRoutingModule } from './clients-routing.module';
import { clientsFeature } from '../../store/clients/clients.reducer';
import { ClientsEffects } from '../../store/clients/clients.effects';
import { ClientsService } from '../../core/services/clients.service';

// Components
import { ClientListComponent } from './pages/client-list/client-list.component';
import { ClientFormComponent } from './pages/client-form/client-form.component';
import { ClientDetailComponent } from './pages/client-detail/client-detail.component';

@NgModule({
  declarations: [
    ClientListComponent,
    ClientFormComponent,
    ClientDetailComponent
  ],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    ClientsRoutingModule,
    StoreModule.forFeature(clientsFeature),
    EffectsModule.forFeature([ClientsEffects])
  ],
  providers: [
    ClientsService
  ]
})
export class ClientsModule { }
