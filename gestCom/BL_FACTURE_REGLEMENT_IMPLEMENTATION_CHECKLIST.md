# Checklist Stricte: BL -> Facture Client -> Reglement

## Objectif
Mettre en production un flux complet et robuste de vente: creation BL, conversion BL vers facture client, puis encaissement (reglement) avec invariants metier et coherence comptable/stock.

## Definition Of Done
- Le flux BL -> Facture -> Reglement fonctionne en API sans endpoint placeholder pour les operations critiques.
- Les invariants metier sont verifies par validateurs + handlers.
- Les ecritures sont transactionnelles sur les operations multi-entites.
- Une build `dotnet build` passe sans erreur.

## 1) Gate d'entree
- [ ] Verifier que les contrats DTO/Command sont figes (pas de breaking change non valide).
- [ ] Verifier que `CodeEntreprise` provient du contexte utilisateur (pas de confiance implicite dans le payload).
- [ ] Verifier que les mappings AutoMapper existent pour toutes les reponses du flux.

## 2) BL - Creation
- [x] Ajouter un validateur `CreateBonLivraisonCommandValidator`.
- [ ] Bloquer BL vide (0 ligne) au niveau validation.
- [ ] Verifier stock disponible pour chaque ligne avant decrement.
- [ ] Transaction obligatoire: decrement stock + creation BL + MAJ commande.
- [ ] Tests: creation BL ok, creation BL sans lignes, creation BL stock insuffisant.

## 3) BL -> Facture Client
- [x] Ajouter un validateur `ConvertBLToFactureCommandValidator`.
- [x] Utiliser `INumeroService` pour la numerotation facture (pas de numerotation manuelle).
- [x] Transaction obligatoire: creation facture + marquage BL factures.
- [x] Marquer chaque BL converti (`Statut`, `Facture`, `NumeroFacture`).
- [ ] Refuser conversion si BL deja facture (flags + lien facture existant).
- [ ] Calculer proprement HT/TVA/FODEC/remises + APayer/NetAPayer/MontantRestant.
- [ ] Tests: conversion BL unique, conversion multi-BL meme client, refus clients differents, refus BL deja facture.

## 4) Reglement Facture Client
- [x] Ajouter un validateur `CreateReglementFactureCommandValidator`.
- [x] Ajouter mapping AutoMapper `ReglementFactureMappingProfile`.
- [x] Transaction obligatoire: creation reglement + MAJ facture.
- [x] MAJ facture: `MontantRegle`, `MontantRestant`, `Statut`.
- [x] Verifier `Montant > 0` et `Montant <= ResteARegler`.
- [ ] Tests: reglement total, reglement partiel, sur-reglement refuse, reglement sur facture deja payee.

## 5) API Read/Query (a finaliser)
- [ ] Implementer `GetAll` reglements (filtres client/facture/dates).
- [ ] Implementer `GetByNumero` reglement.
- [ ] Implementer `GetByFacture` reglements.
- [ ] Implementer resume client reglements.
- [ ] Remplacer endpoints TODO des controlleurs par queries MediatR.

## 6) Validation transversale
- [ ] Build `dotnet build` sans erreurs.
- [ ] Executer tests unitaires/integration du flux.
- [ ] Verifier comportement multi-tenant sur toutes les requetes.
- [ ] Verifier idempotence partielle (pas de double conversion BL, pas de double reglement involontaire).

## 7) Rollout
- [ ] Documenter payloads exemples BL -> Facture -> Reglement.
- [ ] Ajouter checks de monitoring (erreurs conversion/reglement, collisions numerotation).
- [ ] Plan de rollback applicatif en cas de regression flux vente.

## Avancement Session Courante
- [x] Checklist stricte creee.
- [x] Premier slice applique (validateurs + mapping + transactions + numerotation + MAJ flags BL/facture).
- [ ] Slice suivant recommande: queries reglements + branchement complet des endpoints TODO.