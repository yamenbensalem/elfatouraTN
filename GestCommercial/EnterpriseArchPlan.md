**Section A: Final recommended architecture**

1. Identity and tenant model
1. Introduce two account scopes in utilisateurs:
1. SuperAdmin account: IsSuperAdmin = true, CompanyId = null.
1. Tenant account: IsSuperAdmin = false, CompanyId required.
1. Keep one database and one set of business tables.
1. Add a strict TenantContext per request/circuit:
1. For tenant users: TenantContext.CompanyId = user CompanyId.
1. For SuperAdmin: TenantContext has no company until explicitly selecting one enterprise for delegated actions.
1. Deny-by-default if tenant scope is required and missing.

2. Role and permission boundaries
1. Global role:
1. SuperAdmin only, global scope, no tenant business access by default.
1. Enterprise roles per company:
1. Admin, Manager, Employe.
1. Same permission catalog, but assignments are tenant-scoped.
1. Permission evaluation rule:
1. Allow only if actor has permission and actor tenant equals resource tenant.
1. SuperAdmin exception only on enterprise-management and global-security endpoints, not generic business CRUD by default.

3. Data isolation model (single database)
1. Add CompanyId to all tenant-owned business entities.
1. Start with: client, fournisseur, produit.
1. Add to sales docs: devis, devis lines, commandes vente, commandes vente lines, bons livraison, bons livraison lines, factures client, factures client lines, reglements.
1. Add to purchase docs: commandes achat, lignes, bons reception, lignes, factures fournisseur, lignes.
1. Add to activity/audit tables (including existing journalactivite).
1. Keep global reference tables without CompanyId only if truly shared and safe (ex: permission catalog, VAT master if intentionally shared).
1. Use composite integrity to prevent cross-tenant relations:
1. Child FK should include CompanyId + parent key.
1. Example pattern: child (CompanyId, ParentId) references parent (CompanyId, Id).

4. Auth/RBAC schema updates
1. utilisateurs:
1. Add IsSuperAdmin bit not null default false.
1. Add SecurityStamp uniqueidentifier/string.
1. Add PermissionsVersion int not null default 1.
1. Add LastRoleChangeUtc datetime2.
1. Keep CompanyId nullable only for SuperAdmin.
1. app_role:
1. CompanyId required for tenant roles.
1. Only SuperAdmin role can be global.
1. user_role:
1. Include CompanyId to enforce same-tenant assignment.
1. Unique index on (CompanyId, UserId, RoleId).
1. role_permission:
1. Include CompanyId if roles are tenant-specific, with unique (CompanyId, RoleId, PermissionId).
1. Constraints/indexes:
1. Unique role name per tenant: (CompanyId, RoleName).
1. Unique business code per tenant: (CompanyId, CodeX).
1. Nonclustered index on CompanyId for every tenant table.
1. Check constraint: IsSuperAdmin true implies CompanyId null; false implies CompanyId not null.

5. Authentication and claims
1. Required claims in cookie:
1. sub (UserId), login, is_superadmin, tenant_id (for tenant users), roles, permissions_version, security_stamp, session_id.
1. Company selection/login behavior:
1. Tenant users log in directly to their tenant (single tenant claim).
1. SuperAdmin logs in globally, then chooses enterprise context explicitly for delegated operations.
1. Session invalidation when roles/permissions change:
1. Use cookie validation hook to compare security_stamp and permissions_version from DB.
1. If mismatch: reject principal and force re-authentication.

6. Authorization enforcement design
1. Policies:
1. SuperAdminOnly policy.
1. TenantScoped policy (requires tenant_id claim and non-superadmin).
1. Permission policies (perm:feature.action) plus tenant check.
1. Service guards (mandatory for backend services):
1. EnsureAuthenticated.
1. EnsureTenantAccess(targetCompanyId).
1. EnsurePermission(permission, targetCompanyId).
1. Endpoint and service behavior:
1. Never rely on UI visibility.
1. Apply company filter before materializing data.
1. Return generic not found/forbidden responses to avoid tenant enumeration.

