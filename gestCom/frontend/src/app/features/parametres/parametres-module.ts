import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { ParametresPage } from './parametres-page/parametres-page';
import { SharedModule } from '../../shared/shared.module';

const routes: Routes = [
  { path: '', component: ParametresPage }
];

@NgModule({
  declarations: [
    ParametresPage
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class ParametresModule { }
