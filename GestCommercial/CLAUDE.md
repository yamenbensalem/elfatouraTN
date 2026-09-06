# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Scope of this directory

`GestCommercial/` is a container for one active application plus its delivery/planning artifacts —
it is not itself a buildable project. There is no `T4C_Commercial_Project` WinForms app in this
folder (an earlier version of this file described one; that project does not exist here — ignore
any prior reference to it).

| Path | What it is |
|---|---|
| `Web_GestCom/` | **The active app** — Blazor Server, .NET 8, EF Core, SQL Server. Has its own [CLAUDE.md](Web_GestCom/CLAUDE.md) — read that before making any code change, it documents build/test commands, layered architecture, document numbering, and stock/payment logic in detail. |
| `Web_GestCom.Tests/` | xUnit + bUnit + Moq test project for the app above. Run via `dotnet test` from inside `Web_GestCom/` (see that project's CLAUDE.md for exact commands). |
| `Web_GestCom/deploy/prod/` | Docker/VPS deployment for the live instance at `http://gestioncom.tijaraflow.fr/`. See `DEPLOY.md` there; follows the shared OVH VPS conventions from the user's global instructions (nginx-proxy + acme-companion already running, networks declared `external: true`, `$http_upgrade` not `$connection_upgrade` in vhost config). |
| `__Delivery/` | Packaged releases handed off to the client (zips, per-version folders with `DEPLOIEMENT.md`/`RELEASE_NOTES.md`, and a separate demo deployment under `demos.aivorconsulting/`). Historical artifacts — don't treat as source of truth for current app behavior. |
| `EnterpriseArchPlan.md`, `PROJECT_CODE_ANALYSIS_SUMMARY.md`, `PlanPourDev.md`, `Plan_Test_Manuel_GestCom.docx` | Planning/analysis documents at various points in the project's history. May be stale — verify against actual code in `Web_GestCom/` before relying on them. |
| `.github/agents/app-pattern-analyst.agent.md` | A read-only subagent definition scoped to `Web_GestCom` for tracing architecture/data-flow patterns before implementing a change in that style. |

## Where to actually work

Almost all code changes happen inside `Web_GestCom/`. `cd` there and follow
[Web_GestCom/CLAUDE.md](Web_GestCom/CLAUDE.md) — it has the build/run/test commands,
the Components → Services → AppDbContext → Models layering, the document numbering table,
stock/payment computation rules, and the French domain glossary. This file only exists to
orient you at the parent-directory level and prevent confusion with stale/unrelated content.

## VPS deployment conventions

When touching anything under `Web_GestCom/deploy/`, follow the shared OVH VPS Docker rules
from the user's global `~/.claude/CLAUDE.md`: `nginx-proxy` and `nginx-letsencrypt` are
pre-existing global containers owning ports 80/443 (never recreate them), only
`docker-compose.infra.yml` defines the app's network (as a plain bridge network, not external),
every other compose file declares that network as `external: true`, and vhost WebSocket config
must use `$http_upgrade` (never `$connection_upgrade`).
