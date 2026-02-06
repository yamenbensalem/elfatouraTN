import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';

import { MainLayoutComponent } from './main-layout/main-layout.component';
import { HeaderComponent } from './header/header.component';
import { SidenavComponent } from './sidenav/sidenav.component';

@NgModule({
  declarations: [
    MainLayoutComponent,
    HeaderComponent,
    SidenavComponent
  ],
  imports: [
    SharedModule,
    RouterModule
  ],
  exports: [
    MainLayoutComponent
  ]
})
export class LayoutModule { }
