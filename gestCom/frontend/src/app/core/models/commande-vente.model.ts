export interface LigneCommandeVente {
  id: number;
  codeProduit: string;
  designation: string;
  quantite: number;
  prixUnitaireHT: number;
  tauxRemise: number;
  montantRemise: number;
  montantHT: number;
}

export interface CommandeVente {
  codeCommande: string;
  dateCommande: Date;
  codeClient: string;
  raisonSocialeClient?: string;
  montantHT: number;
  montantTVA: number;
  montantTTC: number;
  statut: string;
  dateLivraisonPrevue?: Date;
  notes?: string;
  lignes: LigneCommandeVente[];
  dateCreation?: Date;
}

export interface CreateCommandeVenteRequest {
  codeCommande: string;
  dateCommande: Date;
  codeClient: string;
  dateLivraisonPrevue?: Date;
  notes?: string;
  lignes: { codeProduit: string; quantite: number; prixUnitaireHT: number; tauxRemise?: number; }[];
}

export interface UpdateCommandeVenteRequest {
  dateCommande?: Date;
  codeClient?: string;
  dateLivraisonPrevue?: Date;
  statut?: string;
  notes?: string;
  lignes?: { codeProduit: string; quantite: number; prixUnitaireHT: number; tauxRemise?: number; }[];
}

export interface CommandesVenteFilter {
  search?: string;
  codeClient?: string;
  statut?: string;
  dateFrom?: Date;
  dateTo?: Date;
}

export interface CommandesVenteQueryParams {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
  filter?: CommandesVenteFilter;
}
