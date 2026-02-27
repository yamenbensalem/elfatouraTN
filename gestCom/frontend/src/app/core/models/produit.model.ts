export interface Produit {
  codeProduit: string;
  designation: string;
  codeCategorie?: string;
  nomCategorie?: string;
  prixAchatHT: number;
  prixVenteHT: number;
  tauxTVA: number;
  tauxFODEC: number;
  unite?: string;
  codeBarres?: string;
  isService?: boolean; // true for services (no stock)
  stockActuel: number;
  stockMinimum: number;
  stockMaximum: number;
  emplacement?: string;
  actif: boolean;
  dateCreation?: Date;
  notes?: string;
}

export interface CreateProduitRequest {
  codeProduit: string;
  designation: string;
  codeCategorie?: string;
  prixAchatHT?: number;
  prixVenteHT?: number;
  tauxTVA?: number;
  tauxFODEC?: number;
  unite?: string;
  codeBarres?: string;
  isService?: boolean;
  stockMinimum?: number;
  stockMaximum?: number;
  emplacement?: string;
  notes?: string;
}

export interface UpdateProduitRequest {
  designation?: string;
  codeCategorie?: string;
  prixAchatHT?: number;
  prixVenteHT?: number;
  tauxTVA?: number;
  tauxFODEC?: number;
  unite?: string;
  codeBarres?: string;
  isService?: boolean;
  stockMinimum?: number;
  stockMaximum?: number;
  emplacement?: string;
  actif?: boolean;
  notes?: string;
}

export interface ProduitsFilter {
  search?: string;
  codeCategorie?: string;
  actif?: boolean;
  stockBas?: boolean;
}

import { PaginatedResponse, PaginationParams } from './client.model';

/**
 * Combined query parameters for produits
 */
export interface ProduitsQueryParams extends PaginationParams {
  filter?: ProduitsFilter;
}
