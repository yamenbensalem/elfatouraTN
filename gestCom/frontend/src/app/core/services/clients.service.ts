import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Client,
  CreateClientRequest,
  UpdateClientRequest,
  PaginatedResponse,
  ClientsQueryParams
} from '../models/client.model';

/**
 * Service for managing client operations
 */
@Injectable({
  providedIn: 'root'
})
export class ClientsService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = 'clients';

  /**
   * Get all clients with optional pagination and filtering
   */
  getAll(params?: ClientsQueryParams): Observable<PaginatedResponse<Client>> {
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
        if (params.filter.exonere !== undefined) queryParams['exonere'] = params.filter.exonere.toString();
      }
    }

    return this.apiService.get<PaginatedResponse<Client>>(this.endpoint, queryParams);
  }

  /**
   * Get a single client by code
   */
  getById(codeClient: string): Observable<Client> {
    return this.apiService.get<Client>(`${this.endpoint}/${codeClient}`);
  }

  /**
   * Create a new client
   */
  create(client: CreateClientRequest): Observable<Client> {
    return this.apiService.post<Client>(this.endpoint, client);
  }

  /**
   * Update an existing client
   */
  update(codeClient: string, client: UpdateClientRequest): Observable<Client> {
    return this.apiService.put<Client>(`${this.endpoint}/${codeClient}`, client);
  }

  /**
   * Delete a client
   */
  delete(codeClient: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${codeClient}`);
  }

  /**
   * Check if a client code is available
   */
  checkCodeAvailable(codeClient: string): Observable<boolean> {
    return this.apiService.get<boolean>(`${this.endpoint}/check-code/${codeClient}`);
  }
}
