# 🧩 Standards Blazor — globs: ["**/*.razor", "**/*.razor.cs"]

---

## 🧭 Principes fondamentaux

* Un composant = **une seule responsabilité**
* Composants **petits, testables, réutilisables**
* Séparer UI (`.razor`) et logique (`.razor.cs`) dès que > 30 lignes
* Favoriser la **composition** plutôt que les composants “god object”

---

## 💉 Injection & dépendances

* Utiliser `@inject` ou `[Inject]`
* ❌ JAMAIS de state global statique
* ❌ JAMAIS de `new` pour les services
* Préférer des services **scopés** (Blazor Server) ou **singleton maîtrisé** (WASM)

---

## 🔄 Communication entre composants

* Parent → enfant : `[Parameter]`
* Enfant → parent : `EventCallback<T>` (**obligatoire**)
* ❌ Pas de couplage direct entre composants
* ❌ Pas d’accès à un composant via référence sauf cas exceptionnel

---

## 🔁 Rendu & performance

* `@key` **obligatoire** dans chaque `@foreach`
* Implémenter `ShouldRender()` si optimisation nécessaire
* Éviter les recalculs lourds dans le rendu
* Préférer des propriétés calculées **memoïsées** si coûteuses

---

## ⏱️ Cycle de vie

* `OnInitializedAsync()` → chargement initial
* `OnParametersSetAsync()` → réaction aux changements de paramètres
* `OnAfterRenderAsync()` → accès DOM (via JS uniquement)

### Interdictions

* ❌ Pas de logique métier dans `OnAfterRenderAsync`
* ❌ Pas d’appels async bloquants

---

## 🌐 JavaScript Interop

* Utiliser `IJSRuntime` **uniquement si indispensable**
* Encapsuler les appels JS dans des **services dédiés**
* ❌ JAMAIS d’appel JS inline dans les composants complexes
* Toujours gérer les erreurs JS

---

## 🧠 Gestion d’état

Choisir UNE stratégie :

### Option 1 — Simple

* StateContainer (scoped)
* Notifications via événements

### Option 2 — Complexe

* Fluxor (Redux-like)

### Règles

* ❌ Pas d’état dupliqué
* ❌ Pas de logique métier dans le state UI
* Toujours centraliser les mutations

---

## 📦 Data & API

* Appels API via **services dédiés**
* ❌ JAMAIS d’appel HTTP direct dans un composant
* Toujours gérer :

  * loading state
  * erreurs
  * retry si nécessaire

---

## 🔐 Sécurité

* Ne jamais faire confiance au client
* Toujours valider côté serveur
* Utiliser `[Authorize]` sur les pages sensibles
* Masquer les données sensibles côté UI

---

## 🧪 Testabilité

* Logique dans `.razor.cs` ou services testables
* Minimiser le code dans `.razor`
* Utiliser des interfaces pour les services

---

## 🎨 Bonnes pratiques UI

* Pas de logique complexe dans le markup
* Extraire les sous-composants rapidement
* Nommer clairement (`UserCard`, `OrderList`, etc.)
* Utiliser des `RenderFragment` pour la composition avancée

---

## 🚨 Règles strictes

* `async/await` obligatoire partout
* `CancellationToken` si appel long
* DTO uniquement (jamais d’entités Domain)
* Gestion des erreurs centralisée

---

## ⚠️ Anti-patterns interdits

* ❌ Composants > 300 lignes
* ❌ Logique métier dans UI
* ❌ Appels API multiples non contrôlés
* ❌ State partagé non maîtrisé
* ❌ JS interop abusif

---

## 🚀 Performance (Blazor Server spécifique)

* Minimiser les updates UI
* Grouper les changements d’état
* Attention aux appels fréquents (SignalR)
* Utiliser `Virtualize` pour les grandes listes

---

## 🧠 Philosophie

> Blazor = UI déclarative + état maîtrisé
> ❌ Pas = logique métier + effets de bord partout

---
