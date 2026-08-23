# Déploiement client — T4C GestCom Desktop

Référence unique pour livrer l'application à un nouveau client : empaquetage, obfuscation,
licence verrouillée sur sa machine, et mise en place de la base de données. Ce document couvre
le cas d'un déploiement où l'application **et** SQL Server tournent sur la même machine chez le
client (poste unique).

**À lire d'abord** : aucune des deux protections ci-dessous (obfuscation, hardening SQL) n'est
absolue. Du code managé .NET reste toujours décompilable avec assez de temps — on ne fait que
relever la barre. Et si le client a un accès admin/physique à sa propre machine, il pourra
toujours atteindre sa propre base SQL Server avec les bons droits Windows — on limite les dégâts
possibles, on ne rend pas la base inatteignable.

## Vue d'ensemble du flux

```
Yamen                              Client
------------------------------     ------------------------------
1. build-client-package.ps1
   (régénère d'abord Database/T4C_GestCom_Template.bak
    via prepare-template-database.ps1 si le schéma a changé)
   -> App/ + LicenseTool/ + Database/
2. Envoie le package au client --> 3. Lance LicenseTool\collect
                                       -> fingerprint.json
                             <-- 4. Renvoie fingerprint.json à Yamen
5. LicenseTool issue
   (avec la clé privée)
   -> <client>.lic
6. Envoie le .lic au client   --> 7. Place license.lic au bon endroit
                                   8. Installe la base (Database/restore-database.ps1
                                      puis create-app-login.sql)
                                   9. Premier lancement de l'app
```

## Outils à installer (une seule fois, sur ta machine)

| Outil | Commande | Sert à |
|---|---|---|
| .NET 8 SDK | (déjà installé si tu as pu builder le repo) | Compiler tous les projets |
| PowerShell 7+ | `winget install Microsoft.PowerShell` si absent | Exécuter les scripts `deploy/*.ps1` |
| Obfuscar | `dotnet tool install --global obfuscar.globaltool` | Obfusque `T4C_GestCom_Desktop.dll` avant livraison |

## Outils à installer chez le client (avant le premier lancement)

