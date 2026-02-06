import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Produit,
  CreateProduitRequest,
  UpdateProduitRequest,
  ProduitsQueryParams
} from '../models/produit.model';
import { PaginatedResponse } from '../models/client.model';

/**
 * Service for managing produit operations
 */
@Injectable({
  providedIn: 'root'
})
export class ProduitsService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = '/produits';

  /**
   * Get all produits with optional pagination and filtering
   */
  getAll(params?: ProduitsQueryParams): Observable<PaginatedResponse<Produit>> {
    const queryParams: { [param: string]: string | string[] } = {};

    if (params) {
      if (params.pageNumber) queryParams['pageNumber'] = params.pageNumber.toString();
      if (params.pageSize) queryParams['pageSize'] = params.pageSize.toString();
      if (params.sortBy) queryParams['sortBy'] = params.sortBy;
      if (params.sortDescending !== undefined) queryParams['sortDescending'] = params.sortDescending.toString();

      if (params.filter) {
        if (params.filter.search) queryParams['search'] = params.filter.search;
        if (params.filter.codeCategorie) queryParams['codeCategorie'] = params.filter.codeCategorie;
        if (params.filter.actif !== undefined) queryParams['actif'] = params.filter.actif.toString();
        if (params.filter.stockBas !== undefined) queryParams['stockBas'] = params.filter.stockBas.toString();
      }
    }

    return this.apiService.get<PaginatedResponse<Produit>>(this.endpoint, queryParams);
  }

  /**
   * Get a single produit by code
   */
  getById(codeProduit: string): Observable<Produit> {
    return this.apiService.get<Produit>(`${this.endpoint}/${codeProduit}`);
  }

  /**
   * Create a new produit
   */
  create(produit: CreateProduitRequest): Observable<Produit> {
    return this.apiService.post<Produit>(this.endpoint, produit);
  }

  /**
   * Update an existing produit
   */
  update(codeProduit: string, produit: UpdateProduitRequest): Observable<Produit> {
    return this.apiService.put<Produit>(`${this.endpoint}/${codeProduit}`, produit);
  }

  /**
   * Delete a produit
   */
  delete(codeProduit: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${codeProduit}`);
  }

  /**
   * Check if a produit code is available
   */
  checkCodeAvailable(codeProduit: string): Observable<boolean> {
    return this.apiService.get<boolean>(`${this.endpoint}/check-code/${codeProduit}`);
  }
}
