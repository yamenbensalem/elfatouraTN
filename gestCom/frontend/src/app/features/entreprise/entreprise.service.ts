import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface EntrepriseDto {
  codeEntreprise: string;
  raisonSociale?: string;
  matriculeFiscal?: string;
  adresse?: string;
  codePostal?: string;
  ville?: string;
  telephone?: string;
  fax?: string;
  email?: string;
  siteWeb?: string;
  rib?: string;
  nomBanque?: string;
  codeDevise?: string;
  logo?: string;
}

@Injectable({
  providedIn: 'root'
})
export class EntrepriseService {
  private apiUrl = `${environment.apiUrl}/entreprise`;

  constructor(private http: HttpClient) {}

  getEntreprise(): Observable<EntrepriseDto> {
    return this.http.get<EntrepriseDto>(this.apiUrl);
  }

  updateEntreprise(data: EntrepriseDto): Observable<EntrepriseDto> {
    return this.http.put<EntrepriseDto>(this.apiUrl, data);
  }
}
