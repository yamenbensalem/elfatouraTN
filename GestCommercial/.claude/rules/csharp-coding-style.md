# 🧑‍💻 Standards C# — Style de code & bonnes pratiques

---

## 🧭 Principes fondamentaux

* Code **lisible > clever code**
* Une méthode = **une responsabilité**
* Favoriser **immutabilité** et **pureté**
* Réduire les effets de bord
* Optimiser **après mesure**, jamais avant

---

## 🔤 Nommage

* `PascalCase` → classes, méthodes, propriétés
* `camelCase` → variables locales, paramètres
* `_camelCase` → champs privés

### Règles strictes

* ❌ JAMAIS d’abréviations (`obj`, `tmp`, `data`, `val`)
* ✅ Noms explicites : `customerOrder`, `isPaymentSuccessful`
* Booléens → préfixes : `is`, `has`, `can`

---

## 🧱 Types & structures

* DTOs → `record` (immutables)
* Domain → `class` riche (logique métier)
* Value Objects → `record` + validation dans constructeur

```csharp
public sealed record Email(string Value)
{
    public Email : this(Validate(Value)) {}

    private static string Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email invalide");
        return value;
    }
}
```

---

## 🔁 Contrôle de flux

* Préférer `switch expressions` aux `if/else` longs
* Utiliser des **guard clauses** pour réduire l’imbrication

```csharp
if (order is null)
    throw new ArgumentNullException(nameof(order));
```

---

## ⚠️ Gestion des erreurs

### Règle principale

* ❌ Pas d’exception pour la logique métier
* ✅ Utiliser un **Result pattern**

```csharp
public sealed record Result<T>(bool IsSuccess, T? Value, string? Error);
```

### Exceptions autorisées

* Erreurs techniques uniquement
* Toujours spécifiques (pas de `Exception` générique)

---

## 🔄 Async / Await

* Toujours `async/await`
* ❌ JAMAIS `.Result` ou `.Wait()`
* Toujours accepter un `CancellationToken`

```csharp
public async Task<Order> GetAsync(Guid id, CancellationToken ct)
```

---

## 🧬 Nullabilité

* `<Nullable>enable</Nullable>` obligatoire

### Règles

* ❌ Pas de `!` (null-forgiving) sauf cas justifié
* Toujours valider les entrées
* Utiliser `required` quand pertinent

---

## 🧪 LINQ

* Préférer LINQ pour la lisibilité
* ❌ Éviter LINQ dans les boucles critiques (perf)
* ❌ Pas de requêtes complexes inline → extraire

---

## 🧠 Méthodes

* < 20 lignes
* < 3 paramètres → sinon objet
* Nom explicite : `CalculateTotalPrice()`, pas `Calc()`

---

## 📦 Classes

* < 300 lignes
* Responsabilité unique
* Dépendances explicites (constructeur)

---

## 💉 Injection de dépendances

* Toujours via constructeur
* ❌ Pas de `new` pour services
* ❌ Pas de Service Locator

---

## 🧾 Documentation

* XML comments obligatoires sur **API publiques**

```csharp
/// <summary>
/// Calcule le total d’une commande.
/// </summary>
```

---

## ⚡ Performance & allocations

* Éviter allocations inutiles
* Utiliser `Span` / `ReadOnlySpan` si critique
* Éviter `ToList()` inutile
* Attention aux closures

---

## 🧵 Collections

* Préférer `IReadOnlyList<T>` / `IEnumerable<T>` en exposition
* ❌ Ne jamais exposer `List<T>` directement
* Utiliser `Array.Empty<T>()` plutôt que `new T[0]`

---

## 🔐 Sécurité

* Ne jamais faire confiance aux entrées
* Toujours valider
* Éviter concat SQL → utiliser paramètres

---

## 🚨 Anti-patterns interdits

* ❌ Méthodes longues et complexes
* ❌ Classes “God object”
* ❌ Exceptions pour flow normal
* ❌ Logique métier dans controllers/UI
* ❌ Null non maîtrisé

---

## 🧠 Bonnes pratiques avancées

* Pattern Matching (C# moderne)
* Records pour immutabilité
* `with` pour copies

```csharp
var updated = order with { Status = OrderStatus.Paid };
```

---

## 🧪 Tests

* Code testable par design
* Pas de dépendances statiques
* Pas de logique cachée

---

## ⚖️ Philosophie

> Code propre = lisible + maintenable + prévisible
> ❌ Pas = abstrait inutilement

---
