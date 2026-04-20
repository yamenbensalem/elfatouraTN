# 🔐 Authentification externe — Backend (.NET)

---

## 🧭 Principe fondamental

➡️ **Le backend est l’unique autorité de sécurité**
➡️ Le frontend ne manipule JAMAIS les tokens OAuth

---

## 🌐 Providers supportés

* Google (OpenID Connect)
* Facebook (OAuth 2.0)

---

## 🔄 Flow obligatoire

### 1. Initiation login

```http
GET /api/auth/external-login?provider=google
```

* Redirige vers le provider via ASP.NET Identity

---

### 2. Callback

```http
GET /api/auth/external-callback
```

### Étapes obligatoires :

1. Vérifier identité externe (`AuthenticateAsync`)
2. Extraire :

   * Email
   * Provider ID
3. Vérifier si utilisateur existe
4. Sinon → créer utilisateur
5. Lier login externe
6. Générer :

   * JWT (court)
   * Refresh token

---

## 🔐 Configuration ASP.NET

### Exemple

```csharp
builder.Services
    .AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = config["Auth:Google:ClientId"];
        options.ClientSecret = config["Auth:Google:ClientSecret"];
    })
    .AddFacebook(options =>
    {
        options.AppId = config["Auth:Facebook:AppId"];
        options.AppSecret = config["Auth:Facebook:AppSecret"];
    });
```

---

## 🔑 Gestion des utilisateurs

* Identifier un utilisateur par :

  * Email (unique)
  * Provider + ProviderId

### Cas à gérer

* Utilisateur existe déjà avec email → linker compte
* Multi-provider autorisé (Google + Facebook)

---

## 🔄 Tokens

### JWT

* Durée courte (5–15 min)

### Refresh token

* Stocké en base
* Rotation obligatoire
* Révocation possible

---

## 🍪 Stockage recommandé

* Cookies sécurisés :

  * HttpOnly
  * Secure
  * SameSite=Strict

* ❌ Pas de retour token brut dans URL

---

## 🛡️ Validation sécurité

* Vérifier :

  * signature du token externe
  * audience
  * issuer

* ❌ Ne jamais faire confiance aux données brutes du provider

---

## 🔐 Secrets

* Stocker dans :

  * variables d’environnement
  * vault sécurisé

* ❌ JAMAIS dans `appsettings.json`

---

## 🚨 Règles strictes

* ❌ Pas de logique OAuth dans Angular
* ❌ Pas de token externe stocké côté client
* ❌ Pas de login sans validation backend
* ❌ Pas de JWT long-lived

---

## 🧠 Gestion des erreurs

* Refus utilisateur → retour contrôlé
* Provider indisponible → fallback propre
* Email manquant → bloquer création

---

## 📊 Logging

Logger :

* succès login externe
* échecs
* nouveaux comptes liés

❌ Ne jamais logger :

* tokens
* secrets

---

## 🔄 Linking / unlinking

* Permettre :

  * ajouter un provider
  * supprimer un provider

* ❗ Toujours garder au moins un moyen de login

---

## 🧪 Tests

Tester :

* login complet
* linking compte
* refresh token rotation
* échecs provider

---

## ⚡ Bonnes pratiques avancées

* Ajouter MFA après login externe
* Détection d’anomalies (nouvel appareil)
* Limiter tentatives

---

## ⚠️ Anti-patterns interdits

* ❌ Faire confiance au frontend
* ❌ Stocker tokens externes
* ❌ Retourner tokens dans URL
* ❌ Ignorer validation issuer/audience

---

## ⚖️ Philosophie

> OAuth backend = sécurité critique
> ❌ Pas = simple “plugin login”

---
