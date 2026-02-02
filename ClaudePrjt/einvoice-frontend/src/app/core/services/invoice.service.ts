import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  InvoiceRequest, 
  InvoiceResponse, 
  InvoiceRecord, 
  PagedResult,
  InvoiceStatus 
} from '../models/invoice.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/invoice`;

  /**
   * Generate a new invoice (creates XML and PDF)
   */
  generateInvoice(request: InvoiceRequest): Observable<InvoiceResponse> {
    return this.http.post<InvoiceResponse>(`${this.baseUrl}/generate`, request);
  }

  /**
   * Validate invoice data without generating
   */
  validateInvoice(request: InvoiceRequest): Observable<InvoiceResponse> {
    return this.http.post<InvoiceResponse>(`${this.baseUrl}/validate`, request);
  }

  /**
   * Get all stored invoices with pagination
   */
  getInvoices(
    pageNumber: number = 1,
    pageSize: number = 10,
    clientId?: string,
    status?: InvoiceStatus,
    fromDate?: Date,
    toDate?: Date
  ): Observable<PagedResult<InvoiceRecord>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (clientId) params = params.set('clientId', clientId);
    if (status !== undefined) params = params.set('status', status.toString());
    if (fromDate) params = params.set('fromDate', fromDate.toISOString());
    if (toDate) params = params.set('toDate', toDate.toISOString());

    return this.http.get<PagedResult<InvoiceRecord>>(this.baseUrl, { params });
  }

  /**
   * Get single invoice by ID
   */
  getInvoice(id: string): Observable<InvoiceRecord> {
    return this.http.get<InvoiceRecord>(`${this.baseUrl}/${id}`);
  }

  /**
   * Download invoice XML
   */
  downloadXml(id: string, signed: boolean = true): Observable<Blob> {
    const endpoint = signed ? 'xml-signed' : 'xml';
    return this.http.get(`${this.baseUrl}/${id}/${endpoint}`, { responseType: 'blob' });
  }

  /**
   * Download invoice PDF
   */
  downloadPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/pdf`, { responseType: 'blob' });
  }

  /**
   * Submit invoice to TTN
   */
  submitToTtn(id: string): Observable<InvoiceResponse> {
    return this.http.post<InvoiceResponse>(`${this.baseUrl}/${id}/submit-ttn`, {});
  }

  /**
   * Check TTN status
   */
  checkTtnStatus(id: string): Observable<InvoiceResponse> {
    return this.http.get<InvoiceResponse>(`${this.baseUrl}/${id}/ttn-status`);
  }

  /**
   * Cancel an invoice
   */
  cancelInvoice(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/cancel`, {});
  }

  /**
   * Delete a draft invoice
   */
  deleteInvoice(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
