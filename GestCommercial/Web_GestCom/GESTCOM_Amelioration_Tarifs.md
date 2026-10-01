# FICHE AMÉLIORATION — Tarifs & Conversion GestCom
## Fichier de travail pour Claude Code

---

## CONTEXTE RAPIDE

Site : https://gestioncom.tijaraflow.fr
Stack : Blazor Server · .NET 8 · EF Core · SQL Server · multi-tenant

Problème identifié : la page tarifs actuelle crée 3 frictions
qui bloquent les clients tunisiens avant même de s'inscrire.
Cette fiche liste exactement ce qu'il faut changer et comment.

---

## CHANGEMENT 1 — Ajouter le paiement mensuel

### Pourquoi
Les PME tunisiennes ont une trésorerie serrée.
Payer 390 DT d'un coup bloque la décision.
Payer 45 DT/mois est psychologiquement beaucoup plus simple.

### Ce qu'il faut faire

**En base de données** — ajouter une colonne `billing_cycle` :
```sql
ALTER TABLE AbonnementPlans
ADD billing_cycle VARCHAR(10) NOT NULL DEFAULT 'annual'
-- Valeurs : 'monthly' | 'annual'

ALTER TABLE Abonnements
ADD billing_cycle VARCHAR(10) NOT NULL DEFAULT 'annual'
ADD next_billing_date DATE NULL
```

**Les nouveaux prix** :

| Plan     | Annuel (actuel) | Mensuel (nouveau) |
|----------|-----------------|-------------------|
| Standard | 390 DT/an       | 45 DT/mois        |
| Pro      | 690 DT/an       | 79 DT/mois        |
| Enterprise | Sur devis     | Sur devis         |

**Sur la page tarifs** — ajouter un toggle :
```
[ Annuel  |  Mensuel ]
           ↑
     bouton switch
```

Quand on bascule sur "Mensuel" :
- Afficher les prix mensuels
- Afficher un badge "Sans engagement"
- Masquer le badge "Le plus populaire" sur Pro
  (remplacer par "Flexible")

**Sur le formulaire de demande d'abonnement** :
- Ajouter un champ `billing_cycle` (radio : Annuel / Mensuel)
- Pré-remplir selon ce qui était sélectionné sur la page tarifs

---

## CHANGEMENT 2 — Allonger l'essai gratuit à 30 jours

### Pourquoi
14 jours c'est trop court pour le marché tunisien.
Le décideur reporte, part en déplacement, "voit ça la semaine prochaine".
30 jours laisse le temps à l'outil d'entrer dans les habitudes.

### Ce qu'il faut faire

**En base de données** :
```sql
-- Trouver la config du trial et changer la valeur
UPDATE AbonnementPlans
SET trial_days = 30
WHERE name = 'Standard' AND trial_days = 14
```

**Dans le code** — chercher partout où `14` ou `trial_days` apparaît :
```
grep -r "trial" --include="*.cs" -l
grep -r "14" --include="*.cs" -l   ← attention aux faux positifs
```

**Sur le site** — remplacer partout "14 jours" par "30 jours" :
```
Textes à modifier :
- Section hero : "Standard reste gratuit (14 jours...)"
- Page tarifs : partout où "14 jours" apparaît
- Email de bienvenue (si existant)
- Page /demande-abonnement : note explicative
```

**Dans la logique de conversion** :
```csharp
// Vérifier que la date d'expiration du trial est bien calculée
// à partir de trial_days (pas hardcodé à +14)
var trialExpiry = DateTime.UtcNow.AddDays(plan.TrialDays);
```

---

## CHANGEMENT 3 — Tarif de lancement "Clients Fondateurs"

### Pourquoi
Les 10 premiers clients ont besoin d'une raison d'agir maintenant.
Un tarif préférentiel permanent crée cette urgence sans dévaluer le produit.

### Ce qu'il faut faire

