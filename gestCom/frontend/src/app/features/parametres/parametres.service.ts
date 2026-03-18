import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ParametresDecimalesDto {
  decimalesQuantite: number;
  decimalesPrix: number;
  decimalesMontant: number;
  decimalesTva: number;
}

@Injectable({
  providedIn: 'root'
})
export class ParametresService {
  private apiUrl = `${environment.apiUrl}/entreprise/parametres-decimales`;

  constructor(private http: HttpClient) {}

  getParametresDecimales(): Observable<ParametresDecimalesDto> {
    return this.http.get<ParametresDecimalesDto>(this.apiUrl);
  }

  updateParametresDecimales(data: ParametresDecimalesDto): Observable<ParametresDecimalesDto> {
    return this.http.put<ParametresDecimalesDto>(this.apiUrl, data);
  }
}
