import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  CommandeAchat,
  CreateCommandeAchatRequest,
  UpdateCommandeAchatRequest,
  CommandesAchatQueryParams
} from '../models/commande-achat.model';
import { PaginatedResponse } from '../models/client.model';

/**
 * Service for managing commandes d'achat operations
 */
@Injectable({
  providedIn: 'root'
})
export class CommandesAchatService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = 'commandes-achat';

  /**
   * Get all commandes d'achat with optional pagination and filtering
   */
  getAll(params?: CommandesAchatQueryParams): Observable<PaginatedResponse<CommandeAchat>> {
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

    return this.apiService.get<PaginatedResponse<CommandeAchat>>(this.endpoint, queryParams);
  }

  /**
   * Get a single commande d'achat by code
   */
  getById(codeCommande: string): Observable<CommandeAchat> {
    return this.apiService.get<CommandeAchat>(`${this.endpoint}/${codeCommande}`);
  }

  /**
   * Create a new commande d'achat
   */
  create(commande: CreateCommandeAchatRequest): Observable<CommandeAchat> {
    return this.apiService.post<CommandeAchat>(this.endpoint, commande);
  }

  /**
   * Update an existing commande d'achat
   */
  update(codeCommande: string, commande: UpdateCommandeAchatRequest): Observable<CommandeAchat> {
    return this.apiService.put<CommandeAchat>(`${this.endpoint}/${codeCommande}`, commande);
  }

  /**
   * Delete a commande d'achat
   */
  delete(codeCommande: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${codeCommande}`);
  }
}
