# Modifications requises dans l'application pour le déploiement demo

Ces changements permettent à Blazor Server de fonctionner sous un sous-chemin
(`/demos/gestionscommercial`) au lieu de la racine `/`.

---

## 1. `Web_T4C_GestCom/Program.cs`

Ajouter **juste après** `var app = builder.Build();` et **avant** le bloc
`if (!app.Environment.IsDevelopment())` (ligne ~728) :

```csharp
// ── Subpath support (démo demo : /demos/gestionscommercial) ───────────────────
// ForwardedHeaders doit être le PREMIER middleware
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost
});

var appBasePath = builder.Configuration["APP_BASE_PATH"] ?? string.Empty;
if (!string.IsNullOrWhiteSpace(appBasePath))
{
    app.UsePathBase(appBasePath);
}
// ─────────────────────────────────────────────────────────────────────────────
```

### Résultat dans Program.cs (lignes ~727-743 après modification)

```csharp
// ── Subpath support ──────────────────────────────────────────────────────────
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost
});
var appBasePath = builder.Configuration["APP_BASE_PATH"] ?? string.Empty;
if (!string.IsNullOrWhiteSpace(appBasePath)) app.UsePathBase(appBasePath);
// ─────────────────────────────────────────────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

---

## 2. `Web_T4C_GestCom/Components/App.razor`

Remplacer la balise `<base>` statique par une valeur dynamique :

### Avant
```html
<base href="/" />
```

### Après
```razor
@inject IConfiguration Configuration

<base href="@((Configuration["APP_BASE_PATH"] ?? "").TrimEnd('/') + "/")" />
```

> En production demo, cela rendra `<base href="/demos/gestionscommercial/" />`
> En développement local (APP_BASE_PATH absent), cela rendra `<base href="/" />`

---

## 3. Aucune modification de `appsettings.json` requise

La variable `APP_BASE_PATH=/demos/gestionscommercial` est injectée par Docker
via `docker-compose.app.yml`. En local (sans Docker), elle est absente → `UsePathBase`
ne s'applique pas et l'app tourne normalement sur `/`.

---

## Pourquoi ces changements ?

| Problème sans ces changements | Solution |
|---|---|
| Blazor génère des URLs absolues depuis `/` → assets 404 | `UsePathBase` préfixe toutes les URLs internes |
| `<base href="/">` fait que le router Blazor ignore le préfixe de subpath | `<base href="/demos/gestionscommercial/">` corrige le routing côté client |
| nginx proxy renvoie l'IP réelle mais ASP.NET voit `127.0.0.1` | `UseForwardedHeaders` lit `X-Forwarded-For` / `X-Forwarded-Proto` |
| Les cookies auth ont `Path=/` au lieu de `/demos/gestionscommercial/` | `UsePathBase` + `UseForwardedHeaders` corrigent automatiquement le path des cookies |
