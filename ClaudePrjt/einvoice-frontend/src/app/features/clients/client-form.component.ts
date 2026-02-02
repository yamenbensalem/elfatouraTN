import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ClientService } from '../../core/services/client.service';
import { Client } from '../../core/models/invoice.model';

@Component({
  selector: 'app-client-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container mx-auto p-4 max-w-3xl">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold text-gray-800">
          {{ isEdit() ? 'Modifier Client' : 'Nouveau Client' }}
        </h1>
        <a routerLink="/clients" class="text-gray-600 hover:text-gray-800">
          ← Retour à la liste
        </a>
      </div>

      <form (ngSubmit)="onSubmit()" class="space-y-6">
        <!-- Identification -->
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-semibold mb-4 text-gray-700">Identification</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Matricule Fiscal *</label>
              <input type="text" [(ngModel)]="client.matriculeFiscal" name="matriculeFiscal"
                     class="w-full border rounded-lg px-3 py-2" required
                     placeholder="1234567/A/B/M/000"
                     [disabled]="isEdit()">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Raison Sociale *</label>
              <input type="text" [(ngModel)]="client.name" name="name"
                     class="w-full border rounded-lg px-3 py-2" required>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Forme Juridique</label>
              <select [(ngModel)]="client.legalForm" name="legalForm"
                      class="w-full border rounded-lg px-3 py-2">
                <option value="">-- Sélectionner --</option>
                <option value="SARL">SARL</option>
                <option value="SA">SA</option>
                <option value="SUARL">SUARL</option>
                <option value="SNC">SNC</option>
                <option value="SCS">SCS</option>
                <option value="PP">Personne Physique</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">N° Registre Commerce</label>
              <input type="text" [(ngModel)]="client.registrationNumber" name="registrationNumber"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Capital (TND)</label>
              <input type="number" [(ngModel)]="client.capital" name="capital"
                     class="w-full border rounded-lg px-3 py-2" step="0.001">
            </div>
          </div>
        </div>

        <!-- Adresse -->
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-semibold mb-4 text-gray-700">Adresse</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="md:col-span-2">
              <label class="block text-sm font-medium text-gray-600 mb-1">Adresse</label>
              <input type="text" [(ngModel)]="client.street" name="street"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Ville</label>
              <input type="text" [(ngModel)]="client.city" name="city"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Code Postal</label>
              <input type="text" [(ngModel)]="client.postalCode" name="postalCode"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
          </div>
        </div>

        <!-- Contact -->
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-semibold mb-4 text-gray-700">Contact</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Téléphone</label>
              <input type="tel" [(ngModel)]="client.phone" name="phone"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Fax</label>
              <input type="tel" [(ngModel)]="client.fax" name="fax"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Email</label>
              <input type="email" [(ngModel)]="client.email" name="email"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Site Web</label>
              <input type="url" [(ngModel)]="client.website" name="website"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
          </div>
        </div>

        <!-- Banque -->
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-semibold mb-4 text-gray-700">Coordonnées Bancaires</h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Code Banque</label>
              <input type="text" [(ngModel)]="client.bankCode" name="bankCode"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">RIB</label>
              <input type="text" [(ngModel)]="client.bankAccountNumber" name="bankAccountNumber"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Nom Banque</label>
              <input type="text" [(ngModel)]="client.bankName" name="bankName"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
          </div>
        </div>

        <!-- TTN Configuration -->
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-semibold mb-4 text-gray-700">Configuration El Fatoora (TTN)</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Mode Compte</label>
              <select [(ngModel)]="client.ttnAccountMode" name="ttnAccountMode"
                      class="w-full border rounded-lg px-3 py-2">
                <option value="TEST">TEST</option>
                <option value="PROD">PRODUCTION</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Rang Compte</label>
              <input type="text" [(ngModel)]="client.ttnAccountRank" name="ttnAccountRank"
                     class="w-full border rounded-lg px-3 py-2" placeholder="1">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Profil</label>
              <input type="text" [(ngModel)]="client.ttnProfile" name="ttnProfile"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Code Client TTN</label>
              <input type="text" [(ngModel)]="client.ttnClientCode" name="ttnClientCode"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
          </div>
        </div>

        <!-- Status -->
        <div class="bg-white rounded-lg shadow p-6">
          <label class="flex items-center">
            <input type="checkbox" [(ngModel)]="client.isActive" name="isActive"
                   class="w-4 h-4 mr-2">
            <span class="font-medium">Client actif</span>
          </label>
        </div>

        <!-- Actions -->
        <div class="flex justify-end gap-4">
          <a routerLink="/clients" class="px-6 py-2 border rounded-lg hover:bg-gray-100">
            Annuler
          </a>
          <button type="submit" 
                  class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                  [disabled]="submitting()">
            {{ submitting() ? 'Enregistrement...' : 'Enregistrer' }}
          </button>
        </div>
      </form>
    </div>
  `
})
export class ClientFormComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private clientService = inject(ClientService);

  isEdit = signal(false);
  submitting = signal(false);

  client: Partial<Client> = {
    matriculeFiscal: '',
    name: '',
    countryCode: 'TN',
    ttnAccountMode: 'TEST',
    ttnAccountRank: '1',
    isActive: true
  };

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEdit.set(true);
      this.loadClient(id);
    }
  }

  loadClient(id: string) {
    this.clientService.getClient(id).subscribe({
      next: (client) => {
        this.client = { ...client };
      },
      error: () => {
        alert('Client non trouvé');
        this.router.navigate(['/clients']);
      }
    });
  }

  onSubmit() {
    this.submitting.set(true);

    const operation = this.isEdit()
      ? this.clientService.updateClient(this.client.id!, this.client)
      : this.clientService.createClient(this.client as Omit<Client, 'id' | 'createdAt'>);

    operation.subscribe({
      next: () => {
        alert(this.isEdit() ? 'Client mis à jour!' : 'Client créé!');
        this.router.navigate(['/clients']);
      },
      error: (err) => {
        this.submitting.set(false);
        alert('Erreur: ' + err.message);
      }
    });
  }
}
