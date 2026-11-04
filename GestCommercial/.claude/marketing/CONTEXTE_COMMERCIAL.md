# Contexte commercial GestCom — source de vérité des experts marketing/vente

> Lu par `expert-marketing-tn`, `expert-vente-tn` et `expert-offre-prix` avant toute réponse.
> Les faits produit/prix ci-dessous viennent du code (`Web_GestCom/appsettings.json`, `Home.razor`,
> `Web_GestCom/CLAUDE.md`) au 2026-10-31. **Le code reste la référence** : relire la section `Tarifs`
> de `appsettings.json` avant de citer un prix.

## Objectif

**5 premiers clients d'ici 30 jours** (échéance : fin novembre 2026).

Définition de « client » : **à confirmer avec Yamen**. Piste réaliste vu l'essai gratuit de 30 jours :
5 entreprises avec un compte activé (essai) à J+30, dont 2 ou 3 qui paient. Ne jamais présenter un
objectif de « 5 clients payants » comme acquis sans en parler.

## Le produit (faits vérifiés dans le code)

- **GestCom** : application web de gestion commerciale (Blazor Server), interface **en français**,
  montants en **dinar tunisien (TND / DT)**, multi-entreprise, accès par rôles
  (Admin / Manager / Employé).
- Modules : clients, fournisseurs, produits et stock ; ventes (devis, commandes, bons de livraison,
  factures, avoirs) ; achats (commandes, bons de réception, factures fournisseur) ; règlements ;
  rapports dont la **retenue à la source** ; journal d'activité.
- Spécificités tunisiennes **déjà gérées** : TVA, **timbre fiscal** (0,6 DT), **retenue à la source**
  (1,5 %).
- Offre **Desktop** (logiciel installé sur le PC du client, achat unique) en plus de l'abonnement web.
- Site/produit en production : `gestioncom.tijaraflow.fr`. Contact : `admin@tijaraflow.fr`.

### Ce qu'il ne faut PAS affirmer
- **Pas de conformité El Fatoora / TTN pour GestCom.** La facturation électronique TTN est un projet
  séparé (`TunisianEInvoice`) non intégré à GestCom. Aucun expert ne dit « conforme El Fatoora » sans
  que Yamen l'ait confirmé.
- Pas de paiement en ligne, pas d'application mobile native, pas d'API publique : ne rien promettre
  qui n'est pas dans le code.
- Aucune statistique de marché inventée (nombre de PME, part de marché, taux de conversion « typiques »).
  Toute estimation est marquée `[HYPOTHÈSE]` ou `[À VÉRIFIER]` avec la façon de la vérifier.

## Tarifs publics (tous **HT**, « TVA en sus » à écrire partout)

| Offre | Prix | Utilisateurs / contenu |
|---|---|---|
| Standard | 39 DT/mois ou 350 DT/an | 1 Admin, 1 Manager, 2 Employés |
| Pro | 69 DT/mois ou 590 DT/an | 2 Admins, 3 Managers, 10 Employés |
| Enterprise | sur devis | quotas sur mesure |
| Desktop Essentiel | 1 000 DT (achat unique) | 1 poste, garantie 2 mois, sans mises à jour |
| Desktop Sérénité | 1 400 DT + 350 DT/an de maintenance après la 1re année | 1 poste, 1 an de mises à jour et support |
| Desktop Équipe | 2 400 DT + 600 DT/an de maintenance après la 1re année | 3 postes, 1 an de mises à jour et support |
| Poste Desktop supplémentaire | 500 DT | |

- Essai gratuit **30 jours, sans carte bancaire** ; **2 h d'initiation incluses** (au-delà : facturée).
- Remise « Clients Fondateurs » **retirée le 2026-10-08** : ne pas la proposer, ne pas citer
  `FONDATEUR2026`. Le mécanisme de code promo existe mais est inactif ; toute remise = décision de Yamen.

## Parcours client actuel (contraintes réelles)

1. Le prospect remplit `/demande-abonnement` (ou Yamen crée la demande).
2. **Activation manuelle** par le SuperAdmin → création de l'entreprise + compte Admin → email avec
   identifiants.
