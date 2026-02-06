import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Devis,
  CreateDevisRequest,
  UpdateDevisRequest,
} from '../models/devis.model';
import { PaginatedResponse } from '../models/client.model';

/**
 * Service for managing devis (customer quotes) operations
 */
@Injectable({
  providedIn: 'root'
})
export class DevisService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = 'devis';

  /**
   * Get all devis with optional pagination and filtering
   */
  getAll(params?: { [param: string]: string | string[] }): Observable<PaginatedResponse<Devis>> {
    return this.apiService.get<PaginatedResponse<Devis>>(this.endpoint, params);
  }

  /**
   * Get a single devis by code
   */
  getById(codeDevis: string): Observable<Devis> {
    return this.apiService.get<Devis>(`${this.endpoint}/${codeDevis}`);
  }

  /**
   * Create a new devis
   */
  create(devis: CreateDevisRequest): Observable<Devis> {
    return this.apiService.post<Devis>(this.endpoint, devis);
  }

  /**
   * Update an existing devis
   */
  update(codeDevis: string, devis: UpdateDevisRequest): Observable<Devis> {
    return this.apiService.put<Devis>(`${this.endpoint}/${codeDevis}`, devis);
  }

  /**
   * Delete a devis
   */
  delete(codeDevis: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${codeDevis}`);
  }
}
