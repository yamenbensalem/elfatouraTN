# 🧪 Standards de tests — Backend & Blazor

---

## 🧭 Principes fondamentaux

* Tester le **comportement**, pas l’implémentation
* Un test = **une seule raison d’échouer**
* Tests **lisibles > DRY extrême**
* Tests rapides, déterministes, indépendants

---

## 🛠️ Stack

* Framework : xUnit
* Assertions : FluentAssertions
* Mocking : Moq ou NSubstitute
* Tests UI Blazor : bUnit (recommandé)

---

## 🏷️ Nommage

Format obligatoire :

```text
MethodName_WhenCondition_ExpectedResult
```

### Exemple

```csharp
CalculateTotal_WhenCartIsEmpty_ShouldReturnZero
```

---

## 🧱 Structure (AAA obligatoire)

```csharp
// Arrange
var service = new OrderService();

// Act
var result = service.CalculateTotal();

// Assert
result.Should().Be(0);
```

* Commentaires `Arrange / Act / Assert` obligatoires
* ❌ Pas de logique dans le Assert

---

## 🎯 Couverture

* Domain : **≥ 90%**
* Application : **≥ 90%**
* API / Controllers : **≥ 80%**
* Blazor Components : **≥ 70%**

⚠️ La couverture est un **indicateur**, pas un objectif aveugle

---

## 🧪 Types de tests

### Unit tests (priorité maximale)

* Domain
* Application (Handlers, Services)

### Integration tests

* Infrastructure (EF Core, DB)
* API (endpoints)

### UI tests

* Blazor (bUnit)

---

## 🔌 Gestion des dépendances

* TOUJOURS mocker :

  * services externes
  * API
  * DB (en unit tests)

* ❌ JAMAIS d’appel réseau réel en unit test

* ❌ JAMAIS de dépendance au système (horloge, fichiers)

---

## 🧠 Ce qu’il faut tester

### ✅ À tester

* Logique métier (Domain)
* Cas limites (edge cases)
* Gestion des erreurs
* Validation
* Mapping critique

### ❌ À NE PAS tester

* Getters/setters simples
* Framework (ASP.NET, EF)
* Détails internes (implémentation privée)

---

## ⚠️ Anti-patterns interdits

* ❌ Tests fragiles (cassent au refactor)
* ❌ Tests dépendants entre eux
* ❌ Tests longs (> 1s en unit)
* ❌ Assertions multiples non liées
* ❌ Setup complexe illisible

---

## 🧪 Données de test

* Utiliser des **builders** ou **fixtures**
* ❌ Pas de données magiques inline

```csharp
var order = OrderBuilder.WithDefaultValues().Build();
```

---

## ⏱️ Temps d’exécution

* Unit tests → < 100 ms
* Suite complète → rapide (CI friendly)

---

## 🗃️ Tests EF Core

### Unit tests

* Mock repository

### Integration tests

* Utiliser :

  * SQLite InMemory (préféré)
  * ou Testcontainers

* ❌ Éviter `InMemoryProvider` pour logique complexe (comportement différent)

---

## 🔄 Tests async

* Toujours tester async avec `await`
* ❌ JAMAIS `.Result` ou `.Wait()`

---

## 🧪 Tests d’erreurs

* Vérifier :

  * exceptions techniques
  * Result pattern (failures)

```csharp
result.IsSuccess.Should().BeFalse();
result.Error.Should().Be("Invalid input");
```

---

## 🧬 Tests Blazor (bUnit)

* Tester :

  * rendu
  * interactions utilisateur
  * événements

* ❌ Ne pas tester :

  * CSS
  * détails HTML inutiles

---

## 📊 Qualité des tests

Un bon test est :

* Lisible en < 10 secondes
* Stable
* Indépendant
* Expressif

---

## 🔁 Workflow

1. Écrire test (idéalement avant — TDD)
2. Implémenter
3. Refactor
4. Garder tests verts

---

## 🚨 Règles strictes

* Pas de test = pas de merge
* Bug → test obligatoire avant fix
* Toute logique métier → testée

---

## ⚖️ Philosophie

> Tests = filet de sécurité + documentation vivante
> ❌ Pas = charge inutile ou métrique vanity

---
