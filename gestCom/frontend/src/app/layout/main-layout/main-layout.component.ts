import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';

import { AppState } from '../../store/app.state';
import { AuthService } from '../../core/services/auth.service';
import { AuthActions } from '../../store/auth/auth.actions';
import { selectUser, selectUserFullName } from '../../store/auth/auth.selectors';
import { User } from '../../core/models/auth.models';
import { NavItem } from '../sidenav/sidenav.component';

@Component({
  selector: 'app-main-layout',
  standalone: false,
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent implements OnInit {
  @ViewChild('sidenav') sidenav!: MatSidenav;

  isHandset$: Observable<boolean>;
  user$: Observable<User | null>;
  userFullName$: Observable<string | null>;

  navItems: NavItem[] = [
    {
      label: 'Tableau de bord',
      icon: 'dashboard',
      route: '/dashboard'
    },
    {
      label: 'Ventes',
      icon: 'point_of_sale',
      expanded: false,
      children: [
        { label: 'Clients', icon: 'people', route: '/clients' },
        { label: 'Devis', icon: 'request_quote', route: '/devis' },
        { label: 'Commandes', icon: 'shopping_cart', route: '/commandes-vente' },
        { label: 'Factures', icon: 'receipt_long', route: '/factures-client' }
      ]
    },
    {
      label: 'Achats',
      icon: 'shopping_bag',
      expanded: false,
      children: [
        { label: 'Fournisseurs', icon: 'business', route: '/fournisseurs' },
        { label: 'Commandes Achat', icon: 'add_shopping_cart', route: '/commandes-achat' },
        { label: 'Factures Fournisseur', icon: 'receipt', route: '/factures-fournisseur' }
      ]
    },
    {
      label: 'Stock',
      icon: 'inventory_2',
      expanded: false,
      children: [
        { label: 'Produits', icon: 'inventory', route: '/produits' },
        { label: 'Catégories', icon: 'category', route: '/categories' }
      ]
    },
    {
      label: 'Configuration',
      icon: 'settings',
      expanded: false,
      children: [
        { label: 'Entreprise', icon: 'business_center', route: '/entreprise' },
        { label: 'Paramètres', icon: 'tune', route: '/parametres' }
      ]
    }
  ];

  constructor(
    private breakpointObserver: BreakpointObserver,
    private store: Store<AppState>,
    private authService: AuthService
  ) {
    this.isHandset$ = this.breakpointObserver.observe(Breakpoints.Handset)
      .pipe(
        map(result => result.matches),
        shareReplay()
      );

    this.user$ = this.store.select(selectUser);
    this.userFullName$ = this.store.select(selectUserFullName);
  }

  ngOnInit(): void {
    // Initialization logic if needed
  }

  toggleSidenav(): void {
    this.sidenav.toggle();
  }

  toggleNavItem(item: NavItem): void {
    if (item.children) {
      item.expanded = !item.expanded;
    }
  }

  onLogout(): void {
    this.store.dispatch(AuthActions.logout());
  }

  closeSidenavOnMobile(): void {
    this.isHandset$.subscribe(isHandset => {
      if (isHandset && this.sidenav) {
        this.sidenav.close();
      }
    }).unsubscribe();
  }
}
