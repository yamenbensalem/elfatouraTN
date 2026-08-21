# Déploiement client — T4C GestCom Desktop

Ce document couvre le déploiement chez un client où l'application **et** SQL Server tournent sur
la même machine (poste unique). Deux volets : protéger le code contre la décompilation "casuelle",
et limiter ce qu'on peut faire avec la base si quelqu'un accède à la machine.

**À lire d'abord** : aucune des deux protections n'est absolue. Du code managé .NET reste toujours
décompilable avec assez de temps — on ne fait que relever la barre. Et si le client a un accès
admin/physique à sa propre machine, il pourra toujours atteindre sa propre base SQL Server avec les
bons droits Windows — on limite les dégâts possibles, on ne rend pas la base inatteignable.

## 1. Build + obfuscation

### Prérequis (une seule fois)

```bash
dotnet tool install --global obfuscar.globaltool
```

### Build

```powershell
cd T4C_GestCom_Desktop/deploy
./publish-and-obfuscate.ps1
```

Produit `deploy/dist/` — c'est ce dossier qu'on zippe et qu'on donne au client. Le script :

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

### Vérification obligatoire avant handoff

Le script obfusque avec un scope volontairement réduit (voir ci-dessus), validé par un test
scripté (lancement + requête EF Core réelle), **pas** par un passage manuel sur les 14 écrans.
Avant de livrer : lancer `deploy/dist/T4C_GestCom_Desktop.exe` contre une vraie base, se connecter,
ouvrir un Devis ou une Facture, tester le combo Produit dans la grille de lignes (c'est le seul
endroit où l'obfuscation a dû exclure explicitement un type — `ProduitOption`), enregistrer un
document.

## 2. Base de données — SQL Server local sur le poste client

### 2.1 Compte applicatif à droits minimaux

Ne pas faire tourner l'app avec le compte Windows interactif du client (souvent admin local). Sur
la machine du client, en admin :

```
net user svc_t4cgestcom "<mot de passe fort généré>" /add
```

Puis exécuter `deploy/sql/create-app-login.sql` dans SSMS (ou `sqlcmd`) connecté en admin sur
l'instance SQL Server du client. Ça crée un login Windows dédié, scoped à `db_datareader` +
`db_datawriter` sur `T4C_GestCom` uniquement — pas `db_owner`, pas `sysadmin`. Si la machine est
compromise, ce compte ne peut lire/écrire que les tables de l'app, pas modifier le schéma ni
toucher aux autres bases.

Faire tourner `T4C_GestCom_Desktop.exe` sous ce compte (tâche planifiée configurée "Exécuter en
tant que" `svc_t4cgestcom`, ou raccourci `runas /user:.\svc_t4cgestcom`). La chaîne de connexion
reste `Trusted_Connection=True` — aucun mot de passe n'apparaît jamais dans `appsettings.json`.

### 2.2 Désactiver l'accès réseau distant à SQL Server

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

### 2.3 Ce qui reste hors de portée

Un utilisateur avec un accès admin Windows sur la machine du client pourra toujours, en théorie,
retrouver le mot de passe du compte `svc_t4cgestcom`, désactiver le pare-feu local, ou réactiver
TCP/IP. Ces mesures visent un attaquant opportuniste ou un utilisateur non-admin de la machine —
pas un administrateur système déterminé de sa propre machine. C'est une limite inhérente à tout
déploiement où le client héberge sa propre infrastructure.
