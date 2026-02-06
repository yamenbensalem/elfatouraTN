import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  CommandeVente,
  CreateCommandeVenteRequest,
  UpdateCommandeVenteRequest,
  CommandesVenteQueryParams
} from '../models/commande-vente.model';
import { PaginatedResponse } from '../models/client.model';

/**
 * Service for managing commandes de vente operations
 */
@Injectable({
  providedIn: 'root'
})
export class CommandesVenteService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = 'commandesvente';

  /**
   * Get all commandes de vente with optional pagination and filtering
   */
  getAll(params?: CommandesVenteQueryParams): Observable<PaginatedResponse<CommandeVente>> {
    const queryParams: { [param: string]: string | string[] } = {};

    if (params) {
      if (params.pageNumber) queryParams['pageNumber'] = params.pageNumber.toString();
      if (params.pageSize) queryParams['pageSize'] = params.pageSize.toString();
      if (params.sortBy) queryParams['sortBy'] = params.sortBy;
      if (params.sortDescending !== undefined) queryParams['sortDescending'] = params.sortDescending.toString();

      if (params.filter) {
        if (params.filter.search) queryParams['search'] = params.filter.search;
        if (params.filter.codeClient) queryParams['codeClient'] = params.filter.codeClient;
        if (params.filter.statut) queryParams['statut'] = params.filter.statut;
        if (params.filter.dateFrom) queryParams['dateFrom'] = params.filter.dateFrom.toISOString();
        if (params.filter.dateTo) queryParams['dateTo'] = params.filter.dateTo.toISOString();
      }
    }

    return this.apiService.get<PaginatedResponse<CommandeVente>>(this.endpoint, queryParams);
  }

  /**
   * Get a single commande de vente by code
   */
  getById(codeCommande: string): Observable<CommandeVente> {
    return this.apiService.get<CommandeVente>(`${this.endpoint}/${codeCommande}`);
  }

  /**
   * Create a new commande de vente
   */
  create(commande: CreateCommandeVenteRequest): Observable<CommandeVente> {
    return this.apiService.post<CommandeVente>(this.endpoint, commande);
  }

  /**
   * Update an existing commande de vente
   */
  update(codeCommande: string, commande: UpdateCommandeVenteRequest): Observable<CommandeVente> {
    return this.apiService.put<CommandeVente>(`${this.endpoint}/${codeCommande}`, commande);
  }

  /**
   * Delete a commande de vente
   */
  delete(codeCommande: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${codeCommande}`);
  }
}