| Outil | Pourquoi | Remarque |
|---|---|---|
| **.NET 8 Desktop Runtime (x64)** | (https://dotnet.microsoft.com/en-us/download/dotnet/8.0 -- windows x64)Fait tourner `T4C_GestCom_Desktop.exe` et `T4C_GestCom_LicenseGenerator.exe` — le build est framework-dependent, pas autonome | Runtime seul, pas le SDK. S'il manque, Windows affiche lui-même un lien de téléchargement Microsoft au lancement — pas besoin de le pré-télécharger |
| **SQL Server Express** | Héberge la base en local sur le même poste (choix d'archi retenu) (https://www.microsoft.com/fr-fr/download/details.aspx?id=104781) | Édition **Express** (gratuite, jusqu'à 10 Go/base) — jamais Developer Edition pour un client réel, licence non autorisée en production |
| **SSMS** (ou `sqlcmd`, fourni avec SQL Server) | Restaurer la base initiale et lancer `create-app-login.sql` | Optionnel *sur le poste client* si tu fais cette étape toi-même à distance (RDP/AnyDesk) plutôt que sur place |

SQL Server Configuration Manager (pour désactiver TCP/IP, voir section 8.3 ci-dessous) est fourni
avec l'installation de SQL Server — rien à installer en plus pour ça.

## Ce qu'on livre au client, et pourquoi

Un dossier avec trois sous-dossiers, produit par `build-client-package.ps1` :

- **`App/`** — l'application, en Release, sans PDB, avec `T4C_GestCom_Desktop.dll` obfusqué
  (renommage des membres privés/champs — voir section 1 pour le détail et les limites : la
  logique métier dans `Web_T4C_GestCom.Core.dll` n'est volontairement **pas** obfusquée).
- **`LicenseTool/`** — `T4C_GestCom_LicenseGenerator.exe`, utilisé par le client (`collect`, sans
  risque) puis par toi (`issue`, avec ta clé privée qui ne quitte jamais ta machine).
- **`Database/`** — `T4C_GestCom_Template.bak` (image initiale : schéma + données de référence +
  admin par défaut), `restore-database.ps1` (la restaure chez le client) et `create-app-login.sql`
  (crée ensuite un compte Windows applicatif à droits minimaux `db_datareader`/`db_datawriter`, pas
  `db_owner`, au lieu de faire tourner l'app en admin local).

Le `.bak` n'est pas réimplémenté à la main : `sql/prepare-template-database.ps1` (sur ta machine)
lance une fois `Web_T4C_GestCom` contre une base vide — son propre `Program.cs` fait tout le travail
réel (EnsureCreated + migrations SQL incrémentales + seed) — puis sauvegarde le résultat avec
`BACKUP DATABASE` natif SQL Server. Zéro risque de réimplémenter ces migrations à côté. À relancer
à chaque changement de schéma dans `Web_T4C_GestCom` (sinon le `.bak` dérive silencieusement).

## 0. Prérequis (une seule fois sur la machine de Yamen)

- `dotnet tool install --global obfuscar.globaltool` (voir section 1).
- La clé privée existe à `T4C_GestCom_Desktop/deploy/keys/t4c-license-private.pem`. Si ce fichier
  n'existe pas (nouvelle machine de dev, restauration après incident), voir la section
  **Sauvegarde de la clé privée** en bas de ce document — **sans elle, aucune licence ne peut être
  émise**, quel que soit le code source disponible.

## 1. Build + obfuscation

Si le schéma de `Web_T4C_GestCom` a changé depuis la dernière livraison, régénère d'abord l'image de
base initiale (sinon `build-client-package.ps1` refuse de continuer si elle n'existe pas encore) :

```powershell
cd T4C_GestCom_Desktop/deploy
./sql/prepare-template-database.ps1
# -> deploy/db-template/T4C_GestCom_Template.bak
```

Puis construis le package complet (obfuscation + outil de licence + scripts SQL + image de base) :

```powershell
./build-client-package.ps1 -ClientName "Nom-Du-Client"
# -> deploy/client-package/Nom-Du-Client/{App,LicenseTool,Database}/
```

En interne, ce script appelle `publish-and-obfuscate.ps1` pour la partie `App/`, qui :

1. `dotnet publish -c Release` — sans PDB (voir `<DebugType>none</DebugType>` dans les `.csproj`,
   sinon les chemins de fichiers source et numéros de ligne partiraient avec l'exe).
2. Obfusque **uniquement** `T4C_GestCom_Desktop.dll` avec Obfuscar (voir le commentaire en tête de
   `deploy/Obfuscar.xml` pour le détail complet, notamment **pourquoi
   `Web_T4C_GestCom.Core.dll` (entités + services) n'est PAS obfusqué** — deux essais réels ont
   cassé l'app : les clés composites d'EF Core (`HasKey(ur => new {...})`) et le pruning des
   `DbSet<T>` par Obfuscar). La logique métier (calculs, permissions, numérotation) reste donc
   lisible dans un décompilateur pour cette première version.
3. Archive `Mapping.txt` (la table nom original → nom obfusqué) dans `deploy/mapping-archive/`,
   **hors du dossier livré au client**. Ce fichier est la clé qui annule toute la protection — à
   garder privé, utile seulement si un crash chez le client doit être débogué depuis un build
   obfusqué.

Le script `build-client-package.ps1` vérifie lui-même qu'aucun fichier de clé privée ne s'est
retrouvé dans le package — s'il détecte quoi que ce soit, il efface le package et lève une erreur.

### Vérification obligatoire avant handoff

Le script obfusque avec un scope volontairement réduit (voir ci-dessus), validé par un test
scripté (lancement + requête EF Core réelle), **pas** par un passage manuel sur les 14 écrans.
Avant de livrer : lancer `App\T4C_GestCom_Desktop.exe` (dans le package produit) contre une vraie
base, se connecter, ouvrir un Devis ou une Facture, tester le combo Produit dans la grille de
lignes (c'est le seul endroit où l'obfuscation a dû exclure explicitement un type —
`ProduitOption`), enregistrer un document.

### Zipper et envoyer

```powershell
Compress-Archive -Path .\client-package\Nom-Du-Client\* -DestinationPath .\Nom-Du-Client.zip
```

Envoyez uniquement le contenu du zip (`App/`, `LicenseTool/`, `Database/`). Rien dedans n'est
sensible : l'app n'a pas encore de licence installée, et `LicenseTool` en mode `collect` ne
nécessite pas la clé privée.

## 2. Chez le client — installer les prérequis

Avant tout le reste : **.NET 8 Desktop Runtime (x64)** + **SQL Server Express** (voir tableau
ci-dessus).

## 3. Le client collecte l'empreinte de sa machine

Sur la machine du client, sans droits particuliers requis :

```
cd LicenseTool
T4C_GestCom_LicenseGenerator.exe collect
```

Produit `fingerprint.json` dans le dossier courant (MachineGuid, adresse MAC, nom de machine, ID
CPU, plus le hash calculé). Le client vous renvoie ce fichier — aucune information sensible dedans.

## 4. Yamen reçoit `fingerprint.json`

Récupérez le fichier envoyé par le client, notez-le quelque part avec le nom du client (utile en
cas de renouvellement futur).

## 5. Yamen émet la licence

Sur votre propre machine, avec la clé privée :

```powershell
cd T4C_GestCom_Desktop/deploy
.\client-package\Nom-Du-Client\LicenseTool\T4C_GestCom_LicenseGenerator.exe issue `
    --fingerprint "chemin\vers\fingerprint.json" `
    --client "Nom Du Client SARL" `
    --key "keys\t4c-license-private.pem" `
    --out "Nom-Du-Client.lic"
```

Ajoutez `--expires yyyy-MM-dd` si la licence doit expirer (par défaut : perpétuelle). L'outil
confirme le `LicenseId` généré et le nom de fichier produit — gardez une trace de ces licences
émises (par exemple un tableau simple client / date / LicenseId).

## 6. Envoyer le fichier `.lic` au client

Envoyez uniquement `Nom-Du-Client.lic` — ce fichier est signé et ne fonctionne que sur la machine
dont l'empreinte a été collectée à l'étape 3 ; il ne présente donc pas de risque particulier à
transiter par email.

## 7. Le client installe la licence

Deux emplacements possibles, dans cet ordre de priorité :

1. **Recommandé** : `%ProgramData%\T4C_GestCom\license.lic`
   (généralement `C:\ProgramData\T4C_GestCom\license.lic` — créer le dossier `T4C_GestCom` s'il
   n'existe pas).
2. **Dépannage rapide** : à côté de `T4C_GestCom_Desktop.exe`, sous le nom `license.lic`. Pratique
   pour un premier test avant d'avoir mis en place le dossier ProgramData définitif.

Renommez le fichier reçu (`Nom-Du-Client.lic`) en `license.lic` à l'emplacement choisi.

## 8. Base de données — SQL Server local sur le poste client

### 8.1 Restaurer la base initiale

Avant de créer le compte applicatif (ci-dessous), la base `T4C_GestCom` doit exister. Le package
client contient dans `Database/` un `T4C_GestCom_Template.bak` — produit sur la machine de Yamen par
`deploy/sql/prepare-template-database.ps1` (voir section 1) — et un `restore-database.ps1` qui le
restaure. Sur l'instance SQL Server du client, avec un compte Windows ayant le rôle **sysadmin**
sur cette instance (voir l'encadré ci-dessous si tu ne sais pas lequel) :

```powershell
cd Database
./restore-database.ps1
```

**Piège fréquent — nom d'instance** : `-Server` vaut `.` par défaut (instance par défaut), mais
**SQL Server Express s'installe presque toujours en instance nommée `SQLEXPRESS`**, jamais en
instance par défaut. Si tu obtiens une erreur `Fournisseur de canaux nommés : Impossible d'ouvrir
une connexion à SQL Server [2]`, vérifie d'abord le nom réel de l'instance :

```powershell
Get-Service | Where-Object { $_.Name -like "MSSQL*" }
# Service nommé "MSSQL$SQLEXPRESS" -> l'instance s'appelle ".\SQLEXPRESS"
```

Puis relance avec le bon nom d'instance :

```powershell
./restore-database.ps1 -Server ".\SQLEXPRESS"
```

Le même `-Server` doit être répété pour `create-app-login.sql` (§8.2) et reporté dans
`appsettings.json` (§8.3, point 4) — sinon l'app ne trouvera pas la base au premier lancement.

Le script découvre lui-même les noms logiques des fichiers du `.bak` (`RESTORE FILELISTONLY`),
résout le dossier data/log par défaut de l'instance cible, et refuse d'écraser une base `T4C_GestCom`
déjà existante sans `-Force` (voir l'en-tête du script pour les paramètres `-BackupFile`,
`-TargetDatabaseName`, `-Server`).

### 8.2 Compte applicatif à droits minimaux

Ne pas faire tourner l'app avec le compte Windows interactif du client (souvent admin local). Sur
la machine du client, en admin :

```
net user svc_t4cgestcom "<mot de passe fort généré>" /add
```

Puis exécuter `deploy/sql/create-app-login.sql` dans SSMS (ou `sqlcmd`) connecté en admin sur
l'instance SQL Server du client — utilise le même nom d'instance qu'à l'étape 8.1 (`.` ou
`.\SQLEXPRESS` selon le cas) :

```powershell
sqlcmd -S .\SQLEXPRESS -E -C -i create-app-login.sql
```

Ça crée un login Windows dédié, scoped à `db_datareader` + `db_datawriter` sur `T4C_GestCom`
uniquement — pas `db_owner`, pas `sysadmin`. Si la machine est compromise, ce compte ne peut
lire/écrire que les tables de l'app, pas modifier le schéma ni toucher aux autres bases.

Faire tourner `T4C_GestCom_Desktop.exe` sous ce compte (tâche planifiée configurée "Exécuter en
tant que" `svc_t4cgestcom`, ou raccourci `runas /user:.\svc_t4cgestcom`). La chaîne de connexion
reste `Trusted_Connection=True` — aucun mot de passe n'apparaît jamais dans `appsettings.json`.

### 8.3 Désactiver l'accès réseau distant à SQL Server

Puisque l'app et la base sont sur la même machine, aucune connexion réseau entrante n'est
nécessaire — seuls les canaux locaux (Shared Memory / Named Pipes) suffisent. Sur la machine du
client, via **SQL Server Configuration Manager** :

1. `SQL Server Network Configuration` → `Protocols for <instance>` → clic droit sur **TCP/IP** →
   **Disable**.
2. Vérifier que **Shared Memory** reste **Enabled**.
3. Redémarrer le service SQL Server (`services.msc` → SQL Server (`<instance>`) → Redémarrer) —
   toute connexion active tombe, à faire hors heures d'utilisation.
4. Ajuster `T4C_GestCom_Desktop/appsettings.json` : remplacer `Server=<NomMachine>` par
   `Server=.` ou `Server=.\<NomInstance>` (connexion locale, ne dépend plus de la résolution
   réseau du nom de machine).

Ceci empêche quiconque sur le même réseau (même avec de bons identifiants Windows) d'atteindre la
base à distance — il faut être physiquement/en RDP sur la machine elle-même.

### 8.4 Ce qui reste hors de portée

Un utilisateur avec un accès admin Windows sur la machine du client pourra toujours, en théorie,
retrouver le mot de passe du compte `svc_t4cgestcom`, désactiver le pare-feu local, ou réactiver
TCP/IP. Ces mesures visent un attaquant opportuniste ou un utilisateur non-admin de la machine —
pas un administrateur système déterminé de sa propre machine. C'est une limite inhérente à tout
déploiement où le client héberge sa propre infrastructure.

## 9. Premier lancement

Lancer `App\T4C_GestCom_Desktop.exe`. Si la licence est valide, l'écran de connexion s'affiche
normalement. Si un message d'erreur de licence apparaît, vérifier :

- Le fichier est bien nommé `license.lic` (pas `Nom-Du-Client.lic`).
- Il est bien à l'un des deux emplacements listés à l'étape 7.
- L'empreinte n'a pas changé depuis la collecte (carte réseau remplacée, machine réinstallée) —
  dans ce cas, refaire `collect` et réémettre une licence.

L'application revalide la licence toutes les 15 minutes pendant la session ; si le fichier est
supprimé ou modifié en cours d'utilisation, l'app affiche le message d'erreur et se ferme.

### Journal de l'application (logs)

Toute erreur (démarrage, licence, connexion base, authentification) est journalisée dans :

```
%ProgramData%\T4C_GestCom\logs\app-<date>.log
```

Si ce dossier n'est pas inscriptible par le compte Windows qui exécute l'app (cas du compte
applicatif à droits minimaux, voir §8.2), l'app bascule automatiquement sur
`%LOCALAPPDATA%\T4C_GestCom\logs\` — regarder les deux emplacements si le premier est vide. Un
fichier par jour, conservé 14 jours. C'est le premier réflexe en cas de comportement inattendu
chez un client (écran qui ne s'affiche pas, erreur de connexion, etc.) — avant toute manipulation
plus lourde (Journal d'événements Windows, débogage à distance).

## Ce que la licence protège réellement

Verrouillage machine (MachineGuid + MAC + nom de PC + ID CPU) signé RSA-3072 — sans ta clé privée,
personne ne peut fabriquer un `.lic` valide même en décompilant l'app. Vérifié au démarrage (avant
tout accès à la base) et revérifié toutes les 15 minutes en session. Comme pour l'obfuscation :
ça dissuade la copie casuelle vers une deuxième machine, ce n'est pas une protection absolue contre
quelqu'un de déterminé avec un décompilateur — voir l'avertissement en tête de ce document.

## Sauvegarde de la clé privée

`T4C_GestCom_Desktop/deploy/keys/t4c-license-private.pem` est **l'artefact le plus critique de
tout ce système** : sans elle, aucune nouvelle licence ne peut jamais être émise pour un client déjà
déployé (la clé publique embarquée dans les apps déjà livrées ne peut être validée que par cette
clé privée précise). Elle est volontairement exclue de git (`deploy/.gitignore`).

- Copier ce fichier vers un stockage hors du dépôt git (gestionnaire de mots de passe avec pièces
  jointes, coffre chiffré, disque externe) dès sa génération.
- Ne jamais l'envoyer par email en clair.
- Si elle est perdue : régénérer une nouvelle paire de clés (voir `LicenseValidator.cs` pour où la
  clé publique est embarquée), ce qui invalide toutes les licences déjà émises — chaque client
  devra refaire les étapes 3 à 7 avec la nouvelle clé publique après une mise à jour de l'app.
