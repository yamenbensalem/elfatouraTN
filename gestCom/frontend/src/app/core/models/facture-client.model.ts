export interface LigneFactureClient {
  id: number;
  codeProduit: string;
  designation: string;
  quantite: number;
  prixUnitaireHT: number;
  tauxRemise: number;
  montantRemise: number;
  tauxTVA: number;
  tauxFodec: number;
  montantFodec: number;
  montantHT: number;
  montantTVA: number;
  montantTTC: number;
}

export interface FactureClient {
  codeFacture: string;
  dateFacture: Date;
  codeClient: string;
  raisonSocialeClient?: string;
  montantHT: number;
  montantTVA: number;
  montantTTC: number;
  tauxRemiseGlobale: number;
  montantTimbre: number;
  tauxRAS: number;
  netAPayer: number;
  montantPaye: number;
  resteAPayer: number;
  statut: string;
  notes?: string;
  lignes: LigneFactureClient[];
  dateCreation?: Date;
}

export interface CreateFactureClientRequest {
  codeFacture: string;
  dateFacture: Date;
  codeClient: string;
  tauxRemiseGlobale?: number;
  tauxRAS?: number;
  notes?: string;
  lignes: { codeProduit: string; quantite: number; prixUnitaireHT: number; tauxRemise?: number; }[];
}

export interface UpdateFactureClientRequest {
  dateFacture?: Date;
  codeClient?: string;
  tauxRemiseGlobale?: number;
  tauxRAS?: number;
  statut?: string;
  notes?: string;
  lignes?: { codeProduit: string; quantite: number; prixUnitaireHT: number; tauxRemise?: number; }[];
}

export interface FacturesClientFilter {
  search?: string;
  codeClient?: string;
  statut?: string;
  dateFrom?: Date;
  dateTo?: Date;
}
