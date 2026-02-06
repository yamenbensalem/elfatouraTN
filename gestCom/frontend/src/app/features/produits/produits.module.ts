import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { SharedModule } from '../../shared/shared.module';
import { ProduitsRoutingModule } from './produits-routing.module';
import { produitsFeature } from '../../store/produits/produits.reducer';
import { ProduitsEffects } from '../../store/produits/produits.effects';
import { ProduitsService } from '../../core/services/produits.service';

// Components
import { ProduitListComponent } from './pages/produit-list/produit-list.component';
import { ProduitFormComponent } from './pages/produit-form/produit-form.component';
import { ProduitDetailComponent } from './pages/produit-detail/produit-detail.component';

@NgModule({
  declarations: [
    ProduitListComponent,
    ProduitFormComponent,
    ProduitDetailComponent
  ],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    ProduitsRoutingModule,
    StoreModule.forFeature(produitsFeature),
    EffectsModule.forFeature([ProduitsEffects])
  ],
  providers: [
    ProduitsService
  ]
})
export class ProduitsModule { }