7. Audit model (highly auditable SuperAdmin)
1. Add a dedicated append-only security audit table:
1. EventId, TimestampUtc, ActorUserId, ActorIsSuperAdmin, ActorCompanyId, Action, TargetType, TargetId, TargetCompanyId, Outcome, ReasonCode, CorrelationId, Ip, UserAgent.
1. Mandatory audited actions:
1. Enterprise create/update/disable.
1. Role/permission changes.
1. User create/disable/password reset.
1. Tenant context switch by SuperAdmin.
1. Authentication success/failure for privileged accounts.
1. Alert points:
1. Repeated forbidden cross-tenant attempts.
1. SuperAdmin actions outside business hours (optional).
1. Rapid role elevation patterns.

---

**Section B: Step-by-step implementation plan**

1. Phase 1: Foundation and feature flags
1. Add feature flag MultiTenantEnforcement.
1. Add TenantContext service and request/circuit initialization.
1. Dependency: none.
1. Risk: accidental partial enforcement if mixed old/new paths.
1. Mitigation: keep enforcement off by default until phase 4.

2. Phase 2: Schema expansion (backward compatible)
1. Add CompanyId columns nullable initially on tenant business tables.
1. Add IsSuperAdmin, SecurityStamp, PermissionsVersion to utilisateurs.
1. Add new indexes (CompanyId, key), and supporting role/user indexes.
1. Dependency: phase 1.
1. Risk: migration duration.
1. Mitigation: online index creation where available and batched operations.

3. Phase 3: Seed and role model
1. Seed global SuperAdmin role and permission catalog idempotently.
1. Add tenant role templates for Admin, Manager, Employe.
1. Implement enterprise role provisioning service.
1. Dependency: phase 2.
1. Risk: duplicate role seeds.
1. Mitigation: unique constraints and idempotent upsert.

4. Phase 4: Backend authorization hardening
1. Add service guard checks to all business services (create/read/update/delete).
1. Add tenant filter in all queries.
1. Add superadmin exception rules only for enterprise/security modules.
1. Dependency: phases 1-3.
1. Risk: missed service path.
1. Mitigation: static analysis checklist and integration tests.

5. Phase 5: Enterprise onboarding flow
1. Create EnterpriseProvisioningService:
1. Transaction: create company, seed roles, create initial tenant admin, assign role, write audit.
1. Idempotency key support to prevent duplicate enterprises.
1. Outbox event for notifications after commit.
1. Dependency: phases 2-4.
1. Risk: partial provisioning.
1. Mitigation: single transaction + idempotency state table.

6. Phase 6: Authentication claim refresh and session revocation
1. Add cookie validation against SecurityStamp and PermissionsVersion.
1. On role/permission change, increment PermissionsVersion and rotate SecurityStamp.
1. Dependency: phase 2.
1. Risk: DB overhead on validation.
1. Mitigation: short cache window and bounded validation interval.

7. Phase 7: UI adaptation
1. Add SuperAdmin enterprise management screens.
1. Add top-level enterprise selector for SuperAdmin delegated context.
1. Keep all backend checks authoritative.
1. Dependency: phases 4-6.
1. Risk: UI suggests access not actually allowed.
1. Mitigation: always read effective permissions from backend.

8. Phase 8: Security observability and runbooks
1. Add audit dashboards and alerts.
1. Add incident response runbook for suspected tenant boundary breach.
1. Dependency: phases 4-7.
1. Risk: noisy alerts.
1. Mitigation: threshold tuning and severity levels.

---

**Section C: Migration and rollout plan**

1. Pre-rollout
1. Create a default company for legacy records.
1. Map all existing users and business data to default company.
1. Create one SuperAdmin bootstrap account.

2. Expand deployment (no downtime)
1. Deploy schema with nullable CompanyId and new auth columns.
1. Deploy app version that writes CompanyId for new records and reads null as default company.

