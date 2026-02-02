import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { InvoiceService } from '../../core/services/invoice.service';
import { InvoiceRecord, InvoiceStatus, PagedResult } from '../../core/models/invoice.model';

@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container mx-auto p-4">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold text-gray-800">Factures Électroniques</h1>
        <a routerLink="/invoices/new" 
           class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition">
          + Nouvelle Facture
        </a>
      </div>

      <!-- Filters -->
      <div class="bg-white rounded-lg shadow p-4 mb-6">
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <select class="border rounded-lg px-3 py-2" (change)="onStatusChange($event)">
            <option value="">Tous les statuts</option>
            <option value="0">Brouillon</option>
            <option value="1">Générée</option>
            <option value="2">Signée</option>
            <option value="3">Envoyée</option>
            <option value="5">Validée</option>
            <option value="6">Rejetée</option>
          </select>
          <input type="date" class="border rounded-lg px-3 py-2" placeholder="Date début"
                 (change)="onDateFromChange($event)">
          <input type="date" class="border rounded-lg px-3 py-2" placeholder="Date fin"
                 (change)="onDateToChange($event)">
          <button class="bg-gray-200 px-4 py-2 rounded-lg hover:bg-gray-300 transition"
                  (click)="loadInvoices()">
            Rechercher
          </button>
        </div>
      </div>

      <!-- Table -->
      <div class="bg-white rounded-lg shadow overflow-hidden">
        @if (loading()) {
          <div class="p-8 text-center text-gray-500">
            Chargement...
          </div>
        } @else if (invoices().length === 0) {
          <div class="p-8 text-center text-gray-500">
            Aucune facture trouvée
          </div>
        } @else {
          <table class="w-full">
            <thead class="bg-gray-100">
              <tr>
                <th class="px-4 py-3 text-left text-sm font-medium text-gray-600">N° Facture</th>
                <th class="px-4 py-3 text-left text-sm font-medium text-gray-600">Date</th>
                <th class="px-4 py-3 text-left text-sm font-medium text-gray-600">Client</th>
                <th class="px-4 py-3 text-right text-sm font-medium text-gray-600">Total TTC</th>
                <th class="px-4 py-3 text-center text-sm font-medium text-gray-600">Statut</th>
                <th class="px-4 py-3 text-center text-sm font-medium text-gray-600">Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (invoice of invoices(); track invoice.id) {
                <tr class="border-t hover:bg-gray-50">
                  <td class="px-4 py-3">
                    <a [routerLink]="['/invoices', invoice.id]" class="text-blue-600 hover:underline">
                      {{ invoice.documentIdentifier }}
                    </a>
                  </td>
                  <td class="px-4 py-3 text-gray-600">
                    {{ invoice.invoiceDate | date:'dd/MM/yyyy' }}
                  </td>
                  <td class="px-4 py-3">{{ invoice.receiverName || 'N/A' }}</td>
                  <td class="px-4 py-3 text-right font-medium">
                    {{ invoice.totalIncludingTax | number:'1.3-3' }} TND
                  </td>
                  <td class="px-4 py-3 text-center">
                    <span [class]="getStatusClass(invoice.status)">
                      {{ getStatusLabel(invoice.status) }}
                    </span>
                  </td>
                  <td class="px-4 py-3 text-center">
                    <div class="flex justify-center gap-2">
                      <button class="text-blue-600 hover:text-blue-800" title="Télécharger PDF"
                              (click)="downloadPdf(invoice.id)">
                        📄
                      </button>
                      <button class="text-green-600 hover:text-green-800" title="Télécharger XML"
                              (click)="downloadXml(invoice.id)">
                        📋
                      </button>
                      @if (invoice.status < 3) {
                        <button class="text-purple-600 hover:text-purple-800" title="Soumettre à TTN"
                                (click)="submitToTtn(invoice.id)">
                          📤
                        </button>
                      }
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>

          <!-- Pagination -->
          <div class="flex justify-between items-center px-4 py-3 bg-gray-50">
            <span class="text-sm text-gray-600">
              {{ totalCount() }} facture(s) trouvée(s)
            </span>
            <div class="flex gap-2">
              <button class="px-3 py-1 border rounded hover:bg-gray-100"
                      [disabled]="pageNumber() <= 1"
                      (click)="previousPage()">
                Précédent
              </button>
              <span class="px-3 py-1">Page {{ pageNumber() }}</span>
              <button class="px-3 py-1 border rounded hover:bg-gray-100"
                      [disabled]="invoices().length < pageSize()"
                      (click)="nextPage()">
                Suivant
              </button>
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class InvoiceListComponent implements OnInit {
  private invoiceService = inject(InvoiceService);

  invoices = signal<InvoiceRecord[]>([]);
  loading = signal(true);
  totalCount = signal(0);
  pageNumber = signal(1);
  pageSize = signal(10);
  
  private statusFilter?: InvoiceStatus;
  private dateFrom?: Date;
  private dateTo?: Date;

  ngOnInit() {
    this.loadInvoices();
  }

  loadInvoices() {
    this.loading.set(true);
    this.invoiceService.getInvoices(
      this.pageNumber(),
      this.pageSize(),
      undefined,
      this.statusFilter,
      this.dateFrom,
      this.dateTo
    ).subscribe({
      next: (result) => {
        this.invoices.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onStatusChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.statusFilter = value ? parseInt(value) as InvoiceStatus : undefined;
  }

  onDateFromChange(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.dateFrom = value ? new Date(value) : undefined;
  }

  onDateToChange(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.dateTo = value ? new Date(value) : undefined;
  }

  previousPage() {
    if (this.pageNumber() > 1) {
      this.pageNumber.update(n => n - 1);
      this.loadInvoices();
    }
  }

  nextPage() {
    this.pageNumber.update(n => n + 1);
    this.loadInvoices();
  }

  getStatusLabel(status: InvoiceStatus): string {
    const labels: Record<InvoiceStatus, string> = {
      [InvoiceStatus.Draft]: 'Brouillon',
      [InvoiceStatus.Generated]: 'Générée',
      [InvoiceStatus.Signed]: 'Signée',
      [InvoiceStatus.Sent]: 'Envoyée',
      [InvoiceStatus.Pending]: 'En attente',
      [InvoiceStatus.Validated]: 'Validée',
      [InvoiceStatus.Rejected]: 'Rejetée',
      [InvoiceStatus.Cancelled]: 'Annulée'
    };
    return labels[status] || 'Inconnu';
  }

  getStatusClass(status: InvoiceStatus): string {
    const base = 'px-2 py-1 rounded-full text-xs font-medium';
    const colors: Record<InvoiceStatus, string> = {
      [InvoiceStatus.Draft]: 'bg-gray-100 text-gray-800',
      [InvoiceStatus.Generated]: 'bg-blue-100 text-blue-800',
      [InvoiceStatus.Signed]: 'bg-purple-100 text-purple-800',
      [InvoiceStatus.Sent]: 'bg-orange-100 text-orange-800',
      [InvoiceStatus.Pending]: 'bg-yellow-100 text-yellow-800',
      [InvoiceStatus.Validated]: 'bg-green-100 text-green-800',
      [InvoiceStatus.Rejected]: 'bg-red-100 text-red-800',
      [InvoiceStatus.Cancelled]: 'bg-gray-200 text-gray-600'
    };
    return `${base} ${colors[status] || ''}`;
  }

  downloadPdf(id: string) {
    this.invoiceService.downloadPdf(id).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `facture_${id}.pdf`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  downloadXml(id: string) {
    this.invoiceService.downloadXml(id).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `facture_${id}.xml`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  submitToTtn(id: string) {
    if (confirm('Voulez-vous soumettre cette facture à El Fatoora (TTN)?')) {
      this.invoiceService.submitToTtn(id).subscribe({
        next: (response) => {
          if (response.success) {
            alert(`Facture soumise avec succès! Référence TTN: ${response.ttnReference}`);
            this.loadInvoices();
          } else {
            alert(`Erreur: ${response.message}`);
          }
        },
        error: (err) => {
          alert('Erreur lors de la soumission');
        }
      });
    }
  }
}
