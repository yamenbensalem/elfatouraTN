import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { InvoiceService } from '../../core/services/invoice.service';
import { InvoiceRecord, InvoiceStatus } from '../../core/models/invoice.model';

@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container mx-auto p-4 max-w-4xl">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold text-gray-800">Détail Facture</h1>
        <a routerLink="/invoices" class="text-gray-600 hover:text-gray-800">
          ← Retour à la liste
        </a>
      </div>

      @if (loading()) {
        <div class="text-center p-8 text-gray-500">Chargement...</div>
      } @else if (invoice()) {
        <div class="space-y-6">
          <!-- Header Info -->
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex justify-between items-start">
              <div>
                <h2 class="text-xl font-bold">{{ invoice()!.documentIdentifier }}</h2>
                <p class="text-gray-600">{{ invoice()!.documentTypeName }}</p>
              </div>
              <span [class]="getStatusClass(invoice()!.status)">
                {{ getStatusLabel(invoice()!.status) }}
              </span>
            </div>
            
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
              <div>
                <span class="text-sm text-gray-500">Date facture</span>
                <p class="font-medium">{{ invoice()!.invoiceDate | date:'dd/MM/yyyy' }}</p>
              </div>
              <div>
                <span class="text-sm text-gray-500">Échéance</span>
                <p class="font-medium">{{ invoice()!.dueDate | date:'dd/MM/yyyy' }}</p>
              </div>
              <div>
                <span class="text-sm text-gray-500">Référence TTN</span>
                <p class="font-medium">{{ invoice()!.ttnReference || '-' }}</p>
              </div>
              <div>
                <span class="text-sm text-gray-500">Créé le</span>
                <p class="font-medium">{{ invoice()!.createdAt | date:'dd/MM/yyyy HH:mm' }}</p>
              </div>
            </div>
          </div>

          <!-- Amounts -->
          <div class="bg-white rounded-lg shadow p-6">
            <h3 class="font-semibold text-gray-700 mb-4">Montants</h3>
            <div class="grid grid-cols-2 md:grid-cols-5 gap-4">
              <div>
                <span class="text-sm text-gray-500">Total HT</span>
                <p class="font-medium">{{ invoice()!.totalExcludingTax | number:'1.3-3' }} TND</p>
              </div>
              <div>
                <span class="text-sm text-gray-500">Total TVA</span>
                <p class="font-medium">{{ invoice()!.totalTaxAmount | number:'1.3-3' }} TND</p>
              </div>
              <div>
                <span class="text-sm text-gray-500">Timbre</span>
                <p class="font-medium">{{ invoice()!.stampDuty | number:'1.3-3' }} TND</p>
              </div>
              <div class="md:col-span-2">
                <span class="text-sm text-gray-500">Total TTC</span>
                <p class="text-xl font-bold text-blue-600">{{ invoice()!.totalIncludingTax | number:'1.3-3' }} TND</p>
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="bg-white rounded-lg shadow p-6">
            <h3 class="font-semibold text-gray-700 mb-4">Actions</h3>
            <div class="flex flex-wrap gap-3">
              <button (click)="downloadPdf()" 
                      class="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700">
                📄 Télécharger PDF
              </button>
              <button (click)="downloadXml(false)"
                      class="bg-orange-600 text-white px-4 py-2 rounded hover:bg-orange-700">
                📋 XML (non signé)
              </button>
              <button (click)="downloadXml(true)"
                      class="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700">
                📋 XML (signé)
              </button>
              @if (invoice()!.status < 3) {
                <button (click)="submitToTtn()"
                        class="bg-purple-600 text-white px-4 py-2 rounded hover:bg-purple-700">
                  📤 Soumettre à TTN
                </button>
              }
              @if (invoice()!.status === 3 || invoice()!.status === 4) {
                <button (click)="checkStatus()"
                        class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
                  🔄 Vérifier statut TTN
                </button>
              }
            </div>
          </div>

          <!-- Status Message -->
          @if (invoice()!.statusMessage) {
            <div class="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
              <p class="text-yellow-800">{{ invoice()!.statusMessage }}</p>
            </div>
          }
        </div>
      }
    </div>
  `
})
export class InvoiceDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private invoiceService = inject(InvoiceService);

  invoice = signal<InvoiceRecord | null>(null);
  loading = signal(true);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadInvoice(id);
    }
  }

  loadInvoice(id: string) {
    this.loading.set(true);
    this.invoiceService.getInvoice(id).subscribe({
      next: (inv) => {
        this.invoice.set(inv);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
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
    const base = 'px-3 py-1 rounded-full text-sm font-medium';
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

  downloadPdf() {
    const inv = this.invoice();
    if (inv) {
      this.invoiceService.downloadPdf(inv.id).subscribe(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `facture_${inv.documentIdentifier}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      });
    }
  }

  downloadXml(signed: boolean) {
    const inv = this.invoice();
    if (inv) {
      this.invoiceService.downloadXml(inv.id, signed).subscribe(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `facture_${inv.documentIdentifier}${signed ? '_signed' : ''}.xml`;
        a.click();
        window.URL.revokeObjectURL(url);
      });
    }
  }

  submitToTtn() {
    const inv = this.invoice();
    if (inv && confirm('Voulez-vous soumettre cette facture à El Fatoora (TTN)?')) {
      this.invoiceService.submitToTtn(inv.id).subscribe({
        next: (response) => {
          if (response.success) {
            alert(`Facture soumise! Référence TTN: ${response.ttnReference}`);
            this.loadInvoice(inv.id);
          } else {
            alert(`Erreur: ${response.message}`);
          }
        },
        error: (err) => alert('Erreur lors de la soumission')
      });
    }
  }

  checkStatus() {
    const inv = this.invoice();
    if (inv) {
      this.invoiceService.checkTtnStatus(inv.id).subscribe({
        next: (response) => {
          alert(`Statut: ${response.message}`);
          this.loadInvoice(inv.id);
        },
        error: (err) => alert('Erreur lors de la vérification')
      });
    }
  }
}
