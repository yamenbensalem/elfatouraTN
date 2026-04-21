# 🔐 Règles de sécurité — Backend & Blazor

**globs**: ["src/**/*.cs", "src/**/*.razor"]

---

## 🧭 Principes fondamentaux

* Ne jamais faire confiance aux entrées utilisateur
* Sécurité **par défaut** (secure by default)
* Principe du **moindre privilège**
* Défense en profondeur (multi-couches)

---

## 🛡️ Validation & entrées

* Validation obligatoire sur **TOUS les endpoints**
* Utiliser FluentValidation côté Application
* ❌ Ne jamais faire confiance aux données du client (même authentifié)
* Sanitizer les entrées si affichées (XSS)

---

## 🔑 Authentification & autorisation

### JWT

* Durée de vie courte (5–15 min max)
* Refresh token :

  * rotation obligatoire
  * stockage sécurisé (HttpOnly + Secure cookie recommandé)
* ❌ Ne jamais stocker de JWT dans localStorage

### Autorisation

* Utiliser `[Authorize]` par défaut
* Préférer les **Policies** aux rôles
* ❌ Pas de logique d’autorisation inline

---

## 🌐 API & réseau

* HTTPS obligatoire :

  * `UseHttpsRedirection`
  * `UseHsts` (production uniquement)
* CORS :

  * ❌ Jamais de `AllowAnyOrigin()` en production
  * Autoriser uniquement les domaines nécessaires

---

## 🚦 Protection contre abus

* Rate limiting obligatoire (`AddRateLimiter`)
* Protection brute force (login endpoints)
* Throttling spécifique par endpoint sensible

---

## 🗃️ Accès aux données

* EF Core par défaut
* SQL brut autorisé **uniquement si paramétré**

```csharp
context.Users
    .FromSqlRaw("SELECT * FROM Users WHERE Id = @id", param);
```

* ❌ Interdiction de concaténer du SQL

---

## 🔐 Secrets & configuration

* ❌ JAMAIS de secrets dans `appsettings.json`

* Utiliser :

  * Variables d’environnement
  * User Secrets (dev)
  * Vault (Azure Key Vault, etc.)

* Rotation régulière des secrets

---

## 📜 Headers de sécurité

Ajouter middleware pour :

* `X-Content-Type-Options: nosniff`
* `X-Frame-Options: DENY`
* `X-XSS-Protection: 0`
* `Content-Security-Policy` (CSP strict)
* `Referrer-Policy: no-referrer`

---

## 🧠 Gestion des erreurs

* ❌ Ne jamais exposer les stack traces
* Middleware global de gestion des erreurs
* Messages utilisateurs génériques
* Logs détaillés côté serveur uniquement

---

## 📊 Logging & monitoring

* Logger :

  * tentatives de login
  * erreurs critiques
  * accès sensibles

* ❌ Ne jamais logger :

  * mots de passe
  * tokens
  * données sensibles

---

## 🧬 Blazor spécifique

### Blazor Server

* Attention aux circuits (SignalR)
* Vérifier authentification côté serveur (pas UI uniquement)

### Blazor WebAssembly

* ❌ Tout code côté client est visible
* ❌ Ne jamais stocker secrets
* Toujours valider côté API

---

## 🧾 Upload de fichiers

* Limiter taille max
* Vérifier type MIME
* Renommer fichiers (éviter collisions)
* Stocker hors wwwroot si sensible

---

## 🧪 Tests de sécurité

* Tests d’injection SQL
* Tests XSS
* Tests auth (accès non autorisé)
* Tests rate limiting

---

## 🚨 Anti-patterns interdits

* ❌ JWT long-lived sans refresh
* ❌ Secrets en dur
* ❌ `AllowAnyOrigin()` en prod
* ❌ Logs sensibles
* ❌ Confiance dans le client
* ❌ Validation uniquement côté frontend

---

## ⚡ Bonnes pratiques avancées

* CSRF protection (si cookies utilisés)
* Hashing robuste (ASP.NET Identity par défaut OK)
* Security headers automatisés
* Rotation des clés JWT
* Audit régulier des dépendances (`dotnet list package --vulnerable`)

---

## ⚖️ Philosophie

> Sécurité = couches + discipline + vigilance continue
> ❌ Pas = checklist ponctuelle

---
