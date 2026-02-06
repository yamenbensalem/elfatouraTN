import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  FactureFournisseur,
  CreateFactureFournisseurRequest,
  UpdateFactureFournisseurRequest,
  FacturesFournisseurQueryParams
} from '../models/facture-fournisseur.model';
import { PaginatedResponse } from '../models/client.model';

/**
 * Service for managing factures fournisseur operations
 */
@Injectable({
  providedIn: 'root'
})
export class FacturesFournisseurService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = 'factures/fournisseurs';

  /**
   * Get all factures fournisseur with optional pagination and filtering
   */
  getAll(params?: FacturesFournisseurQueryParams): Observable<PaginatedResponse<FactureFournisseur>> {
    const queryParams: { [param: string]: string | string[] } = {};

    if (params) {
      if (params.pageNumber) queryParams['pageNumber'] = params.pageNumber.toString();
      if (params.pageSize) queryParams['pageSize'] = params.pageSize.toString();
      if (params.sortBy) queryParams['sortBy'] = params.sortBy;
      if (params.sortDescending !== undefined) queryParams['sortDescending'] = params.sortDescending.toString();

      if (params.filter) {
        if (params.filter.search) queryParams['search'] = params.filter.search;
        if (params.filter.codeFournisseur) queryParams['codeFournisseur'] = params.filter.codeFournisseur;
        if (params.filter.statut) queryParams['statut'] = params.filter.statut;
        if (params.filter.dateFrom) queryParams['dateFrom'] = params.filter.dateFrom.toISOString();
        if (params.filter.dateTo) queryParams['dateTo'] = params.filter.dateTo.toISOString();
      }
    }

    return this.apiService.get<PaginatedResponse<FactureFournisseur>>(this.endpoint, queryParams);
  }

  /**
   * Get a single facture fournisseur by code
   */
  getById(codeFacture: string): Observable<FactureFournisseur> {
    return this.apiService.get<FactureFournisseur>(`${this.endpoint}/${codeFacture}`);
  }

  /**
   * Create a new facture fournisseur
   */
  create(facture: CreateFactureFournisseurRequest): Observable<FactureFournisseur> {
    return this.apiService.post<FactureFournisseur>(this.endpoint, facture);
  }

  /**
   * Update an existing facture fournisseur
   */
  update(codeFacture: string, facture: UpdateFactureFournisseurRequest): Observable<FactureFournisseur> {
    return this.apiService.put<FactureFournisseur>(`${this.endpoint}/${codeFacture}`, facture);
  }

  /**
   * Delete a facture fournisseur
   */
  delete(codeFacture: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${codeFacture}`);
  }
}
