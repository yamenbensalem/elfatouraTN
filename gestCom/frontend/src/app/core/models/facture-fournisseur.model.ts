export interface LigneFactureFournisseur {
  id: number;
  codeProduit: string;
  designation: string;
  quantite: number;
  prixUnitaireHT: number;
  tauxRemise: number;
  montantRemise: number;
  tauxTVA: number;
  montantHT: number;
  montantTVA: number;
  montantTTC: number;
}

export interface FactureFournisseur {
  codeFacture: string;
  dateFacture: Date;
  codeFournisseur: string;
  raisonSocialeFournisseur?: string;
  numeroFactureFournisseur?: string;
  montantHT: number;
  montantTVA: number;
  montantTTC: number;
  tauxRemiseGlobale: number;
  montantTimbre: number;
  netAPayer: number;
  montantPaye: number;
  resteAPayer: number;
  statut: string;
  notes?: string;
  lignes: LigneFactureFournisseur[];
  dateCreation?: Date;
}

export interface CreateFactureFournisseurRequest {
  codeFacture: string;
  dateFacture: Date;
  codeFournisseur: string;
  numeroFactureFournisseur?: string;
  tauxRemiseGlobale?: number;
  notes?: string;
  lignes: { codeProduit: string; quantite: number; prixUnitaireHT: number; tauxRemise?: number; }[];
}

export interface UpdateFactureFournisseurRequest {
  dateFacture?: Date;
  codeFournisseur?: string;
  numeroFactureFournisseur?: string;
  tauxRemiseGlobale?: number;
  statut?: string;
  notes?: string;
  lignes?: { codeProduit: string; quantite: number; prixUnitaireHT: number; tauxRemise?: number; }[];
}

export interface FacturesFournisseurFilter {
  search?: string;
  codeFournisseur?: string;
  statut?: string;
  dateFrom?: Date;
  dateTo?: Date;
}

export interface FacturesFournisseurQueryParams {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
  filter?: FacturesFournisseurFilter;
}
