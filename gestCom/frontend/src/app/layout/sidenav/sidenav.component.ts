import { Component, EventEmitter, Output, Input } from '@angular/core';

export interface NavItem {
  label: string;
  icon: string;
  route?: string;
  children?: NavItem[];
  expanded?: boolean;
}

@Component({
  selector: 'app-sidenav',
  standalone: false,
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss']
})
export class SidenavComponent {
  @Output() navItemClicked = new EventEmitter<void>();
  @Input() isHandset = false;

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

  toggleNavItem(item: NavItem): void {
    if (item.children) {
      item.expanded = !item.expanded;
    }
  }

  onNavItemClick(): void {
    if (this.isHandset) {
      this.navItemClicked.emit();
    }
  }
}
