import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ClientService } from '../../core/services/client.service';
import { Client } from '../../core/models/invoice.model';

@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container mx-auto p-4">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold text-gray-800">Clients</h1>
        <a routerLink="/clients/new" 
           class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition">
          + Nouveau Client
        </a>
      </div>

      <!-- Search -->
      <div class="bg-white rounded-lg shadow p-4 mb-6">
        <input type="text" placeholder="Rechercher par nom ou matricule fiscal..."
               (input)="onSearch($event)"
               class="w-full border rounded-lg px-4 py-2">
      </div>

      <!-- Table -->
      <div class="bg-white rounded-lg shadow overflow-hidden">
        @if (loading()) {
          <div class="p-8 text-center text-gray-500">Chargement...</div>
        } @else if (clients().length === 0) {
          <div class="p-8 text-center text-gray-500">Aucun client trouvé</div>
        } @else {
          <table class="w-full">
            <thead class="bg-gray-100">
              <tr>
                <th class="px-4 py-3 text-left text-sm font-medium text-gray-600">Matricule Fiscal</th>
                <th class="px-4 py-3 text-left text-sm font-medium text-gray-600">Raison Sociale</th>
                <th class="px-4 py-3 text-left text-sm font-medium text-gray-600">Ville</th>
                <th class="px-4 py-3 text-left text-sm font-medium text-gray-600">Téléphone</th>
                <th class="px-4 py-3 text-center text-sm font-medium text-gray-600">Statut</th>
                <th class="px-4 py-3 text-center text-sm font-medium text-gray-600">Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (client of clients(); track client.id) {
                <tr class="border-t hover:bg-gray-50">
                  <td class="px-4 py-3 font-mono text-sm">{{ client.matriculeFiscal }}</td>
                  <td class="px-4 py-3">
                    <a [routerLink]="['/clients', client.id]" class="text-blue-600 hover:underline">
                      {{ client.name }}
                    </a>
                    @if (client.legalForm) {
                      <span class="text-gray-500 text-sm ml-1">({{ client.legalForm }})</span>
                    }
                  </td>
                  <td class="px-4 py-3 text-gray-600">{{ client.city || '-' }}</td>
                  <td class="px-4 py-3 text-gray-600">{{ client.phone || '-' }}</td>
                  <td class="px-4 py-3 text-center">
                    <span [class]="client.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                          class="px-2 py-1 rounded-full text-xs font-medium">
                      {{ client.isActive ? 'Actif' : 'Inactif' }}
                    </span>
                  </td>
                  <td class="px-4 py-3 text-center">
                    <a [routerLink]="['/clients', client.id]" 
                       class="text-blue-600 hover:text-blue-800 mr-3">✏️</a>
                    <button (click)="deleteClient(client)" 
                            class="text-red-600 hover:text-red-800">🗑️</button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        }
      </div>
    </div>
  `
})
export class ClientListComponent implements OnInit {
  private clientService = inject(ClientService);

  clients = signal<Client[]>([]);
  loading = signal(true);
  private searchTimeout?: ReturnType<typeof setTimeout>;

  ngOnInit() {
    this.loadClients();
  }

  loadClients() {
    this.loading.set(true);
    this.clientService.getClients().subscribe({
      next: (clients) => {
        this.clients.set(clients);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSearch(event: Event) {
    const term = (event.target as HTMLInputElement).value;
    
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      if (term.length >= 2) {
        this.clientService.searchClients(term).subscribe(clients => {
          this.clients.set(clients);
        });
      } else if (term.length === 0) {
        this.loadClients();
      }
    }, 300);
  }

  deleteClient(client: Client) {
    if (confirm(`Voulez-vous supprimer le client "${client.name}"?`)) {
      this.clientService.deleteClient(client.id).subscribe({
        next: () => this.loadClients(),
        error: () => alert('Erreur lors de la suppression')
      });
    }
  }
}
