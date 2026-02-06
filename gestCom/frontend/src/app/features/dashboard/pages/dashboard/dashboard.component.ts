import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';

import { AppState } from '../../../../store/app.state';
import { selectUserFullName } from '../../../../store/auth/auth.selectors';

export interface StatCard {
  title: string;
  value: string | number;
  icon: string;
  color: string;
  trend?: {
    value: number;
    isPositive: boolean;
  };
}

export interface QuickAction {
  label: string;
  icon: string;
  route: string;
  color: string;
}

export interface RecentActivity {
  id: number;
  type: 'facture' | 'commande' | 'client' | 'produit';
  description: string;
  date: Date;
  icon: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  userFullName$: Observable<string | null>;
  
  isLoading = true;

  statCards: StatCard[] = [
    {
      title: 'Total Clients',
      value: 0,
      icon: 'people',
      color: '#1976d2',
      trend: { value: 12, isPositive: true }
    },
    {
      title: 'Total Produits',
      value: 0,
      icon: 'inventory_2',
      color: '#7b1fa2',
      trend: { value: 5, isPositive: true }
    },
    {
      title: 'CA Mensuel',
      value: '0 TND',
      icon: 'attach_money',
      color: '#388e3c',
      trend: { value: 8, isPositive: true }
    },
    {
      title: 'Factures en attente',
      value: 0,
      icon: 'pending_actions',
      color: '#f57c00',
      trend: { value: 3, isPositive: false }
    }
  ];

  quickActions: QuickAction[] = [
    {
      label: 'Nouveau Client',
      icon: 'person_add',
      route: '/clients/nouveau',
      color: '#1976d2'
    },
    {
      label: 'Nouvelle Facture',
      icon: 'receipt_long',
      route: '/factures-client/nouvelle',
      color: '#388e3c'
    },
    {
      label: 'Nouveau Devis',
      icon: 'request_quote',
      route: '/devis/nouveau',
      color: '#7b1fa2'
    },
    {
      label: 'Nouveau Produit',
      icon: 'add_box',
      route: '/produits/nouveau',
      color: '#f57c00'
    }
  ];

  recentActivities: RecentActivity[] = [];

  constructor(private store: Store<AppState>) {
    this.userFullName$ = this.store.select(selectUserFullName);
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    
    // Simulate API call - replace with actual API call later
    setTimeout(() => {
      this.statCards = [
        {
          title: 'Total Clients',
          value: 156,
          icon: 'people',
          color: '#1976d2',
          trend: { value: 12, isPositive: true }
        },
        {
          title: 'Total Produits',
          value: 423,
          icon: 'inventory_2',
          color: '#7b1fa2',
          trend: { value: 5, isPositive: true }
        },
        {
          title: 'CA Mensuel',
          value: '45,680 TND',
          icon: 'attach_money',
          color: '#388e3c',
          trend: { value: 8, isPositive: true }
        },
        {
          title: 'Factures en attente',
          value: 23,
          icon: 'pending_actions',
          color: '#f57c00',
          trend: { value: 3, isPositive: false }
        }
      ];

      this.recentActivities = [
        {
          id: 1,
          type: 'facture',
          description: 'Facture FC-2026-0045 créée pour Client ABC',
          date: new Date(),
          icon: 'receipt_long'
        },
        {
          id: 2,
          type: 'client',
          description: 'Nouveau client "Société XYZ" ajouté',
          date: new Date(Date.now() - 3600000),
          icon: 'person_add'
        },
        {
          id: 3,
          type: 'commande',
          description: 'Commande CMD-2026-0123 validée',
          date: new Date(Date.now() - 7200000),
          icon: 'shopping_cart'
        },
        {
          id: 4,
          type: 'produit',
          description: 'Stock du produit "PRD-001" mis à jour',
          date: new Date(Date.now() - 10800000),
          icon: 'inventory'
        },
        {
          id: 5,
          type: 'facture',
          description: 'Paiement reçu pour Facture FC-2026-0042',
          date: new Date(Date.now() - 14400000),
          icon: 'payments'
        }
      ];

      this.isLoading = false;
    }, 1000);
  }

  getTimeAgo(date: Date): string {
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 1) {
      return 'À l\'instant';
    } else if (minutes < 60) {
      return `Il y a ${minutes} min`;
    } else if (hours < 24) {
      return `Il y a ${hours} h`;
    } else {
      return `Il y a ${days} jour(s)`;
    }
  }
}
