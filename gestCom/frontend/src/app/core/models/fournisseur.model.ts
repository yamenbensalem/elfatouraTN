export interface Fournisseur {
  codeFournisseur: string;
  raisonSociale: string;
  adresse?: string;
  ville?: string;
  codePostal?: string;
  telephone?: string;
  fax?: string;
  email?: string;
  matriculeFiscale?: string;
  registreCommerce?: string;
  soldeInitial: number;
  soldeActuel: number;
  delaiPaiement: number;
  exonere: boolean;
  bloque: boolean;
  dateCreation?: Date;
  notes?: string;
}

export interface CreateFournisseurRequest {
  codeFournisseur: string;
  raisonSociale: string;
  adresse?: string;
  ville?: string;
  codePostal?: string;
  telephone?: string;
  fax?: string;
  email?: string;
  matriculeFiscale?: string;
  registreCommerce?: string;
  soldeInitial?: number;
  delaiPaiement?: number;
  exonere?: boolean;
  notes?: string;
}

export interface UpdateFournisseurRequest {
  raisonSociale?: string;
  adresse?: string;
  ville?: string;
  codePostal?: string;
  telephone?: string;
  fax?: string;
  email?: string;
  matriculeFiscale?: string;
  registreCommerce?: string;
  delaiPaiement?: number;
  exonere?: boolean;
  bloque?: boolean;
  notes?: string;
}

/**
 * Paginated response wrapper
 */
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Pagination parameters for requests
 */
export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
}

/**
 * Fournisseur filter parameters
 */
export interface FournisseursFilter {
  search?: string;
  ville?: string;
  bloque?: boolean;
}

/**
 * Combined query parameters for fournisseurs
 */
export interface FournisseursQueryParams extends PaginationParams {
  filter?: FournisseursFilter;
}