3. Backfill
1. Backfill CompanyId in batches by table.
1. Verify row counts and null counts per table after each batch.
1. Backfill role mappings per company.

4. Enforce
1. Turn on MultiTenantEnforcement feature flag.
1. Switch authorization guards to strict mode.
1. Apply not-null constraints on CompanyId for tenant tables.
1. Add final composite FK and unique constraints.

5. Cutover validation
1. Validate no cross-tenant reads in logs.
1. Validate SuperAdmin-only enterprise management.
1. Validate tenant admins cannot see other enterprise users/data.

6. Rollback strategy
1. Keep previous app binary available.
1. Keep compatibility mode for null CompanyId until enforcement completion.
1. Roll back feature flag first, then app, then optional schema rollback if needed.

---

**Section D: Security risks and mitigations**

1. Horizontal privilege escalation via direct ID access
1. Mitigation: every service method enforces target CompanyId match before action.

2. Missing tenant filter in one query path
1. Mitigation: mandatory service guard pattern plus integration tests for every module.

3. Role spoofing via stale cookie
1. Mitigation: SecurityStamp and PermissionsVersion validation on cookie principal.

4. SuperAdmin overreach into business data
1. Mitigation: explicit allowlist of SuperAdmin endpoints; require delegated tenant context for tenant operations.

5. Cross-tenant FK contamination
1. Mitigation: composite FK with CompanyId on parent-child relations.

6. Tenant/user enumeration
1. Mitigation: generic error responses; no detailed existence hints for unauthorized targets.

7. Weak audit trail
1. Mitigation: append-only security audit table with actor, target, outcome, context metadata.

8. Excessive permissions in default roles
1. Mitigation: least-privilege role templates, deny-by-default for new permissions.

9. Privileged action tampering
1. Mitigation: correlation IDs, signed session identifiers, immutable audit records.

10. Onboarding duplicates and race conditions
1. Mitigation: idempotency key + unique company slug/name constraints + transactional provisioning.

---

**Section E: Test matrix**

1. Unit tests
1. Service guard denies when actor tenant differs from resource tenant.
1. Service guard allows correct tenant permission.
1. SuperAdmin allowed only on enterprise management policy.
1. PermissionsVersion mismatch invalidates principal.
1. SecurityStamp mismatch invalidates principal.

2. Integration tests
1. Tenant A user creates data, Tenant B user cannot read/update/delete it.
1. Tenant Admin manages only users in same company.
1. Role assignment rejects cross-tenant user-role combinations.
1. Enterprise provisioning creates company, roles, admin, and audit records atomically.
1. Permission changes force re-authentication for affected user.

3. Cross-tenant negative tests (must-pass blockers)
1. Read by foreign key ID from another tenant returns forbidden or not found.
1. List/search endpoints never return another tenant rows.
1. Bulk export endpoints scoped by CompanyId only.
1. Print/report endpoints scoped by CompanyId only.
1. Background jobs process tenant partitioned data correctly.

4. Security/audit tests
1. Every privileged action writes one audit record with actor and target.
1. Failed forbidden attempts are audited with reason code.
1. SuperAdmin tenant-switch events are audited.
1. Login failures for privileged accounts are audited.

5. Performance/regression tests
1. Query latency with CompanyId indexes under expected load.
1. Cookie validation overhead remains acceptable.
1. Migration backfill execution time within maintenance window.

---

**Section F: Assumptions**

1. Non-superadmin users belong to exactly one enterprise at a time.
2. Existing company table is authoritative for tenant identity.
3. Existing role model can be evolved to tenant-scoped role assignments.
4. Current business entities can accept CompanyId without changing functional behavior.
5. SQL Server edition/environment supports planned indexing strategy (or equivalent maintenance window exists).
6. Current deployment can use feature flags for staged enforcement.
7. Existing audit table can be extended or a new security audit table can be added.
8. SuperAdmin is intended for platform administration, not day-to-day tenant business transactions by default.

