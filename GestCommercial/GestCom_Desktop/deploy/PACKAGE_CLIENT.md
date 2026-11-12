# Créer un package client — GestCom Desktop

Fiche opérationnelle : de la **commande du client** jusqu'à l'**envoi du zip**. Elle sert de
check-list. Le détail technique (obfuscation, hardening SQL, mise à jour d'un client existant,
sauvegarde de la clé privée) reste dans [DEPLOY.md](DEPLOY.md), à ne pas dupliquer ici.

Ordre de lecture : **A** (ce que le client doit avoir) → **B** (ce qu'on lui demande) →
**C** (ce qu'on prépare de notre côté) → **D** (construire le package) → **E** (contrôle) →
**F** (envoi) → **G** (suivi).

---

## A. Prérequis côté client — à lui communiquer AVANT la livraison

À envoyer au client (ou à vérifier avec lui) dès la commande. Rien n'est livré avant que cette
liste soit validée.

### A.1 Poste

| Prérequis | Détail |
|---|---|
| Windows 10 ou 11, **64 bits** | Application WinForms .NET 8. Windows 7/8 non supportés. |
| Droits **administrateur local** | Nécessaires uniquement pour l'installation (runtime, SQL Server, création du compte `svc_gestcom`). |
| Espace disque libre | Quelques Go (SQL Server Express + base + sauvegardes). Valeur indicative, non mesurée. |
| Poste **unique** (app + SQL Server sur la même machine) | C'est le seul schéma documenté. Voir le point d'attention sur l'offre *Équipe* en section B.3. |
| Accès à distance (AnyDesk ou RDP) | Recommandé : permet de faire les étapes SQL sans se déplacer. |
| Connexion Internet le jour de l'installation | Pour télécharger les deux logiciels ci-dessous. Aucune connexion n'est requise ensuite. |

### A.2 Logiciels à installer par le client

| Logiciel | Lien | Remarque |
|---|---|---|
| **.NET 8 Desktop Runtime (x64)** | https://dotnet.microsoft.com/en-us/download/dotnet/8.0 (Windows x64) | Runtime seul, pas le SDK. Le build est *framework-dependent*, pas autonome. |
| **SQL Server Express** | https://www.microsoft.com/fr-fr/download/details.aspx?id=104781 | Édition **Express** uniquement (gratuite, 10 Go max par base). **Jamais Developer Edition** pour un client réel. S'installe presque toujours en instance nommée **`SQLEXPRESS`**. |
| **SSMS** ou `sqlcmd` | Fourni avec SQL Server (`sqlcmd`) | Optionnel si vous faites les étapes SQL à distance. |

### A.3 Ce que le client doit nous renvoyer

- `fingerprint.json` (produit par `LicenseTool\GestCom_LicenseGenerator.exe collect`, section G).
- Le nom exact de l'instance SQL Server une fois installée (`.\SQLEXPRESS` en général).

---

## B. Informations à récupérer auprès du client — fiche de collecte

À remplir **avant** de lancer le build. Les champs marqués ★ sont indispensables : sans eux on ne
peut ni émettre la licence, ni configurer correctement l'application.

### B.1 Identité et contact

| Info | Sert à |
|---|---|
| ★ Raison sociale exacte | Option `--client` de la commande `issue` (écrite dans la licence). |
| ★ Matricule fiscal | Fiche *Entreprise* de l'app (nom et matricule fiscal ne doivent pas être vides). |
| ★ Contact principal (nom, téléphone, e-mail) | Envoi du zip et du `.lic`, support. |
| Adresse, téléphone, e-mail, RIB, registre de commerce | Fiche *Entreprise* (en-tête des factures et devis). |
| Logo (PNG) | Champ « Chemin du logo » de la fiche *Entreprise* (`./logoApp.png` par défaut). |

### B.2 Paramètres commerciaux de l'application

Valeurs par défaut dans `GestCom_Desktop/appsettings.json`, section `AppConfig` : à confirmer avec
le client.

| Paramètre | Défaut | À demander |
|---|---|---|
| Timbre fiscal | `0.6` | Confirmer la valeur. |
| Taux de retenue à la source | `1.5` | Confirmer (1.5 %, 0.5 %, 1 %…). |
| Afficher la remise | `Yes` | Oui / non. |
| Afficher la TVA | `Yes` | Oui / non. |

### B.3 Offre et licence

| Info | Sert à |
|---|---|
| ★ Formule achetée : **Essentiel** (1000), **Sérénité** (1400) ou **Équipe** (2400) | Durée de garantie / mises à jour, et nombre de postes. |
| ★ Nombre de postes à équiper | **Une licence = une machine** (empreinte matérielle). Poste supplémentaire : 500. |
| Date d'expiration souhaitée | Option `--expires yyyy-MM-dd`. Par défaut : licence perpétuelle. |

> **Point d'attention — offre Équipe (3 postes)** : DEPLOY.md ne documente que le schéma
> *application + SQL Server sur la même machine, TCP/IP désactivé*. Pour plusieurs postes qui
> partagent une base, ce schéma ne convient pas (il faudrait garder TCP/IP actif). Ce cas n'est
> pas couvert : le trancher avec le client **avant** de vendre cette formule, et le documenter.

### B.4 Environnement technique

| Info | Sert à |
|---|---|
| ★ Version de Windows | Vérifier A.1. |
| ★ Une instance SQL Server est-elle déjà installée ? Laquelle ? | Évite une double installation ; fixe le `-Server` des scripts. |
| Nom de la machine | Repère pour le suivi (il fait partie de l'empreinte). |
| ★ Outil d'accès à distance et horaires de disponibilité | Planifier l'intervention. |
| Utilise-t-il déjà un autre logiciel de gestion ? | Besoin éventuel de reprise de données (hors périmètre actuel des scripts). |
| Sauvegardes : qui, où, à quelle fréquence ? | Rien n'est automatisé dans le package. Prévoir un `BACKUP DATABASE` planifié ou une copie manuelle. |

---

## C. Prérequis côté Yamen (une fois par machine de build)

- **PowerShell 7+** (les scripts ont `#Requires -Version 7.0`) : `winget install Microsoft.PowerShell`.
- **.NET 8 SDK**.
- **Obfuscar** : `dotnet tool install --global obfuscar.globaltool`.
- **Une instance SQL Server locale** pour générer le `.bak` (par défaut `YAMEN`, paramètre `-Server`).
- **La clé privée** `GestCom_Desktop/deploy/keys/gestcom-license-private.pem`. Exclue de git. Sans
  elle, aucune licence ne peut être émise. Si elle manque : voir *Sauvegarde de la clé privée* dans
  DEPLOY.md.

---

## D. Construire le package

Toutes les commandes sont à lancer depuis `GestCom_Desktop/deploy`, dans **PowerShell 7**.

### D.1 Avant de lancer le build

1. Dépôt à jour, branche voulue, **aucune modification non commitée** :
   ```powershell
   git status
   ```
2. Tests au vert :
   ```powershell
   dotnet test ..\..\GestCom_Desktop.Tests
   ```
3. **Régénérer le `.bak` si le schéma a changé depuis sa dernière génération.** Le fichier
   `db-template/GestCom_Template.bak` n'est pas versionné : il dérive en silence. Dans le doute
   (nouvelle version de `Web_GestCom`, nouvelles tables ou colonnes), le régénérer :
   ```powershell
   ./sql/prepare-template-database.ps1
   ```
   Il lance `Web_GestCom` contre une base vide et jetable, puis la sauvegarde. Il ne seede pas de
   données de démo.
4. Vérifier la date du `.bak` produit :
   ```powershell
   Get-Item .\db-template\GestCom_Template.bak | Select-Object LastWriteTime, Length
   ```

### D.2 Générer le package

```powershell
./build-client-package.ps1 -ClientName "Nom-Du-Client"
```

Sans espace ni accent dans `-ClientName` (c'est un nom de dossier). Le script enchaîne :

1. publication Release sans PDB et obfuscation de `GestCom_Desktop.dll` ;
2. publication de l'outil de licence dans `LicenseTool/` ;
3. copie de `GestCom_Template.bak`, `restore-database.ps1` et `create-app-login.sql` dans `Database/` ;
4. contrôle qu'aucune clé privée n'a été copiée (il supprime le package et s'arrête s'il en trouve une).

Résultat : `client-package/Nom-Du-Client/{App, LicenseTool, Database}`.

### D.3 Adapter la configuration pour ce client

`App/appsettings.json` est livré tel que publié. Sa chaîne de connexion pointe sur **`Server=YAMEN`**
(votre machine), pas sur celle du client. Avant de zipper (ou avec le client à l'installation),
la remplacer par l'instance fournie en B.4 :

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=GestCom;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Adapter aussi la section `AppConfig` (timbre, retenue, remise, TVA) avec les réponses de B.2, et
déposer le logo à côté de l'exe si le client en fournit un.

---

## E. Contrôle avant envoi

| ☐ | Vérification |
|---|---|
| ☐ | Le dossier contient exactement `App/`, `LicenseTool/`, `Database/`. |
| ☐ | **Aucun `.pem`, aucun dossier `keys/`, aucun `.lic` d'un autre client** dans le package. Contrôle manuel en plus de celui du script : `Get-ChildItem .\client-package\Nom-Du-Client -Recurse -Include *.pem,*.lic`. |
| ☐ | Les exécutables s'appellent `GestCom_*` (et non `T4C_GestCom_*`, nom des anciens packages Hadil et Ines). |
| ☐ | Pas de fichier parasite dans `Database/` (un `.txt` au nom aléatoire figurait dans le package Hadil). |
| ☐ | `GestCom_Template.bak` récent (voir D.1). |
| ☐ | `appsettings.json` adapté au client (D.3). |
| ☐ | **Test à blanc** : lancer `App\GestCom_Desktop.exe` sur votre machine ou une VM contre une base restaurée depuis ce `.bak`. Se connecter, ouvrir un Devis ou une Facture, tester le combo Produit dans la grille de lignes, enregistrer un document (voir « Vérification obligatoire avant handoff » dans DEPLOY.md). |

Zipper :

```powershell
Compress-Archive -Path .\client-package\Nom-Du-Client\* -DestinationPath .\Nom-Du-Client.zip
```

---

## F. Envoi au client

1. Envoyer **uniquement** `Nom-Du-Client.zip`. Le zip est sans danger : l'app n'a pas encore de licence
   et `collect` n'utilise pas la clé privée.
2. Joindre en un message court :
   - la liste des prérequis de la section **A** (surtout .NET 8 Desktop Runtime et SQL Server Express) ;
   - la consigne : *dézipper, lancer `LicenseTool\GestCom_LicenseGenerator.exe collect` sur le poste
     où l'application sera utilisée, puis renvoyer `fingerprint.json`* ;
   - un créneau pour l'installation à distance.
3. **Ne jamais envoyer** : la clé privée, `Mapping.txt` (dossier `mapping-archive/`).

---

## G. Après l'envoi — rappel du flux

| # | Qui | Action | Détail |
|---|---|---|---|
| 1 | Client | `collect` → `fingerprint.json` | DEPLOY.md §3 |
| 2 | Yamen | Émettre le `.lic` | DEPLOY.md §5 (commande ci-dessous) |
| 3 | Client | Placer `license.lic` dans `%ProgramData%\GestCom\` | DEPLOY.md §7 |
| 4 | Yamen / client | Restaurer la base, créer `svc_gestcom`, désactiver TCP/IP | DEPLOY.md §8 |
| 5 | Yamen / client | Premier lancement, vérifier les logs | DEPLOY.md §9 |
| 6 | Client | **Changer les mots de passe par défaut** `admin/admin123` et `superadmin/SuperAdmin123!` | À faire au premier lancement |

Émission de la licence :

```powershell
.\client-package\Nom-Du-Client\LicenseTool\GestCom_LicenseGenerator.exe issue `
    --fingerprint "chemin\vers\fingerprint.json" `
    --client "Raison sociale du client" `
    --key "keys\gestcom-license-private.pem" `
    --out "Nom-Du-Client.lic"
```

Ajouter `--expires yyyy-MM-dd` si la licence doit expirer.

---

## H. Fiche de suivi (à recopier pour chaque client)

| Champ | Valeur |
|---|---|
| Client / raison sociale | |
| Matricule fiscal | |
| Formule / nb de postes | |
| Date de commande / d'envoi du zip | |
| Version livrée (commit git) | |
| Date du `.bak` embarqué | |
| Nom de la machine / instance SQL | |
| `LicenseId` émis | |
| Date d'expiration de la licence | perpétuelle |
| Date de fin de garantie / de maintenance | |
| Mots de passe par défaut changés ? | ☐ admin ☐ superadmin |
| Notes | |
