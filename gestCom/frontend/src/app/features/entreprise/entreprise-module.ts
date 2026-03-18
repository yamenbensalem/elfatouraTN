import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { EntreprisePage } from './entreprise-page/entreprise-page';
import { SharedModule } from '../../shared/shared.module';

const routes: Routes = [
  { path: '', component: EntreprisePage }
];

@NgModule({
  declarations: [
    EntreprisePage
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class EntrepriseModule { }