**Créer un code promo en base** :
```sql
INSERT INTO PromoCodes (
  code, label, discount_type, discount_value,
  max_uses, current_uses, expires_at, is_permanent_for_user
) VALUES (
  'FONDATEUR2026',
  'Client Fondateur — tarif préférentiel permanent',
  'percentage',     -- réduction en %
  35,               -- 35% de réduction → Standard = 253 DT/an ≈ 390 × 0.65
  10,               -- limité aux 10 premiers
  0,
  '2026-12-31',
  1                 -- la réduction s'applique au renouvellement aussi
)
```

**Sur la page tarifs** — ajouter une bannière discrète en haut :
```html
<!-- Bannière jaune/orange, peut être cachée après les 10 clients -->
<div class="banner-fondateur">
  🎁 Offre Clients Fondateurs — 35% de réduction permanente
  pour les 10 premières entreprises · Code : FONDATEUR2026
  · [X/10 places restantes]
</div>
```

**Sur le formulaire /demande-abonnement** :
- Ajouter un champ "Code promotionnel" (optionnel)
- Afficher la réduction calculée en temps réel
- Valider le code côté serveur avant soumission

**Logique serveur** :
```csharp
// Handler de validation du code promo
public async Task<Result<PromoCodeDto>> ValidatePromoCode(string code, string planName)
{
    var promo = await _db.PromoCodes
        .FirstOrDefaultAsync(p => p.Code == code
                               && p.CurrentUses < p.MaxUses
                               && p.ExpiresAt > DateTime.UtcNow);

    if (promo == null) return Result.Failure("Code invalide ou expiré");

    var discountedPrice = CalculateDiscount(planName, promo);
    return Result.Success(new PromoCodeDto { ... });
}
```

---

## CHANGEMENT 4 — Améliorer la note de bas de page (clarté)

### Texte actuel (peu clair)
> "Standard reste gratuit (14 jours pour prendre en main l'outil) ;
> Pro et Enterprise sont facturés dès l'activation (virement bancaire pour l'instant)"

### Nouveau texte suggéré
```
Comment ça marche :
① Vous choisissez un plan et remplissez le formulaire
② On vous contacte sous 24-48h pour activer votre compte
③ Standard : 30 jours gratuits, sans carte bancaire
   Pro / Enterprise : paiement par virement avant activation
④ RIB et facture pro forma envoyés par email
```

### Où modifier
```
Fichier : Components/Pages/Public/Tarifs.razor (ou équivalent)
Chercher : "virement bancaire pour l'instant"
```

---

## ORDRE DE DÉVELOPPEMENT RECOMMANDÉ

```
Étape 1 (1h) — Allonger l'essai à 30 jours
  → Modification base + textes du site
  → Le plus simple, impact immédiat

Étape 2 (2-3h) — Ajouter le toggle Annuel/Mensuel
  → Migration DB + logique abonnement + UI toggle
  → Plus de travail mais gros impact sur la conversion

Étape 3 (2h) — Code promo Fondateurs
  → Table PromoCodes + champ formulaire + validation serveur
  → À faire avant de commencer la prospection

Étape 4 (30min) — Clarifier la note de bas de page
  → Modification texte uniquement
```

---

## FICHIERS PROBABLEMENT CONCERNÉS

```
Chercher dans le projet :
- "14"         → durée essai
- "trial"      → logique essai
- "annual"     → cycle facturation
- "390"        → prix Standard
- "690"        → prix Pro
- "tarif"      → pages et composants tarifs
- "abonnement" → modèles et handlers abonnement
- "billing"    → logique facturation
```

---

## RÉSULTAT ATTENDU APRÈS CES CHANGEMENTS

| Avant | Après |
|-------|-------|
| 14 jours d'essai | 30 jours d'essai |
| Paiement annuel uniquement | Annuel OU mensuel (45 DT/mois) |
| Aucune urgence à s'inscrire | Code fondateur — 10 places limitées |
| Note de bas de page confuse | Étapes claires numérotées |

Ces 4 changements ne touchent pas le cœur fonctionnel de l'app.
Ce sont des modifications de surface + configuration.
Estimation totale : une journée de développement.
