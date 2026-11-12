# Guide pas à pas — Livrer GestCom Desktop à un client

> **Pour qui ?** Pour la personne qui prépare et livre le package, **même sans expérience**.
> Suivez les étapes **dans l'ordre**, sans en sauter. Chaque étape dit : *quoi faire*, *la commande à
> copier*, *ce que vous devez voir si tout va bien*, et *quoi faire sinon*.
>
> **Document de référence technique** : [DEPLOY.md](DEPLOY.md) (obfuscation, sécurité SQL, mise à jour
> d'un client existant). Ce guide-ci est la version « mode d'emploi » : en cas de doute sur le *pourquoi*,
> aller voir DEPLOY.md ; en cas de doute sur le *comment*, rester ici.

---

## Sommaire

0. [Vue d'ensemble et règles d'or](#0-vue-densemble-et-règles-dor)
1. [Prérequis côté CLIENT](#1-prérequis-côté-client--à-lui-communiquer-avant-tout)
2. [Informations à récupérer auprès du client](#2-informations-à-récupérer-auprès-du-client-fiche-de-collecte)
3. [Préparer VOTRE machine](#3-préparer-votre-machine-une-seule-fois)
4. [Construire le package](#4-construire-le-package)
5. [Contrôler le package avant envoi](#5-contrôler-le-package-avant-envoi)
6. [Envoyer au client](#6-envoyer-au-client)
7. [Recevoir l'empreinte et émettre la licence](#7-recevoir-lempreinte-et-émettre-la-licence-yamen)
8. [Installation chez le client](#8-installation-chez-le-client)
9. [Dépannage](#9-dépannage--symptôme--cause--solution)
10. [Fiche de suivi client](#10-fiche-de-suivi-client)

---

## 0. Vue d'ensemble et règles d'or

### Ce qu'on livre

GestCom Desktop est une application Windows installée **sur le PC du client**. Sa base de données
(SQL Server Express) est installée **sur ce même PC**. Une **licence** (fichier `.lic`) la verrouille
sur ce PC : copiée sur une autre machine, elle ne fonctionne pas.

### Le déroulé en 7 étapes

```
 VOUS                                             CLIENT
 ───────────────────────────────                  ───────────────────────────────
 A. Envoyer la liste des prérequis  ───────────►  Installe .NET 8 + SQL Server Express
 B. Remplir la fiche de collecte    ◄───────────► Donne ses informations
 C. Construire le package (zip)
 D. Envoyer le zip                  ───────────►  Dézippe, lance « collect »
                                    ◄───────────  Renvoie fingerprint.json
 E. YAMEN émet la licence (.lic)    ───────────►  Place license.lic
 F. Installer la base de données (à distance de préférence)
 G. Premier lancement + changement des mots de passe
```

### Glossaire

| Mot | Signification |
|---|---|
| **Package** | Le dossier (puis zip) livré au client : `App/` + `LicenseTool/` + `Database/`. |
| **Empreinte** (`fingerprint.json`) | Fichier qui décrit le PC du client (identifiant Windows, carte réseau, nom du PC, processeur). Sert à fabriquer une licence valable pour *ce* PC seulement. Il ne contient rien de secret. |
| **Licence** (`.lic`) | Fichier signé fabriqué par Yamen à partir de l'empreinte. Sans lui, l'application refuse de démarrer. |
| **Clé privée** (`.pem`) | Le secret de Yamen qui signe les licences. **Ne sort jamais de sa machine.** |
| **`.bak`** | Sauvegarde SQL Server contenant une base vide prête à l'emploi (tables + données de départ). |
| **Instance SQL** | Nom du serveur de base de données sur le PC. Avec SQL Server Express c'est presque toujours `.\SQLEXPRESS`. |
| **Obfuscation** | Brouillage du code pour le rendre plus difficile à lire. Fait automatiquement par le script. |
| **PowerShell 7** (`pwsh`) | Le terminal dans lequel on lance tous les scripts. ⚠️ Différent de « Windows PowerShell » (version 5, bleu), qui ne convient **pas**. |

### Les 6 règles d'or

1. 🔒 **Jamais** de fichier `.pem` ni de dossier `keys/` dans ce qu'on envoie au client.
2. 🔒 **Jamais** le dossier `mapping-archive/` ni un fichier `Mapping*.txt` chez le client.
3. **Une licence = un PC.** L'empreinte doit être collectée **sur le PC où l'application sera utilisée**.
4. **Toujours régénérer le `.bak`** si l'application a changé (étape 3.4) : sinon le client reçoit une
   base obsolète.
5. **Ne jamais utiliser `-Force`** pour restaurer une base chez un client sans avoir fait une sauvegarde
   de la base existante : cette option **écrase** les données.
6. **En cas de doute, s'arrêter et demander à Yamen.** Rien n'est urgent au point de risquer des données
   client.

---

## 1. Prérequis côté CLIENT — à lui communiquer avant tout

Envoyer cette section au client **dès la commande**. Ne rien livrer tant qu'il n'a pas confirmé que tout est
en place.

### 1.1 Le PC

| ☐ | Prérequis | Pourquoi |
|---|---|---|
| ☐ | **Windows 10 (version 1607 ou plus récente) ou Windows 11, 64 bits** | L'application est une application Windows .NET 8. |
| ☐ | Un **compte Windows administrateur** sur ce PC | Pour installer les logiciels et créer le compte applicatif. |
| ☐ | **Un seul PC** pour l'application *et* la base de données | Seul schéma documenté (voir l'avertissement en 2.3 pour la formule *Équipe*). |
| ☐ | Quelques Go d'espace disque libre | SQL Server + base + sauvegardes (valeur indicative). |
| ☐ | **Internet** le jour de l'installation | Pour télécharger les logiciels ci-dessous (inutile ensuite). |
| ☐ | **AnyDesk** (ou RDP) installé, et un créneau de disponibilité | Permet de faire l'installation à distance. |

### 1.2 Les logiciels que le client installe (dans cet ordre)

| # | Logiciel | Où le télécharger | Points d'attention |
|---|---|---|---|
| 1 | **.NET 8 Desktop Runtime (x64)** | https://dotnet.microsoft.com/en-us/download/dotnet/8.0 → section *.NET Desktop Runtime* → **Windows x64** | Le *Runtime*, **pas le SDK**. Redémarrer le PC si l'installeur le demande. |
| 2 | **SQL Server Express** | https://www.microsoft.com/fr-fr/download/details.aspx?id=104781 | Édition **Express** uniquement (gratuite, base limitée à 10 Go). **Jamais** « Developer ». Choisir l'installation **Basique**. Noter le nom d'instance affiché à la fin (normalement `SQLEXPRESS`). **L'installer avec le compte administrateur qui fera aussi la suite** : ce compte aura les droits sur SQL Server. |
| 3 | **PowerShell 7** | Dans un terminal : `winget install Microsoft.PowerShell` (ou https://aka.ms/powershell) | **Obligatoire** : les scripts de restauration de la base refusent de tourner dans l'ancien PowerShell. Inutile si c'est Yamen qui fait ces étapes depuis sa propre machine à distance. |
| 4 | *(facultatif)* **SSMS** | https://aka.ms/ssms | Seulement pour regarder la base. Pas nécessaire. |

### 1.3 Ce que le client doit nous renvoyer

- Le fichier **`fingerprint.json`** (voir étape 8.1).
- Le **nom exact de l'instance SQL** (ex. `.\SQLEXPRESS`).

### 1.4 Message prêt à envoyer au client (à adapter)

> Bonjour,
> Pour installer GestCom sur votre PC, merci de préparer ceci à l'avance :
> 1. Un PC sous **Windows 10 ou 11 (64 bits)**, et un compte **administrateur**.
> 2. Installer **.NET 8 Desktop Runtime x64** : https://dotnet.microsoft.com/en-us/download/dotnet/8.0
> 3. Installer **SQL Server Express** (installation *Basique*) :
>    https://www.microsoft.com/fr-fr/download/details.aspx?id=104781
> 4. Installer **AnyDesk** : https://anydesk.com
>
> Dites-moi quand c'est fait, et quel créneau vous convient pour la suite (environ une heure, à distance).

---

## 2. Informations à récupérer auprès du client (fiche de collecte)

Remplir cette fiche **avant** l'étape 4. Les champs marqués ★ sont **obligatoires** : sans eux on ne peut
ni émettre la licence ni configurer l'application.

### 2.1 Identité et contact

| ☐ | Info | Sert à | Réponse |
|---|---|---|---|
| ☐ | ★ **Raison sociale exacte** | Écrite dans la licence (étape 7). | |
| ☐ | ★ **Matricule fiscal** | Fiche *Entreprise* de l'application (nom et matricule fiscal obligatoires). | |
| ☐ | ★ **Contact principal** (nom, téléphone, e-mail) | Envoi des fichiers, support. | |
| ☐ | Adresse, téléphone, e-mail, RIB, registre de commerce | Fiche *Entreprise* (en-tête des factures et devis). | |
| ☐ | **Logo** (fichier PNG) | Champ « Chemin du logo » de la fiche *Entreprise*. | |

### 2.2 Réglages de l'application

Valeurs par défaut du fichier `GestCom_Desktop/appsettings.json` (section `AppConfig`). **Les confirmer**
avec le client :

| ☐ | Réglage | Défaut | Réponse du client |
|---|---|---|---|
| ☐ | Timbre fiscal | `0.6` | |
| ☐ | Taux de retenue à la source | `1.5` | |
| ☐ | Afficher la remise | `Yes` | |
| ☐ | Afficher la TVA | `Yes` | |

### 2.3 Offre et licence

| ☐ | Info | Réponse |
|---|---|---|
| ☐ | ★ Formule : **Essentiel** (1000) / **Sérénité** (1400) / **Équipe** (2400) | |
| ☐ | ★ Nombre de PC à équiper (**une licence par PC** ; PC supplémentaire : 500) | |
| ☐ | Date d'expiration de la licence souhaitée (vide = **perpétuelle**) | |

> ⚠️ **Formule Équipe (3 postes)** : la procédure actuelle ne gère que « application + base sur le même
> PC, accès réseau à SQL Server désactivé ». Si plusieurs PC doivent **partager la même base**, ce schéma
> ne convient pas. **Ne pas promettre ce cas sans en parler à Yamen.**

### 2.4 Environnement technique

| ☐ | Info | Réponse |
|---|---|---|
| ☐ | ★ Version de Windows du PC | |
| ☐ | ★ SQL Server est-il déjà installé ? Quelle instance ? | |
| ☐ | ★ Accès à distance (AnyDesk / RDP) : identifiant et horaires | |
| ☐ | Utilise-t-il déjà un autre logiciel de gestion ? (reprise de données = hors périmètre des scripts actuels) | |
| ☐ | Qui fera les sauvegardes de la base, et à quelle fréquence ? (rien n'est automatisé dans le package) | |

---

## 3. Préparer VOTRE machine (une seule fois)

### 3.1 Ouvrir PowerShell 7

Menu **Démarrer** → taper **PowerShell 7** → l'ouvrir.
Vérifier :

```powershell
$PSVersionTable.PSVersion
```

✅ Attendu : `Major` vaut **7** (ou plus).
❌ Si c'est 5 : vous êtes dans l'ancien PowerShell. Installer le bon : `winget install Microsoft.PowerShell`,
puis rouvrir et chercher « PowerShell 7 » (icône noire).

**Toutes les commandes de ce guide se lancent dans PowerShell 7.**

### 3.2 Vérifier les outils

Lancer chaque commande ; comparer au résultat attendu.

| Vérification | Commande | Résultat attendu | Sinon |
|---|---|---|---|
| .NET SDK 8 | `dotnet --version` | commence par `8.` | Installer le **SDK** .NET 8 : https://dotnet.microsoft.com/download/dotnet/8.0 |
| Obfuscar | `Get-Command obfuscar.console` | une ligne avec `obfuscar.console` | `dotnet tool install --global obfuscar.globaltool`, puis **fermer et rouvrir** PowerShell |
| sqlcmd | `Get-Command sqlcmd` | une ligne avec `sqlcmd` | Installer SQL Server Express ou les outils en ligne de commande SQL Server |
| Instance SQL locale | `Get-Service MSSQL*` | un service `Running` | Démarrer le service SQL Server (`services.msc`) |
| Clé privée *(Yamen)* | `Test-Path .\keys\gestcom-license-private.pem` (depuis le dossier `deploy`) | `True` | Voir « Sauvegarde de la clé privée » dans DEPLOY.md. **Sans elle, aucune licence ne peut être émise.** |

### 3.3 Se placer dans le bon dossier

```powershell
cd D:\Entreprise\Projets\GitProjects\ElFatoura\ElFatourTN_TTN\elfatouraTN\GestCommercial\GestCom_Desktop\deploy
```

(Adapter le début du chemin si le dépôt est ailleurs sur votre PC.)

✅ Vérifier : `ls` affiche notamment `build-client-package.ps1`, `publish-and-obfuscate.ps1`, `sql`, `DEPLOY.md`.

Si PowerShell répond *« l'exécution de scripts est désactivée sur ce système »* ou *« n'est pas signé
numériquement »*, voir le dépannage (§9) — c'est un blocage de sécurité de Windows, sans gravité.

### 3.4 Vérifier le code et régénérer la base de départ

**a) Le dépôt est à jour et propre** (à faire dans le dossier `deploy`) :

```powershell
git status
git pull
```

✅ Attendu : `nothing to commit, working tree clean`. S'il y a des modifications non voulues, **demander à
Yamen** avant de continuer : le package embarque le code actuel.

**b) Les tests passent :**

```powershell
dotnet test ..\..\GestCom_Desktop.Tests
```

✅ Attendu : `Réussi!` / `Passed!` avec 0 échec. ❌ En cas d'échec : **ne pas livrer**, prévenir Yamen.

**c) Régénérer le `.bak`.** À refaire **à chaque nouvelle livraison**, sauf si Yamen confirme que
l'application n'a pas changé depuis la dernière génération :

```powershell
./sql/prepare-template-database.ps1 -Server "VOTRE-INSTANCE"
```

Remplacer `VOTRE-INSTANCE` par l'instance SQL de **votre** machine (par défaut le script cherche `YAMEN` ; sur
une autre machine, c'est par exemple `.\SQLEXPRESS` ou `MONPC\SQLEXPRESS`).

✅ Attendu : le script se termine sans erreur et crée `db-template\GestCom_Template.bak`.

**d) Vérifier que le fichier est bien récent :**

```powershell
Get-Item .\db-template\GestCom_Template.bak | Select-Object LastWriteTime, Length
```

✅ `LastWriteTime` = aujourd'hui, `Length` de l'ordre de 5 Mo. ❌ Sinon, l'étape c) n'a pas fonctionné.

---

## 4. Construire le package

### 4.1 Choisir le nom du dossier

Prendre un nom **sans espace ni accent**, ex. `Societe-Exemple` ou `Ines-2026-11`.

⚠️ **Ce nom sert de nom de dossier, et le script SUPPRIME le dossier s'il existe déjà.** Regarder d'abord ce
qui existe :

```powershell
ls .\client-package
```

Ce dossier contient les packages **et les licences déjà émises** des clients précédents. Ne **jamais**
réutiliser le nom d'un client existant ; pour livrer une nouvelle version à un client existant, ajouter la date
(`Nom-2026-11`). Voir aussi §10 : conserver ailleurs une copie des `fingerprint.json` et `.lic`.

### 4.2 Lancer la construction

```powershell
./build-client-package.ps1 -ClientName "Societe-Exemple"
```

Le script enchaîne 4 phases et affiche des lignes `==>` en bleu :

| Phase | Ce qu'il fait |
|---|---|
| `[1/4]` | Compile l'application en Release et l'obfusque. |
| `[2/4]` | Prépare l'outil de licence (`LicenseTool/`). |
| `[3/4]` | Copie les scripts SQL et le `.bak` dans `Database/`. |
| `[4/4]` | Vérifie qu'**aucune clé privée** n'a été copiée. |

✅ **Attendu à la fin** : des lignes vertes, dont
`OK — aucune clé privée dans le package.` puis `Package client prêt : …\client-package\Societe-Exemple`.

Le script écrit aussi, en jaune, un rappel de faire un test avant livraison : c'est l'objet de l'étape 5.

❌ **Si le script s'arrête avec une erreur rouge** : lire le message, chercher dans le dépannage (§9). Ne **jamais**
livrer un dossier issu d'un script qui a échoué.

### 4.3 Adapter la configuration au client

Ouvrir `client-package\Societe-Exemple\App\appsettings.json` avec le **Bloc-notes** (ou VS Code).

Il contient actuellement `Server=YAMEN`, c'est-à-dire **votre** machine. Il faut le remplacer par
l'instance du client (réponse en 2.4, en général `.\SQLEXPRESS`) :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=GestCom;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "AppConfig": {
    "TimbreFiscal": "0.6",
    "TauxRetenue": "1.5",
    "DisplayRemise": "Yes",
    "DisplayTVA": "Yes",
    "PathLogo": "./logoApp.png"
  }
}
```

Points de vigilance :

- Dans le JSON, la barre oblique inverse est **doublée** : `.\\SQLEXPRESS` (et non `.\SQLEXPRESS`).
- Garder les guillemets, virgules et accolades exactement comme ci-dessus. Une erreur ici empêche
  l'application de démarrer.
- Reporter dans `AppConfig` les réponses du client (fiche 2.2).
- Si le client a fourni un logo, le copier à côté de `GestCom_Desktop.exe` sous le nom `logoApp.png`.

> Si vous préférez ne pas modifier le fichier à l'avance, cette même modification peut être faite avec le
> client lors de l'installation (étape 8) — mais alors **ne pas l'oublier**.

---

## 5. Contrôler le package avant envoi

### 5.1 Contrôle du contenu

Dans PowerShell, toujours depuis le dossier `deploy` :

```powershell
# 1) La structure : doit afficher exactement App, Database, LicenseTool
ls .\client-package\Societe-Exemple

# 2) Aucune clé, aucune licence, aucun fichier de mapping : doit ne RIEN afficher
Get-ChildItem .\client-package\Societe-Exemple -Recurse -File |
  Where-Object { $_.Name -like "*.pem" -or $_.Name -like "*.lic" -or $_.Name -like "Mapping*" -or $_.Name -like "*private*" }

# 3) Le contenu de Database : doit afficher 4 fichiers seulement
ls .\client-package\Societe-Exemple\Database
```

| ☐ | Contrôle | Attendu |
|---|---|---|
| ☐ | Commande 1 | 3 dossiers : `App`, `Database`, `LicenseTool` |
| ☐ | Commande 2 | **Aucun résultat.** Si quelque chose s'affiche : STOP, ne pas envoyer, prévenir Yamen. |
| ☐ | Commande 3 | `GestCom_Template.bak`, `restore-database.ps1`, `create-app-login.sql` (+ éventuellement un autre `.sql`). Aucun fichier à nom bizarre. |
| ☐ | `App\GestCom_Desktop.exe` existe | Oui |
| ☐ | `LicenseTool\GestCom_LicenseGenerator.exe` existe | Oui (le nom commence par `GestCom_`, pas `T4C_GestCom_` : ce dernier est l'ancien nom des premiers packages) |
| ☐ | `appsettings.json` adapté au client (4.3) | Oui |

### 5.2 Test à blanc (obligatoire)

Objectif : **vérifier que l'application démarre et fonctionne**, sur *votre* machine, **avant** de l'envoyer.

> ⚠️ Ne testez **jamais** directement dans le dossier du package (vous risqueriez d'y laisser une licence ou
> de modifier `appsettings.json`). Faites une **copie** :

```powershell
Copy-Item .\client-package\Societe-Exemple\App C:\Temp\test-gestcom -Recurse
```

1. **Une base de test** : restaurer le `.bak` sous un nom de test, sur votre instance :
   ```powershell
   .\client-package\Societe-Exemple\Database\restore-database.ps1 -Server "VOTRE-INSTANCE" -TargetDatabaseName "GestCom_Test"
   ```
   ✅ Attendu : `OK — base 'GestCom_Test' restaurée sur '…'`.
2. **Pointer la copie sur cette base** : ouvrir `C:\Temp\test-gestcom\appsettings.json`, mettre
   `Server=VOTRE-INSTANCE` et `Database=GestCom_Test`.
3. **Une licence de test** pour *votre* PC (l'application refuse de démarrer sans licence). Si votre PC n'a
   pas déjà de licence dans `C:\ProgramData\GestCom\license.lic`, demander à Yamen d'en émettre une pour
   votre poste (procédure de l'étape 7 : `collect` sur votre PC, `issue` par Yamen) et la placer dans
   **`C:\ProgramData\GestCom\license.lic`** — *pas* à côté de l'exe de test, ni dans le package.
4. **Lancer** `C:\Temp\test-gestcom\GestCom_Desktop.exe`, puis :

| ☐ | Test | Attendu |
|---|---|---|
| ☐ | L'écran de connexion s'affiche | Oui |
| ☐ | Connexion avec `admin` / `admin123` | Accès à l'écran principal |
| ☐ | Créer un **Devis** : ajouter une ligne et **choisir un produit** dans la liste déroulante | La liste fonctionne (c'est l'endroit le plus fragile après obfuscation) |
| ☐ | **Enregistrer** le devis | Aucune erreur |
| ☐ | Ouvrir une **Facture** | S'ouvre normalement |

❌ Si un test échoue : **ne pas livrer**. Regarder le journal (`C:\ProgramData\GestCom\logs\app-<date>.log`,
sinon `%LOCALAPPDATA%\GestCom\logs\`) et prévenir Yamen avec le message d'erreur.

Une fois fini, nettoyer : supprimer `C:\Temp\test-gestcom` ; la base `GestCom_Test` peut être supprimée
dans SSMS (jamais la base `GestCom`).

### 5.3 Créer le zip

```powershell
Compress-Archive -Path .\client-package\Societe-Exemple\* -DestinationPath .\client-package\Societe-Exemple.zip
```

✅ Un fichier `Societe-Exemple.zip` (de l'ordre de 6 Mo) apparaît dans `client-package`.

---

## 6. Envoyer au client

1. Envoyer **uniquement** le fichier `Societe-Exemple.zip` (e-mail, lien de téléchargement, etc.). Il ne
   contient rien de sensible.
2. Dans le même message, donner ces instructions :

> **Instructions pour le client**
> 1. Enregistrer le zip, puis **clic droit → Propriétés → cocher « Débloquer » → OK** (s'il y a cette case).
> 2. **Clic droit → Extraire tout…** vers **`C:\GestCom`** (ne pas le placer dans *Bureau* ni *Documents*).
> 3. Ouvrir le dossier `C:\GestCom\LicenseTool`, puis ouvrir un terminal dans ce dossier (clic droit dans
>    une zone vide du dossier → *Ouvrir dans le terminal*) et taper (un double-clic sur l'exe ne suffit pas) :
>    ```
>    .\GestCom_LicenseGenerator.exe collect
>    ```
> 4. Un fichier **`fingerprint.json`** apparaît dans le même dossier. **Nous le renvoyer.**
>
> ⚠️ Cette opération doit être faite **sur le PC où GestCom sera utilisé**, pas sur un autre.

3. **Ne jamais envoyer** : la clé privée, le dossier `mapping-archive`, ni le contenu d'un autre client.

---

## 7. Recevoir l'empreinte et émettre la licence (Yamen)

> 🔒 **Étape réservée à Yamen** : elle utilise la clé privée. L'opérateur débutant prépare le dossier et
> transmet le `fingerprint.json` à Yamen ; il n'a **pas** à manipuler la clé.

### 7.1 Ranger le fichier reçu

Enregistrer `fingerprint.json` **dans un dossier à vous, hors du dépôt Git et hors de `client-package`**
(ex. `D:\Entreprise\Clients\Societe-Exemple\`). Le dossier `client-package` peut être effacé par un nouveau
build ; les empreintes et licences, elles, ne se régénèrent pas.

### 7.2 Émettre la licence

Depuis le dossier `deploy` :

```powershell
.\client-package\Societe-Exemple\LicenseTool\GestCom_LicenseGenerator.exe issue `
    --fingerprint "D:\Entreprise\Clients\Societe-Exemple\fingerprint.json" `
    --client "Raison sociale exacte du client" `
    --key ".\keys\gestcom-license-private.pem" `
    --out "D:\Entreprise\Clients\Societe-Exemple\Societe-Exemple.lic"
```

Ajouter ` --expires 2027-12-31` (format `aaaa-mm-jj`) **uniquement** si la licence doit expirer. Sans cette
option, elle est perpétuelle.

✅ **Attendu** :

```
Licence générée avec succès.
  Client       : Raison sociale exacte du client
  LicenseId    : <identifiant>
  Expiration   : aucune (perpétuelle)
  Fichier      : …\Societe-Exemple.lic
```

Noter le **LicenseId** dans la fiche de suivi (§10).

### 7.3 Envoyer le `.lic` au client

Envoyer uniquement `Societe-Exemple.lic` (e-mail possible : il ne fonctionne que sur le PC de l'empreinte).

---

## 8. Installation chez le client

À faire **sur le PC du client**, avec son compte administrateur, en préférence **à distance** (AnyDesk). Le
client doit avoir terminé la section 1.

> Les commandes SQL se lancent dans **PowerShell 7 ouvert en tant qu'administrateur**
> (Démarrer → *PowerShell 7* → clic droit → *Exécuter en tant qu'administrateur*).

### 8.1 Collecte de l'empreinte *(déjà faite à l'étape 6)*

Si ce n'est pas fait : dans `C:\GestCom\LicenseTool`, `.\GestCom_LicenseGenerator.exe collect`, puis renvoyer
`fingerprint.json` (étapes 6 et 7).

### 8.2 Placer la licence

1. Ouvrir `C:\ProgramData` (dossier **masqué** : taper `C:\ProgramData` dans la barre d'adresse de l'Explorateur).
2. Créer le dossier **`GestCom`** s'il n'existe pas.
3. Y copier le fichier reçu et le **renommer exactement `license.lic`** (pas `Societe-Exemple.lic`).

Chemin final : `C:\ProgramData\GestCom\license.lic`.

### 8.3 Installer la base de données

Dans le terminal administrateur :

```powershell
cd C:\GestCom\Database
```

**a) Trouver le nom de l'instance :**

```powershell
Get-Service | Where-Object { $_.Name -like "MSSQL*" }
```

Un service nommé `MSSQL$SQLEXPRESS` signifie que l'instance s'appelle `.\SQLEXPRESS`.

**b) Restaurer la base :**

```powershell
.\restore-database.ps1 -Server ".\SQLEXPRESS"
```

✅ Attendu : quatre lignes `==> [1/4]` à `[4/4]`, puis
`OK — base 'GestCom' restaurée sur '.\SQLEXPRESS' (xx tables).`

❌ Si le script dit que la base existe déjà : **STOP**. C'est peut-être une vraie base avec des données.
Ne pas ajouter `-Force` sans l'accord de Yamen (voir règle d'or 5).

**c) Créer le compte applicatif à droits limités :**

```powershell
net user svc_gestcom "MOT-DE-PASSE-FORT-A-GENERER" /add
sqlcmd -S .\SQLEXPRESS -E -C -i create-app-login.sql
```

- Générer un mot de passe fort (gestionnaire de mots de passe) et **le noter dans la fiche de suivi sécurisée**.
- ✅ Attendu pour `net user` : `La commande s'est terminée correctement.`

**d) Désactiver l'accès réseau à SQL Server et lancer l'application sous ce compte** :
ces deux opérations (SQL Server Configuration Manager, tâche planifiée ou raccourci `runas`) sont décrites
dans **[DEPLOY.md §8.3 et §8.2](DEPLOY.md)**. **La première fois, les faire avec Yamen.**

### 8.4 Premier lancement

Lancer `C:\GestCom\App\GestCom_Desktop.exe`.

| ☐ | Vérification | Attendu |
|---|---|---|
| ☐ | L'écran de connexion s'affiche | Oui (sinon : message de licence ou de base → §9) |
| ☐ | Connexion `admin` / `admin123` | Accès à l'application |
| ☐ | Renseigner la fiche **Entreprise** (nom, matricule fiscal, logo…) | Enregistrée sans erreur |
| ☐ | **Changer le mot de passe `admin`** | Fait |
| ☐ | **Changer le mot de passe `superadmin`** (mot de passe par défaut : voir DEPLOY.md §10) | Fait |
| ☐ | Créer et enregistrer un devis de test, puis le supprimer | Fonctionne |

> ⚠️ Les mots de passe par défaut sont **connus de tous nos clients**. Ne pas partir avant de les avoir
> changés.

---

## 9. Dépannage — symptôme → cause → solution

### Pendant la préparation (votre machine)

| Symptôme (message) | Cause probable | Solution |
|---|---|---|
| *« …requiert la version 7.0 »* / `#Requires` | Vous êtes dans Windows PowerShell 5 | Ouvrir **PowerShell 7** (§3.1). |
| *« l'exécution de scripts est désactivée »* / *« n'est pas signé numériquement »* | Protection Windows contre les scripts | Dans le dossier du script : `Unblock-File .\*.ps1` ; ou lancer `pwsh -ExecutionPolicy Bypass -File .\nom-du-script.ps1 …`. |
| `obfuscar.console not found` | Obfuscar non installé, ou terminal ouvert avant l'installation | `dotnet tool install --global obfuscar.globaltool`, fermer et rouvrir PowerShell. |
| *« Image de base introuvable »* | `.bak` jamais généré | Faire l'étape 3.4 c). |
| *« sqlcmd introuvable »* | SQL Server / ses outils non installés | Installer SQL Server Express (§1.2). |
| *« Impossible de contacter l'instance SQL Server »* | Mauvais nom d'instance ou service arrêté | `Get-Service MSSQL*` ; corriger `-Server` ; démarrer le service. |
| `ALERTE : des fichiers ressemblant à des clés…` | Un fichier `.pem`/`private` a atterri dans le package | Le script a supprimé le package. **Ne pas contourner** : chercher la cause avec Yamen. |
| `dotnet publish failed` | Une application ou un terminal utilise les fichiers ; ou erreur de code | Fermer toute instance de l'application/Visual Studio, relancer. Si l'erreur persiste, envoyer le message à Yamen. |
| *« Format de date invalide pour --expires »* | Mauvais format | `--expires 2027-12-31` (année-mois-jour). |
| *« Fichier d'empreinte introuvable »* | Mauvais chemin vers `fingerprint.json` | Vérifier le chemin avec `Test-Path`. |

### Chez le client

| Symptôme | Cause probable | Solution |
|---|---|---|
| Windows propose de télécharger .NET au lancement | Runtime manquant | Installer **.NET 8 Desktop Runtime x64** (§1.2). |
| *« Aucune licence n'a été trouvée sur ce poste »* | `license.lic` absent ou mal nommé / mal placé | Vérifier : `C:\ProgramData\GestCom\license.lic` (nom exact, extension non masquée : pas `license.lic.lic`). |
| *« Le fichier de licence est illisible ou corrompu »* | Fichier modifié ou tronqué à l'envoi | Redemander le `.lic` à Yamen. |
| *« La signature de la licence est invalide »* | Fichier altéré, ou licence d'une autre application | Redemander le `.lic`. |
| *« Cette licence ne correspond pas à ce poste »* | `collect` fait sur **un autre PC**, ou matériel changé (carte réseau, réinstallation Windows) | Refaire `collect` **sur le bon PC** et faire réémettre la licence. |
| *« La licence de ce poste a expiré »* | Date d'expiration dépassée | Faire réémettre une licence. |
| L'application s'arrête en cours d'utilisation avec un message de licence | Le fichier `license.lic` a été supprimé/modifié (revérifié toutes les 15 min) | Le remettre, relancer. |
| Erreur de connexion à la base | `Server=` incorrect dans `appsettings.json`, base non restaurée, service SQL arrêté, ou compte sans droits | Vérifier le nom d'instance (8.3 a), le JSON (barre `\\` doublée), `Get-Service MSSQL*`, et que l'étape 8.3 c) a été faite. |
| *« Fournisseur de canaux nommés : Impossible d'ouvrir une connexion à SQL Server [2] »* | Mauvais nom d'instance | Utiliser `-Server ".\SQLEXPRESS"` (8.3). |
| L'application ne démarre pas du tout, aucun message | `appsettings.json` invalide (JSON cassé) | Le recomparer au modèle de 4.3. Regarder les logs. |

**Où lire les journaux :** `C:\ProgramData\GestCom\logs\app-<date>.log`, ou à défaut
`%LOCALAPPDATA%\GestCom\logs\` (selon le compte qui lance l'application). Toujours joindre le message exact
(capture d'écran) quand on demande de l'aide.

---

## 10. Fiche de suivi client

Recopier ce tableau pour chaque client et le garder **hors du dépôt Git** (il contiendra des mots de passe).

| Champ | Valeur |
|---|---|
| Client / raison sociale | |
| Matricule fiscal | |
| Formule / nombre de PC | |
| Date de commande | |
| Nom du dossier package (4.1) | |
| Date d'envoi du zip | |
| Version livrée (`git log -1 --format=%h`) | |
| Date du `.bak` embarqué | |
| Instance SQL du client | |
| Nom du PC du client | |
| `LicenseId` émis | |
| Expiration de la licence | perpétuelle |
| Fin de garantie / de maintenance | |
| Mot de passe de `svc_gestcom` | *(gestionnaire de mots de passe, jamais ici en clair)* |
| Mots de passe par défaut changés | ☐ `admin`  ☐ `superadmin` |
| Emplacement sauvegardé du `fingerprint.json` et du `.lic` | |
| Notes / incidents | |

---

## Annexe — Mettre à jour un client déjà livré

**Ne jamais** simplement remplacer l'exécutable d'un client : sa base doit être migrée en même temps (sinon
erreurs SQL). Procédure complète : [DEPLOY.md §10](DEPLOY.md). À ne pas faire sans Yamen.
