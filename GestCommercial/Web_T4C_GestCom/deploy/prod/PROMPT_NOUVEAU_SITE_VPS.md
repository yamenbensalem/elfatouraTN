# Prompt — Ajouter un nouveau site sur le VPS partagé

Copie-colle ce prompt à Claude quand tu veux déployer un nouveau projet sur le VPS OVH.

---

## PROMPT À UTILISER

```
Je veux déployer un nouveau site/application sur mon VPS OVH.
Le VPS héberge déjà plusieurs sites. Voici l'architecture existante que tu DOIS respecter.

---

## Architecture VPS actuelle (NE PAS MODIFIER)

**Serveur** : vps-bf0b3440.vps.ovh.net  
**User SSH** : ubuntu  
**Base path** : /home/ubuntu/docker/

**Infrastructure GLOBALE partagée (NE PAS RECRÉER) :**
- `nginx-proxy` — container nginxproxy/nginx-proxy, gère le routing HTTP/HTTPS pour TOUS les sites
  - Bind mount vhost.d → /home/ubuntu/docker/nginx/vhost.d/
  - Bind mount certs    → /home/ubuntu/docker/nginx/certs/
  - Bind mount html     → /home/ubuntu/docker/nginx/html/
- `nginx-letsencrypt` — container nginxproxy/acme-companion, émet les certificats SSL pour TOUS les domaines
- Ces deux containers sont connectés à tous les réseaux applicatifs via `docker network connect`

**Sites déjà en production :**
- tunisiaauto.tn → réseau ntw_salecars_prod, containers: backend-api, frontend-web, salecars-prod-sqlserver
- gestioncom.tijaraflow.fr → réseau ntw_gestcom_prod, containers: gestcom-app, gestcom-sqlserver

**Règle d'or :** Chaque nouveau site a son propre réseau Docker isolé. nginx-proxy et nginx-letsencrypt sont connectés à CE réseau via `docker network connect` (pas de nouveau proxy).

---

## Règles OBLIGATOIRES pour les fichiers de déploiement

### docker-compose.infra.yml
Ne contient QUE la définition du réseau. PAS de nginx-proxy ni acme-companion dedans.

```yaml
networks:
  ntw_MONAPP_prod:
    name: ntw_MONAPP_prod
    driver: bridge
```

### docker-compose.app.yml
Le container app DOIT avoir ces variables d'environnement pour être découvert par nginx-proxy :

```yaml
environment:
  - VIRTUAL_HOST=${APP_DOMAIN}
  - VIRTUAL_PORT=<port_interne>
  - LETSENCRYPT_HOST=${APP_DOMAIN}
  - LETSENCRYPT_EMAIL=${LETSENCRYPT_EMAIL}
```

Et le réseau doit être `external: true` :

```yaml
networks:
  ntw_MONAPP_prod:
    name: ntw_MONAPP_prod
    external: true
```

### vhost.d/<mon-domaine>
Si le site utilise WebSocket (Blazor Server, Socket.io, etc.), le fichier vhost DOIT utiliser
`$http_upgrade` et NON `$connection_upgrade` (cette variable n'est pas disponible sur notre nginx-proxy) :

```nginx
# CORRECT ✅
proxy_http_version 1.1;
proxy_set_header Upgrade $http_upgrade;
proxy_set_header Connection $http_upgrade;

# INTERDIT ❌ — crashe nginx
proxy_set_header Connection $connection_upgrade;
```

### deploy_to_vps.ps1
Le script DOIT :
1. Dans `Setup-Network` : créer le réseau, puis connecter nginx-proxy et nginx-letsencrypt au nouveau réseau via `docker network connect`
2. Dans `Sync-ConfigFiles` / `Inject-VhostOverride` : utiliser `docker cp` pour injecter le vhost override (bind mount, pas de volume nommé)
3. Après injection vhost : envoyer `docker kill --signal=HUP nginx-proxy` pour recharger nginx sans redémarrage
4. Valider le fichier vhost avant injection (refuser si `$connection_upgrade` est présent)
5. NE PAS démarrer de nouveau nginx-proxy (port 80/443 déjà pris)

---

## Ce que je veux déployer

**Nom du projet** : <NOM>
**Domaine** : <sous-domaine>.tijaraflow.fr (ou autre domaine)
**Stack** : <ex: Blazor Server .NET 8 / Node.js / Django / ...>
**Base de données** : <ex: SQL Server 2022 / PostgreSQL / aucune>
**Port interne de l'app** : <ex: 8080>
**Path VPS** : /home/ubuntu/docker/<nom>

**Fichiers à créer :**
- deploy/prod/.env.example
- deploy/prod/docker-compose.infra.yml  (réseau uniquement)
- deploy/prod/docker-compose.app.yml    (application)
- deploy/prod/docker-compose_sql_prod.yml (si DB)
- deploy/prod/vhost.d/<domaine>         (si WebSocket)
- deploy/prod/deploy_to_vps.ps1         (script d'orchestration)
- deploy/prod/DEPLOY.md                 (documentation)
- Dockerfile (si build local nécessaire)
- .dockerignore

Génère tous ces fichiers en respectant les contraintes ci-dessus.
```

---

## Checklist avant de lancer le déploiement

Après avoir reçu les fichiers générés, vérifie AVANT d'exécuter :

- [ ] `docker-compose.infra.yml` ne contient PAS de service `nginx-proxy` ou `acme-companion`
- [ ] Les réseaux dans tous les compose sont `external: true` (sauf dans `infra.yml`)
- [ ] Le fichier `vhost.d/<domaine>` utilise `$http_upgrade` et **non** `$connection_upgrade`
- [ ] Le `deploy_to_vps.ps1` contient `docker network connect ntw_XXX nginx-proxy`
- [ ] Le `deploy_to_vps.ps1` contient `docker network connect ntw_XXX nginx-letsencrypt`
- [ ] Le `.env.example` contient `LETSENCRYPT_EMAIL`
- [ ] Le `Dockerfile` est présent si l'image est buildée localement

---

## Séquence de déploiement (rappel)

```powershell
# 1. SSH key (une seule fois par machine)
.\deploy_to_vps.ps1 -Action fix-ssh

# 2. Remplir deploy\prod\.env depuis .env.example

# 3. Déploiement complet
.\deploy_to_vps.ps1

# Si le vhost doit être réinjecté après coup
.\deploy_to_vps.ps1 -Action fix-vhost
```

---

## Infos utiles sur le VPS

```bash
# État général
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'

# Voir les réseaux
docker network ls

# Connecter manuellement nginx-proxy à un réseau (si le script rate)
docker network connect ntw_MONAPP_prod nginx-proxy
docker network connect ntw_MONAPP_prod nginx-letsencrypt

# Voir les logs nginx (erreurs de config)
docker logs nginx-proxy --tail 30

# Recharger nginx après modif vhost (sans redémarrer)
docker kill --signal=HUP nginx-proxy

# Inspecter les mounts du proxy (pour trouver le vhost.d bind mount)
docker inspect nginx-proxy --format '{{range .Mounts}}{{.Source}} -> {{.Destination}}{{"\n"}}{{end}}'
```
