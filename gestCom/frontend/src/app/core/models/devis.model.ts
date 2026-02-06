export interface LigneDevis {
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

export interface Devis {
  codeDevis: string;
  dateDevis: Date;
  codeClient: string;
  raisonSocialeClient?: string;
  objet?: string;
  montantHT: number;
  montantTVA: number;
  montantTTC: number;
  tauxRemiseGlobale: number;
  montantTimbre: number;
  netAPayer: number;
  statut: string;
  validite?: number;
  notes?: string;
  lignes: LigneDevis[];
  dateCreation?: Date;
}

export interface CreateDevisRequest {
  codeDevis: string;
  dateDevis: Date;
  codeClient: string;
  objet?: string;
  tauxRemiseGlobale?: number;
  validite?: number;
  notes?: string;
  lignes: { codeProduit: string; quantite: number; prixUnitaireHT: number; tauxRemise?: number; }[];
}

export interface UpdateDevisRequest {
  dateDevis?: Date;
  codeClient?: string;
  objet?: string;
  tauxRemiseGlobale?: number;
  validite?: number;
  statut?: string;
  notes?: string;
  lignes?: { codeProduit: string; quantite: number; prixUnitaireHT: number; tauxRemise?: number; }[];
}

export interface DevisFilter {
  search?: string;
  codeClient?: string;
  statut?: string;
  dateFrom?: Date;
  dateTo?: Date;
}
