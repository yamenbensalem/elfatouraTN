import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * GET request
   */
  get<T>(endpoint: string, params?: HttpParams | { [param: string]: string | string[] }): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}/${endpoint}`, { params })
      .pipe(catchError(this.handleError));
  }

  /**
   * POST request
   */
  post<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, body)
      .pipe(catchError(this.handleError));
  }

  /**
   * PUT request
   */
  put<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${endpoint}`, body)
      .pipe(catchError(this.handleError));
  }

  /**
   * PATCH request
   */
  patch<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}/${endpoint}`, body)
      .pipe(catchError(this.handleError));
  }

  /**
   * DELETE request
   */
  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}/${endpoint}`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Handle HTTP errors — re-throws the original HttpErrorResponse
   * so NgRx effects and callers can inspect error.status / error.error.
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let userMessage = 'Une erreur inattendue s\'est produite';

    if (error.error instanceof ErrorEvent) {
      // Client-side / network error
      userMessage = `Erreur: ${error.error.message}`;
    } else {
      // Server-side error
      switch (error.status) {
        case 0:
          userMessage = 'Impossible de contacter le serveur. Vérifiez votre connexion.';
          break;
        case 400:
          userMessage = error.error?.message || 'Requête invalide';
          break;
        case 401:
          userMessage = 'Non autorisé. Veuillez vous connecter.';
          break;
        case 403:
          userMessage = 'Accès interdit. Vous n\'avez pas les permissions nécessaires.';
          break;
        case 404:
          userMessage = 'Ressource non trouvée';
          break;
        case 409:
          userMessage = error.error?.message || 'Conflit de données';
          break;
        case 422:
          userMessage = error.error?.message || 'Données de validation invalides';
          break;
        case 500:
          userMessage = 'Erreur serveur. Veuillez réessayer plus tard.';
          break;
        default:
          userMessage = `Erreur ${error.status}: ${error.error?.message || error.message}`;
      }
    }

    console.error('API Error:', userMessage, error);

    // Re-throw the original HttpErrorResponse so callers/effects can access
    // error.status, error.error, etc. Attach a user-friendly message.
    return throwError(() => ({
      ...error,
      userMessage
    }));
  }
}