3. **Paiement manuel** (aucune passerelle en ligne ; Konnect envisagé, non décidé). Après l'échéance :
   14 jours de grâce, puis accès bloqué jusqu'à enregistrement du paiement.

→ Cela convient à une vente **accompagnée** (démo, WhatsApp, virement/espèces). Les experts conçoivent
le parcours de vente autour de ça, pas autour d'un self-service qui n'existe pas.

## Cible (tout est `[HYPOTHÈSE]` jusqu'aux premiers appels)

- Commerces de gros / demi-gros, distributeurs, petites PME qui facturent sur Excel, Word ou papier.
- Boutiques et sociétés de services qui ont un stock ou des clients à crédit (relances, impayés).
- Prescripteurs possibles : experts-comptables et comptables indépendants.
- Facteurs d'achat probables : gain de temps de facturation, suivi des impayés, stock fiable, retenue à
  la source et timbre calculés, bon de livraison → facture en un clic.
- Freins probables : habitude d'Excel, méfiance envers le cloud et les données en ligne, peur de la
  migration, prix perçu, besoin d'être accompagné en français ou en darija.

## Cadence des 30 jours (squelette, à affiner avec les experts)

- **Semaine 1 — préparer** : cible et message, démo de 15 min, 1 page de présentation, liste de ~100
  prospects qualifiés, scripts, tableau de suivi.
- **Semaine 2 — contacter** : messages/appels, 8 à 12 démos.
- **Semaine 3 — essais** : accompagner les comptes ouverts (initiation incluse), relances.
- **Semaine 4 — convertir** : proposition, paiement, demande de recommandation.

Les ratios (contacts → réponses → démos → clients) sont des `[HYPOTHÈSES]` : mesurer dès la semaine 2
et recalculer.

## Règles communes à tous les experts

1. Répondre **en français** ; produire du **darija** (lettres latines ou arabes) seulement sur demande,
   en signalant qu'une relecture par un locuteur natif est conseillée.
2. **Rien n'est envoyé ni publié** par un expert : il rédige, Yamen valide et envoie. Aucun message de
   masse non sollicité ; prospection sur données **professionnelles publiques** uniquement, avec
   possibilité de refus. Obligations légales tunisiennes sur les données personnelles : `[À VÉRIFIER]`.
3. Un livrable = un fichier dans `.claude/marketing/livrables/` (créer le dossier si besoin), nommé
   `AAAA-MM-JJ_sujet.md`.
4. Chaque réponse finit par **« Prochaine action »** (une seule, concrète, datée) et, si utile, par
   **« Connecteurs / outils demandés »**.

## Protocole « connecteurs et outils »

Un expert qui gagnerait du temps avec un outil le demande dans sa réponse, dans cette forme :

```
## Connecteurs / outils demandés
- <outil> — pour <tâche précise> — gain estimé — alternative gratuite — coût/risque
```

Un sous-agent ne peut pas autoriser un connecteur lui-même : la conversation principale relaie à Yamen,
qui l'active (`/mcp`). Connecteurs pertinents ici :

| Besoin | Connecteur | Remarque |
|---|---|---|
| Trouver des prospects publics (Google Maps, pages Facebook de commerces) | **Apify** | Données publiques professionnelles uniquement ; coût à l'usage |
| Visuels, posts, brochure, carrousels | **Canva** | |
| Suivi des prospects et du pipeline | **HubSpot** (CRM gratuit) ou **Notion** | Sans connecteur : un tableau dans `.claude/marketing/livrables/` |
| Brouillons d'emails à des comptables ou prospects | **Gmail** (brouillons seulement) | Jamais d'envoi sans validation |
| Veille concurrents / trafic | **Similarweb**, **Ahrefs** | Payants, utiles surtout pour le SEO ; le marché tunisien passe plus par WhatsApp et Facebook |
| Doc partageable (offre, process) | **Claude Docs** | |

WhatsApp, premier canal de vente en Tunisie, n'a pas de connecteur ici : les experts rédigent les
messages, Yamen les envoie lui-même.
