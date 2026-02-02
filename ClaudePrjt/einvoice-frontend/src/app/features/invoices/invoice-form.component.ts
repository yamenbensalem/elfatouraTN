import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { InvoiceService } from '../../core/services/invoice.service';
import { ClientService } from '../../core/services/client.service';
import { InvoiceRequest, LineItem, Client, PartnerInfo } from '../../core/models/invoice.model';

@Component({
  selector: 'app-invoice-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container mx-auto p-4 max-w-4xl">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold text-gray-800">Nouvelle Facture</h1>
        <a routerLink="/invoices" class="text-gray-600 hover:text-gray-800">
          ← Retour à la liste
        </a>
      </div>

      <form (ngSubmit)="onSubmit()" class="space-y-6">
        <!-- Invoice Header -->
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-semibold mb-4 text-gray-700">Informations Facture</h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">N° Facture *</label>
              <input type="text" [(ngModel)]="invoice.documentIdentifier" name="documentIdentifier"
                     class="w-full border rounded-lg px-3 py-2" required>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Type *</label>
              <select [(ngModel)]="invoice.documentType" name="documentType"
                      class="w-full border rounded-lg px-3 py-2" required>
                <option value="I-11">Facture</option>
                <option value="I-12">Facture rectificative</option>
                <option value="I-13">Note de crédit</option>
                <option value="I-14">Note de débit</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Date *</label>
              <input type="date" [(ngModel)]="invoiceDate" name="invoiceDate"
                     class="w-full border rounded-lg px-3 py-2" required>
            </div>
          </div>
        </div>

        <!-- Sender -->
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-semibold mb-4 text-gray-700">Émetteur (Votre Client)</h2>
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-600 mb-1">Sélectionner un client</label>
            <select (change)="onSenderSelect($event)" class="w-full border rounded-lg px-3 py-2">
              <option value="">-- Sélectionner --</option>
              @for (client of clients(); track client.id) {
                <option [value]="client.id">{{ client.name }} ({{ client.matriculeFiscal }})</option>
              }
            </select>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Matricule Fiscal *</label>
              <input type="text" [(ngModel)]="invoice.sender.identifier" name="senderMf"
                     class="w-full border rounded-lg px-3 py-2" required placeholder="1234567/A/B/M/000">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Raison Sociale *</label>
              <input type="text" [(ngModel)]="invoice.sender.name" name="senderName"
                     class="w-full border rounded-lg px-3 py-2" required>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Adresse</label>
              <input type="text" [(ngModel)]="invoice.sender.address.street" name="senderStreet"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Ville</label>
              <input type="text" [(ngModel)]="invoice.sender.address.city" name="senderCity"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
          </div>
        </div>

        <!-- Receiver -->
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-semibold mb-4 text-gray-700">Destinataire</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Matricule Fiscal *</label>
              <input type="text" [(ngModel)]="invoice.receiver.identifier" name="receiverMf"
                     class="w-full border rounded-lg px-3 py-2" required placeholder="1234567/A/B/M/000">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Raison Sociale *</label>
              <input type="text" [(ngModel)]="invoice.receiver.name" name="receiverName"
                     class="w-full border rounded-lg px-3 py-2" required>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Adresse</label>
              <input type="text" [(ngModel)]="invoice.receiver.address.street" name="receiverStreet"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-600 mb-1">Ville</label>
              <input type="text" [(ngModel)]="invoice.receiver.address.city" name="receiverCity"
                     class="w-full border rounded-lg px-3 py-2">
            </div>
          </div>
        </div>

        <!-- Line Items -->
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-lg font-semibold text-gray-700">Lignes de Facture</h2>
            <button type="button" (click)="addLineItem()"
                    class="bg-green-600 text-white px-3 py-1 rounded hover:bg-green-700">
              + Ajouter ligne
            </button>
          </div>
          
          @for (item of invoice.lineItems; track $index) {
            <div class="border rounded-lg p-4 mb-4">
              <div class="flex justify-between mb-2">
                <span class="font-medium">Ligne {{ $index + 1 }}</span>
                <button type="button" (click)="removeLineItem($index)"
                        class="text-red-600 hover:text-red-800">✕</button>
              </div>
              <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div>
                  <label class="block text-sm text-gray-600 mb-1">Code Article</label>
                  <input type="text" [(ngModel)]="item.itemCode" [name]="'itemCode_' + $index"
                         class="w-full border rounded px-3 py-2" required>
                </div>
                <div class="md:col-span-2">
                  <label class="block text-sm text-gray-600 mb-1">Description</label>
                  <input type="text" [(ngModel)]="item.itemDescription" [name]="'itemDesc_' + $index"
                         class="w-full border rounded px-3 py-2" required>
                </div>
                <div>
                  <label class="block text-sm text-gray-600 mb-1">Quantité</label>
                  <input type="number" [(ngModel)]="item.quantity" [name]="'qty_' + $index"
                         (ngModelChange)="calculateLineTotal(item)"
                         class="w-full border rounded px-3 py-2" step="0.001" required>
                </div>
                <div>
                  <label class="block text-sm text-gray-600 mb-1">Prix Unitaire HT</label>
                  <input type="number" [(ngModel)]="item.unitPriceExcludingTax" [name]="'price_' + $index"
                         (ngModelChange)="calculateLineTotal(item)"
                         class="w-full border rounded px-3 py-2" step="0.001" required>
                </div>
                <div>
                  <label class="block text-sm text-gray-600 mb-1">TVA %</label>
                  <select [(ngModel)]="item.taxRate" [name]="'tax_' + $index"
                          (ngModelChange)="calculateLineTotal(item)"
                          class="w-full border rounded px-3 py-2">
                    <option [value]="0">0%</option>
                    <option [value]="7">7%</option>
                    <option [value]="13">13%</option>
                    <option [value]="19">19%</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm text-gray-600 mb-1">Total HT</label>
                  <input type="text" [value]="item.totalExcludingTax | number:'1.3-3'"
                         class="w-full border rounded px-3 py-2 bg-gray-100" readonly>
                </div>
              </div>
            </div>
          }
        </div>

        <!-- Totals -->
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-semibold mb-4 text-gray-700">Totaux</h2>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div>
              <label class="block text-sm text-gray-600 mb-1">Total HT</label>
              <div class="font-semibold">{{ totalHT() | number:'1.3-3' }} TND</div>
            </div>
            <div>
              <label class="block text-sm text-gray-600 mb-1">Total TVA</label>
              <div class="font-semibold">{{ totalTVA() | number:'1.3-3' }} TND</div>
            </div>
            <div>
              <label class="block text-sm text-gray-600 mb-1">Droit de Timbre</label>
              <input type="number" [(ngModel)]="invoice.stampDuty" name="stampDuty"
                     class="w-full border rounded px-3 py-2" step="0.001">
            </div>
            <div>
              <label class="block text-sm text-gray-600 mb-1">Total TTC</label>
              <div class="text-xl font-bold text-blue-600">{{ totalTTC() | number:'1.3-3' }} TND</div>
            </div>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex justify-end gap-4">
          <button type="button" routerLink="/invoices"
                  class="px-6 py-2 border rounded-lg hover:bg-gray-100">
            Annuler
          </button>
          <button type="button" (click)="onValidate()"
                  class="px-6 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700">
            Valider
          </button>
          <button type="submit"
                  class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                  [disabled]="submitting()">
            {{ submitting() ? 'Génération...' : 'Générer Facture' }}
          </button>
        </div>
      </form>
    </div>
  `
})
export class InvoiceFormComponent implements OnInit {
  private invoiceService = inject(InvoiceService);
  private clientService = inject(ClientService);
  private router = inject(Router);

  clients = signal<Client[]>([]);
  submitting = signal(false);
  invoiceDate = new Date().toISOString().split('T')[0];

  invoice: InvoiceRequest = {
    documentIdentifier: '',
    documentType: 'I-11',
    invoiceDate: new Date(),
    sender: this.createEmptyPartner(),
    receiver: this.createEmptyPartner(),
    lineItems: [this.createEmptyLineItem()],
    stampDuty: 1.000
  };

  ngOnInit() {
    this.loadClients();
  }

  loadClients() {
    this.clientService.getClients().subscribe(clients => {
      this.clients.set(clients);
    });
  }

  createEmptyPartner(): PartnerInfo {
    return {
      identifierType: 'MF',
      identifier: '',
      name: '',
      address: {
        description: '',
        street: '',
        city: '',
        postalCode: '',
        country: 'TN',
        language: 'fr'
      }
    };
  }

  createEmptyLineItem(): LineItem {
    return {
      itemIdentifier: '',
      itemCode: '',
      itemDescription: '',
      language: 'fr',
      quantity: 1,
      measurementUnit: 'UNIT',
      unitPriceExcludingTax: 0,
      totalExcludingTax: 0,
      taxTypeCode: 'V',
      taxTypeName: 'TVA',
      taxRate: 19,
      taxAmount: 0
    };
  }

  onSenderSelect(event: Event) {
    const clientId = (event.target as HTMLSelectElement).value;
    if (clientId) {
      const client = this.clients().find(c => c.id === clientId);
      if (client) {
        this.invoice.sender = {
          identifierType: 'MF',
          identifier: client.matriculeFiscal,
          name: client.name,
          legalForm: client.legalForm,
          registrationNumber: client.registrationNumber,
          capital: client.capital,
          accountMode: client.ttnAccountMode,
          accountRank: client.ttnAccountRank,
          profile: client.ttnProfile,
          clientCode: client.ttnClientCode,
          address: {
            description: client.addressDescription || '',
            street: client.street || '',
            city: client.city || '',
            postalCode: client.postalCode || '',
            country: client.countryCode,
            language: 'fr'
          },
          contacts: client.phone ? [{ type: 'TE', value: client.phone }] : [],
          bankAccounts: client.bankAccountNumber ? [{
            accountNumber: client.bankAccountNumber,
            bankCode: client.bankCode || '',
            ownerIdentifier: client.matriculeFiscal,
            branchIdentifier: '',
            institutionName: client.bankName || '',
            functionCode: 'BKA'
          }] : []
        };
      }
    }
  }

  addLineItem() {
    this.invoice.lineItems.push(this.createEmptyLineItem());
  }

  removeLineItem(index: number) {
    if (this.invoice.lineItems.length > 1) {
      this.invoice.lineItems.splice(index, 1);
    }
  }

  calculateLineTotal(item: LineItem) {
    item.totalExcludingTax = item.quantity * item.unitPriceExcludingTax;
    item.taxAmount = item.totalExcludingTax * (item.taxRate / 100);
  }

  totalHT(): number {
    return this.invoice.lineItems.reduce((sum, item) => sum + item.totalExcludingTax, 0);
  }

  totalTVA(): number {
    return this.invoice.lineItems.reduce((sum, item) => sum + item.taxAmount, 0);
  }

  totalTTC(): number {
    return this.totalHT() + this.totalTVA() + this.invoice.stampDuty;
  }

  onValidate() {
    this.invoice.invoiceDate = new Date(this.invoiceDate);
    this.invoiceService.validateInvoice(this.invoice).subscribe({
      next: (response) => {
        if (response.success) {
          alert('Facture valide!');
        } else {
          alert('Erreurs de validation:\n' + response.validationErrors?.map(e => e.message).join('\n'));
        }
      },
      error: (err) => alert('Erreur de validation: ' + err.message)
    });
  }

  onSubmit() {
    this.submitting.set(true);
    this.invoice.invoiceDate = new Date(this.invoiceDate);
    
    this.invoiceService.generateInvoice(this.invoice).subscribe({
      next: (response) => {
        this.submitting.set(false);
        if (response.success) {
          alert('Facture générée avec succès!');
          this.router.navigate(['/invoices']);
        } else {
          alert('Erreurs:\n' + response.validationErrors?.map(e => e.message).join('\n'));
        }
      },
      error: (err) => {
        this.submitting.set(false);
        alert('Erreur: ' + err.message);
      }
    });
  }
}
