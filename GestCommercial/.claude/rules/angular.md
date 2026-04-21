# 🅰️ Standards Angular — (Angular 17+)

**globs**: ["**/*.ts", "**/*.html"]

---

## 🧭 Principes fondamentaux

* Architecture **feature-based** (pas par type technique)
* Composants **petits, découplés, testables**
* Flux de données **unidirectionnel**
* UI = projection d’un état (pas de logique métier)

---

## 🧱 Architecture

### Organisation

```text
/features/
 └── orders/
      ├── pages/
      ├── components/
      ├── services/
      ├── store/
      └── models/
```

* Une feature = **autonome**
* ❌ Pas de dossier global “shared” fourre-tout

---

## 🧩 Composants

* Standalone components **obligatoire**
* Change detection : `OnPush` **partout**

```ts
@Component({
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
```

---

## 🧠 Smart / Dumb components

### Smart (containers)

* Gèrent état, appels API
* Connectés au store
* Peu nombreux

### Dumb (presentational)

* Input / Output uniquement
* Pas de logique métier
* Réutilisables

---

## ⚡ Réactivité (Signals)

* Utiliser **Signals par défaut**

```ts
const count = signal(0);
```

* `computed()` pour dérivations
* `effect()` pour side-effects

### Règles

* ❌ Pas de mélange inutile avec RxJS
* ❌ Pas de logique métier dans `effect`

---

## 🔄 RxJS (si nécessaire)

* Utiliser uniquement pour :

  * streams complexes
  * websockets
  * événements async avancés

### Bonnes pratiques

* `takeUntilDestroyed()` obligatoire
* ❌ Pas de `subscribe()` manuel sans cleanup

---

## 💉 Injection de dépendances

* Utiliser `inject()` (Angular moderne)

```ts
const service = inject(OrderService);
```

* ❌ Pas de constructor injection (sauf cas spécifique)
* ❌ Pas de logique dans les services globaux non maîtrisés

---

## 🌐 HTTP & API

* Appels HTTP via services dédiés
* ❌ Pas d’appel HTTP dans les composants

### Interceptors obligatoires

* Auth (JWT)
* Gestion des erreurs
* Logging éventuel

---

## 🗂️ Routing

* Lazy loading **par feature obligatoire**

```ts
{
  path: 'orders',
  loadChildren: () => import('./features/orders/routes')
}
```

* Guards pour auth
* ❌ Pas de routes globales massives

---

## 🧾 Templates HTML

* Pas de logique complexe dans le template
* ❌ Pas de fonctions appelées dans le HTML

```html
<!-- ❌ interdit -->
<div>{{ calculateTotal() }}</div>
```

* Utiliser :

  * `@if`
  * `@for`
  * `trackBy` obligatoire

---

## 🧠 Gestion d’état

### Option simple

* Signals + services

### Option avancée

* NgRx SignalStore / Store

### Règles

* Source de vérité unique
* ❌ Pas de duplication d’état
* ❌ Pas de mutation directe

---

## 🔐 Sécurité

* Échapper automatiquement le HTML (Angular le fait par défaut)
* ❌ Jamais de `innerHTML` sans sanitation
* Utiliser `DomSanitizer` avec prudence

---

## 🧪 Tests

* Unit tests pour services et composants
* TestBed minimal
* ❌ Pas de tests couplés au DOM complexe inutile

---

## 🚀 Performance

* OnPush obligatoire
* `trackBy` dans toutes les boucles
* Lazy loading partout
* Éviter recalculs inutiles

---

## 📦 Modèles & types

* Interfaces ou types pour DTOs
* ❌ Pas de `any`
* Types stricts obligatoires

---

## 🚨 Anti-patterns interdits

* ❌ Composants > 300 lignes
* ❌ Logique métier dans composants
* ❌ Subscriptions non nettoyées
* ❌ State global non contrôlé
* ❌ Services “god object”

---

## 🧠 Bonnes pratiques avancées

* Prefer `readonly` partout
* Favoriser fonctions pures
* Centraliser les constantes
* Feature isolation stricte

---

## 🔁 Workflow

1. Créer feature
2. Définir modèle + state
3. Implémenter services
4. Créer smart component
5. Ajouter dumb components
6. Tests

---

## ⚖️ Philosophie

> Angular moderne = réactivité + découplage + performance
> ❌ Pas = RxJS partout + composants lourds

---
