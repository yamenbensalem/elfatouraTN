import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { SharedModule } from '../../shared/shared.module';
import { DevisRoutingModule } from './devis-routing.module';
import { devisFeature } from '../../store/devis/devis.reducer';
import { DevisEffects } from '../../store/devis/devis.effects';
import { DevisService } from '../../core/services/devis.service';

// Components
import { DevisListComponent } from './pages/devis-list/devis-list.component';
import { DevisFormComponent } from './pages/devis-form/devis-form.component';
import { DevisDetailComponent } from './pages/devis-detail/devis-detail.component';

@NgModule({
  declarations: [
    DevisListComponent,
    DevisFormComponent,
    DevisDetailComponent
  ],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    DevisRoutingModule,
    StoreModule.forFeature(devisFeature),
    EffectsModule.forFeature([DevisEffects])
  ],
  providers: [
    DevisService
  ]
})
export class DevisModule { }
