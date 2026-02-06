import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Fournisseur,
  CreateFournisseurRequest,
  UpdateFournisseurRequest,
  PaginatedResponse,
  FournisseursQueryParams
} from '../models/fournisseur.model';

/**
 * Service for managing fournisseur operations
 */
@Injectable({
  providedIn: 'root'
})
export class FournisseursService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = 'fournisseurs';

  /**
   * Get all fournisseurs with optional pagination and filtering
   */
  getAll(params?: FournisseursQueryParams): Observable<PaginatedResponse<Fournisseur>> {
    const queryParams: { [param: string]: string | string[] } = {};

    if (params) {
      if (params.pageNumber) queryParams['pageNumber'] = params.pageNumber.toString();
      if (params.pageSize) queryParams['pageSize'] = params.pageSize.toString();
      if (params.sortBy) queryParams['sortBy'] = params.sortBy;
      if (params.sortDescending !== undefined) queryParams['sortDescending'] = params.sortDescending.toString();

      if (params.filter) {
        if (params.filter.search) queryParams['search'] = params.filter.search;
        if (params.filter.ville) queryParams['ville'] = params.filter.ville;
        if (params.filter.bloque !== undefined) queryParams['bloque'] = params.filter.bloque.toString();
      }
    }

    return this.apiService.get<PaginatedResponse<Fournisseur>>(this.endpoint, queryParams);
  }

  /**
   * Get a single fournisseur by code
   */
  getById(codeFournisseur: string): Observable<Fournisseur> {
    return this.apiService.get<Fournisseur>(`${this.endpoint}/${codeFournisseur}`);
  }

  /**
   * Create a new fournisseur
   */
  create(fournisseur: CreateFournisseurRequest): Observable<Fournisseur> {
    return this.apiService.post<Fournisseur>(this.endpoint, fournisseur);
  }

  /**
   * Update an existing fournisseur
   */
  update(codeFournisseur: string, fournisseur: UpdateFournisseurRequest): Observable<Fournisseur> {
    return this.apiService.put<Fournisseur>(`${this.endpoint}/${codeFournisseur}`, fournisseur);
  }

  /**
   * Delete a fournisseur
   */
  delete(codeFournisseur: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${codeFournisseur}`);
  }

  /**
   * Check if a fournisseur code is available
   */
  checkCodeAvailable(codeFournisseur: string): Observable<boolean> {
    return this.apiService.get<boolean>(`${this.endpoint}/check-code/${codeFournisseur}`);
  }
}
