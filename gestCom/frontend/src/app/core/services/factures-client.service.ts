import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  FactureClient,
  CreateFactureClientRequest,
  UpdateFactureClientRequest,
} from '../models/facture-client.model';
import { PaginatedResponse } from '../models/client.model';

/**
 * Service for managing factures client (customer invoices) operations
 */
@Injectable({
  providedIn: 'root'
})
export class FacturesClientService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = 'factures/clients';

  /**
   * Get all factures client with optional pagination and filtering
   */
  getAll(params?: { [param: string]: string | string[] }): Observable<PaginatedResponse<FactureClient>> {
    return this.apiService.get<PaginatedResponse<FactureClient>>(this.endpoint, params);
  }

  /**
   * Get a single facture client by code
   */
  getById(codeFacture: string): Observable<FactureClient> {
    return this.apiService.get<FactureClient>(`${this.endpoint}/${codeFacture}`);
  }

  /**
   * Create a new facture client
   */
  create(facture: CreateFactureClientRequest): Observable<FactureClient> {
    return this.apiService.post<FactureClient>(this.endpoint, facture);
  }

  /**
   * Update an existing facture client
   */
  update(codeFacture: string, facture: UpdateFactureClientRequest): Observable<FactureClient> {
    return this.apiService.put<FactureClient>(`${this.endpoint}/${codeFacture}`, facture);
  }

  /**
   * Delete a facture client
   */
  delete(codeFacture: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${codeFacture}`);
  }
}
