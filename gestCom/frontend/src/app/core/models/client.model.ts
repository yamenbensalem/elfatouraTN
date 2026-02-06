/**
 * Client model matching backend DTO
 */
export interface Client {
  codeClient: string;
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
  plafondCredit: number;
  delaiPaiement: number;
  exonere: boolean;
  bloque: boolean;
  dateCreation?: Date;
  notes?: string;
}

/**
 * Create client request DTO
 */
export interface CreateClientRequest {
  codeClient: string;
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
  plafondCredit?: number;
  delaiPaiement?: number;
  exonere?: boolean;
  notes?: string;
}

/**
 * Update client request DTO
 */
export interface UpdateClientRequest {
  raisonSociale?: string;
  adresse?: string;
  ville?: string;
  codePostal?: string;
  telephone?: string;
  fax?: string;
  email?: string;
  matriculeFiscale?: string;
  registreCommerce?: string;
  plafondCredit?: number;
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
 * Client filter parameters
 */
export interface ClientsFilter {
  search?: string;
  ville?: string;
  bloque?: boolean;
  exonere?: boolean;
}

/**
 * Combined query parameters for clients
 */
export interface ClientsQueryParams extends PaginationParams {
  filter?: ClientsFilter;
}
