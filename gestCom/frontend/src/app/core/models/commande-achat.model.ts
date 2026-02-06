export interface LigneCommandeAchat {
  id: number;
  codeProduit: string;
  designation: string;
  quantite: number;
  prixUnitaireHT: number;
  tauxRemise: number;
  montantRemise: number;
  montantHT: number;
}

export interface CommandeAchat {
  codeCommande: string;
  dateCommande: Date;
  codeFournisseur: string;
  raisonSocialeFournisseur?: string;
  montantHT: number;
  montantTVA: number;
  montantTTC: number;
  statut: string;
  dateReceptionPrevue?: Date;
  notes?: string;
  lignes: LigneCommandeAchat[];
  dateCreation?: Date;
}

export interface CreateCommandeAchatRequest {
  codeCommande: string;
  dateCommande: Date;
  codeFournisseur: string;
  dateReceptionPrevue?: Date;
  notes?: string;
  lignes: { codeProduit: string; quantite: number; prixUnitaireHT: number; tauxRemise?: number; }[];
}

export interface UpdateCommandeAchatRequest {
  dateCommande?: Date;
  codeFournisseur?: string;
  dateReceptionPrevue?: Date;
  statut?: string;
  notes?: string;
  lignes?: { codeProduit: string; quantite: number; prixUnitaireHT: number; tauxRemise?: number; }[];
}

export interface CommandesAchatFilter {
  search?: string;
  codeFournisseur?: string;
  statut?: string;
  dateFrom?: Date;
  dateTo?: Date;
}

export interface CommandesAchatQueryParams {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
  filter?: CommandesAchatFilter;
}
