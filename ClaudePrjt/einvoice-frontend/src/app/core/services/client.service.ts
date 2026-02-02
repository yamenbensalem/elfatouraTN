import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Client } from '../models/invoice.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ClientService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/clients`;

  /**
   * Get all clients
   */
  getClients(): Observable<Client[]> {
    return this.http.get<Client[]>(this.baseUrl);
  }

  /**
   * Get client by ID
   */
  getClient(id: string): Observable<Client> {
    return this.http.get<Client>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get client by Matricule Fiscal
   */
  getClientByMatricule(matriculeFiscal: string): Observable<Client> {
    return this.http.get<Client>(`${this.baseUrl}/matricule/${matriculeFiscal}`);
  }

  /**
   * Search clients
   */
  searchClients(searchTerm: string): Observable<Client[]> {
    return this.http.get<Client[]>(`${this.baseUrl}/search`, {
      params: { q: searchTerm }
    });
  }

  /**
   * Create new client
   */
  createClient(client: Omit<Client, 'id' | 'createdAt'>): Observable<Client> {
    return this.http.post<Client>(this.baseUrl, client);
  }

  /**
   * Update client
   */
  updateClient(id: string, client: Partial<Client>): Observable<Client> {
    return this.http.put<Client>(`${this.baseUrl}/${id}`, client);
  }

  /**
   * Delete client
   */
  deleteClient(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * Check if matricule fiscal exists
   */
  checkMatriculeExists(matriculeFiscal: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/exists/${matriculeFiscal}`);
  }
}
