# Installation client — T4C GestCom Desktop (avec licence)

Ce document est un **mode opératoire** pour Yamen, pas une doc de conception — il décrit le
déroulé complet, dans l'ordre, pour livrer l'application à un nouveau client avec une licence
verrouillée sur sa machine. Pour le hardening SQL Server (compte applicatif à droits réduits,
désactivation de l'accès réseau), voir [`DEPLOY.md`](DEPLOY.md) — ce document ne le duplique pas.

## Vue d'ensemble du flux

```
Yamen                              Client
------------------------------     ------------------------------
1. build-client-package.ps1
   -> App/ + LicenseTool/ + Database/
2. Envoie le package au client --> 3. Lance LicenseTool\collect
                                       -> fingerprint.json
                             <-- 4. Renvoie fingerprint.json à Yamen
5. LicenseTool issue
   (avec la clé privée)
   -> <client>.lic
6. Envoie le .lic au client   --> 7. Place license.lic au bon endroit
                                   8. Installe la base (Database/*.sql)
                                   9. Premier lancement de l'app
```

## 0. Prérequis (une seule fois sur la machine de Yamen)

- `dotnet tool install --global obfuscar.globaltool` (voir DEPLOY.md)
- La clé privée existe à `T4C_GestCom_Desktop/deploy/keys/t4c-license-private.pem`. Si ce fichier
  n'existe pas (nouvelle machine de dev, restauration après incident), voir la section
  **Sauvegarde de la clé privée** en bas de ce document — **sans elle, aucune licence ne peut être
  émise**, quel que soit le code source disponible.

## 1. Construire le package client

```powershell
cd T4C_GestCom_Desktop/deploy
./build-client-package.ps1 -ClientName "Nom-Du-Client"
```

Produit `deploy/client-package/Nom-Du-Client/` avec trois dossiers :

- `App/` — l'application obfusquée, prête à installer (voir DEPLOY.md pour le hardening SQL Server
  à faire sur place chez le client).
- `LicenseTool/` — `T4C_GestCom_LicenseGenerator.exe`, utilisé aux étapes 3 et 5 ci-dessous.
- `Database/` — les scripts SQL de déploiement (`create-app-login.sql`, etc.).

Le script vérifie lui-même qu'aucun fichier de clé privée ne s'est retrouvé dans le package —
s'il détecte quoi que ce soit, il efface le package et lève une erreur. Zippez ensuite tout le
dossier `Nom-Du-Client/` pour l'envoyer au client (email, lien de transfert, clé USB, etc.).

## 2. Envoyer le package au client

Envoyez uniquement le contenu du zip (`App/`, `LicenseTool/`, `Database/`). Rien dedans n'est
sensible : l'app n'a pas encore de licence installée, et `LicenseTool` en mode `collect` ne
nécessite pas la clé privée.

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

## 8. Installer la base de données

Suivre [`DEPLOY.md`](DEPLOY.md) section 2 (`Database/create-app-login.sql`, désactivation de
l'accès réseau à SQL Server, etc.) — ce document ne répète pas ces étapes.

## 9. Premier lancement

Lancer `App\T4C_GestCom_Desktop.exe`. Si la licence est valide, l'écran de connexion s'affiche
normalement. Si un message d'erreur de licence apparaît, vérifier :

- Le fichier est bien nommé `license.lic` (pas `Nom-Du-Client.lic`).
- Il est bien à l'un des deux emplacements listés à l'étape 7.
- L'empreinte n'a pas changé depuis la collecte (carte réseau remplacée, machine réinstallée) —
  dans ce cas, refaire `collect` et réémettre une licence.

L'application revalide la licence toutes les 15 minutes pendant la session ; si le fichier est
supprimé ou modifié en cours d'utilisation, l'app affiche le message d'erreur et se ferme.

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
