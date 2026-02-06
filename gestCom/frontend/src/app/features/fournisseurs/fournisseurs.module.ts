import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { SharedModule } from '../../shared/shared.module';
import { FournisseursRoutingModule } from './fournisseurs-routing.module';
import { fournisseursFeature } from '../../store/fournisseurs/fournisseurs.reducer';
import { FournisseursEffects } from '../../store/fournisseurs/fournisseurs.effects';
import { FournisseursService } from '../../core/services/fournisseurs.service';

// Components
import { FournisseurListComponent } from './pages/fournisseur-list/fournisseur-list.component';
import { FournisseurFormComponent } from './pages/fournisseur-form/fournisseur-form.component';
import { FournisseurDetailComponent } from './pages/fournisseur-detail/fournisseur-detail.component';

@NgModule({
  declarations: [
    FournisseurListComponent,
    FournisseurFormComponent,
    FournisseurDetailComponent
  ],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    FournisseursRoutingModule,
    StoreModule.forFeature(fournisseursFeature),
    EffectsModule.forFeature([FournisseursEffects])
  ],
  providers: [
    FournisseursService
  ]
})
export class FournisseursModule { }
