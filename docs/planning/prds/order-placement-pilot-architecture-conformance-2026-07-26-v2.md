---
prd: order-placement-pilot
reviewedAt: 2026-07-26
mode: conformance
baseline: docs/planning/prds/order-placement-pilot-architecture.md (status APPROVED)
baselineNote: >
  This run diffs against the approved architecture doc's specific ADR decisions and
  requirement-traceability rows directly — it does NOT use the PRD-acceptance-criteria
  fallback path. That fallback was used by the prior conformance report
  (order-placement-pilot-architecture-conformance-2026-07-26.md, no "-v2" suffix)
  because no approved architecture doc existed yet at that time. An approved doc now
  exists, so per architecture-review.md's checkConformance step 1 ("Load the approved
  architecture doc and ADRs for the PRD, if one exists"), this is the real baseline.
  The prior report is left unmodified — it documents a different baseline and remains
  valid for what it checked (PRD ACs vs. code), not superseded by this one.
supplementaryCheck: docs/planning/prds/order-placement-pilot-lld.md (per-module diagrams
  cross-checked against code for the same class of drift, per this run's instructions)
codeScanned:
  - src/OrderPilot.Api (all Controllers, Services, Domain/Entities, Authorization, Program.cs, Extensions, Data)
  - tests/OrderPilot.Api.UnitTests
  - tests/OrderPilot.Api.IntegrationTests
---

# Architecture Review: order-placement-pilot
Mode: conformance (baseline = approved architecture doc, not the PRD-acceptance-criteria fallback)

## Overall Verdict

**No drift found.** All 6 ADRs (ADR-001 through ADR-006) in `order-placement-pilot-architecture.md` were re-verified independently against the actual source in `src/OrderPilot.Api` — not taken on the doc's word — and every decision described is still exactly what's implemented. All 5 traceability rows (PP-001–PP-005) were likewise re-checked against the cited design elements and hold. The LLD (`order-placement-pilot-lld.md`) was spot-checked against the same code for the same class of drift (diagram claims the code doesn't actually do) and found consistent, including its recent scoped `AuthController` re-run.

One gap was found, but it is a **baseline documentation-completeness gap, not code drift**: `order-placement-pilot-architecture.md` itself contains no High-Level Design (HLD) Mermaid diagram — it goes directly from Approval Status to Architecture Design/ADRs/Traceability. The HLD that exists is in the **parent** PRD's architecture doc (`supply-chain-solutions-for-logistics-architecture.md:26`), not this one. The `architecture-reviewer` agent's own output contract states an HLD is "produced in every `design` and `conformance` run" — this PRD's approved `design`-mode doc never got one. This is noted for completeness per this run's instructions; it does not itself constitute drift against the doc (there's no HLD claim to diff code against), and conformance mode does not gate on it.

## Per-ADR Conformance Summary

| ADR | Decision | Re-verified against | Status |
|---|---|---|---|
| ADR-001 | ASP.NET Core (.NET 8) + EF Core + SQL Server/Azure SQL | `ServiceCollectionExtensions.AddAppDataAndIdentity` (`UseSqlServer`), all data access via EF Core LINQ (`OrderService`, `AdminConfigService`, no raw SQL anywhere) | Matches |
| ADR-002 | Real ASP.NET Core Identity + JWT bearer auth, no stubbed session | `AuthController.Login` (`UserManager.CheckPasswordAsync`), `TokenService.CreateToken` (HMAC-SHA256, `sub`/`NameIdentifier`/`Role`/`jti` claims), `ServiceCollectionExtensions.AddAppAuthentication`. No session/cookie/stub code path found anywhere in `src/OrderPilot.Api` | Matches |
| ADR-003 | No supplier-side integration of any kind; Admin-managed sequential status only | Zero matches for `HttpClient`/`webhook`/`IHttpClientFactory` anywhere in `src/OrderPilot.Api` (re-ran the search independently). `OrderService.UpdateOrderStatusAsync` enforces `(int)newStatus != (int)order.Status + 1` → `InvalidStatusTransitionException` → 409, confirmed by `AdminOrderWorkflowTests.Admin_SkippingStatusTransition_Returns409` | Matches |
| ADR-004 | Three-layer authorization: role gate + resource-based ownership policy + query-level scoping | `[Authorize(Roles="Customer")]` on `OrdersController` / `[Authorize(Roles="Admin")]` on `Admin*Controller`s (Layer 1); `OrderOwnerAuthorizationHandler` + `Policies.OrderOwnerOrAdmin` evaluated in `OrdersController.GetById` (Layer 2); `OrderService.GetOrdersForCustomerAsync` filters `Where(o => o.CustomerId == customerId)` at the query (Layer 3). All three independently confirmed in code, plus `RoleGatingTests` and `OrderIsolationTests` pass-through logic re-read line by line | Matches |
| ADR-005 | Cross-customer order lookup returns 404, not 403 | `OrdersController.cs:62`, verbatim comment `// 404, not 403 — avoids confirming existence of another customer's order.`; both the null-order branch and the failed-authorization branch return `NotFound()`. `OrderIsolationTests.CustomerB_CannotGetCustomerA_OrderById` asserts `HttpStatusCode.NotFound` specifically | Matches |
| ADR-006 | Single-active-supplier invariant enforced via EF Core change-tracker, not a DB constraint | `AdminConfigService.DeactivateAllSuppliersAsync` loads all `IsActive` suppliers and flips them in-memory, staged in the same unit of work as the caller's `SaveChangesAsync` (both `CreateSupplierAsync` and `UpdateSupplierAsync` call it before their own save). No unique filtered index or DB-level constraint found in `Migrations/20260726083142_InitialCreate.cs` or the model snapshot. Confirmed by `AdminConfigServiceSupplierTests` and `AdminOrderWorkflowTests.Admin_ActivatingSupplier_DeactivatesPreviouslyActiveSupplier` | Matches |

## Requirement Traceability Re-Verification

| Requirement ID | Design Element (per architecture doc) | Re-verified | Status |
|---|---|---|---|
| PP-001 | `OrdersController.Create` → `OrderService.CreateOrderAsync` (persists order, resolves single active supplier, no outbound call — ADR-003) | Read `OrdersController.cs:25-41` and `OrderService.cs:18-54` directly; customer `IsPilotActive` gate and single-active-supplier resolution both present exactly as described | Satisfied |
| PP-002 | `OrdersController.GetMine`/`GetById`, `AdminOrdersController.GetAll` (client-polled GET) | Read all three actions; no rate limit, cache, or fixed interval in the backend, consistent with "client-polled" framing. 60-second target remains `[DRAFT]` in the PRD — unchanged, not a traceability failure | Satisfied |
| PP-003 | `AuthController.Login`, `ServiceCollectionExtensions.AddAppDataAndIdentity`/`AddAppAuthentication`, `TokenService` | Read all three; Identity + JWT wiring confirmed, no SSO/OIDC code found | Satisfied |
| PP-004 | `Program.cs` `UseHttpsRedirection`; EF Core LINQ; `CreateOrderRequest` validation; `AuditService`/`AuditLog` from both create and status-change paths | `Program.cs:49` confirmed; `CreateOrderRequest` has `[Required]`/`[StringLength]`/`[Range(1,int.MaxValue)]`; `AuditService.Log` called from both `OrderService.CreateOrderAsync` and `UpdateOrderStatusAsync` | Satisfied |
| PP-005 | `AdminCustomersController`, `AdminSuppliersController`, `AdminConfigService`, `DbSeeder` (seeds only roles + optional dev Admin) | Read all four; `DbSeeder.SeedAsync` seeds only `Admin`/`Customer` roles and an optional config-driven dev Admin — no pilot customer or supplier is hardcoded anywhere | Satisfied |

## Drift Findings

```json
{
  "prd": "order-placement-pilot",
  "driftFindings": []
}
```

No drift findings against the approved architecture doc's ADRs or traceability rows. Every ADR decision and every PP-00x design-element citation was independently re-derived from source (not taken on the doc's assertion) and matches.

## LLD-vs-Code Cross-Check (Supplementary)

Per this run's instructions, the LLD (`order-placement-pilot-lld.md`) was spot-checked for the same class of drift — a diagram claiming behavior the code doesn't actually have:

- **Module 1 (Domain/Entities):** class diagram fields for `ApplicationUser`, `Supplier`, `Order`, `OrderStatus`, `AuditLog` matched the actual entity files field-for-field, including `AuditLog`'s deliberate lack of a navigation property to `ApplicationUser` (drawn as a dependency `..>`, not an association — correct).
- **Module 2 / scoped `AuthController` re-run:** sequence diagram (email lookup → password check → role lookup → `IsPilotActive` gate → `TokenService.CreateToken`) matches `AuthController.cs:22-51` and `TokenService.cs:19-49` exactly, including the `AdminConfigService.CustomerRole` constant reference (not a hardcoded string). The scoped re-run's `AuthController` class diagram (constructor-injected `UserManager<ApplicationUser>` + `ITokenService`, single `Login` action) matches `AuthController.cs:13-20`.
- **Module 3 (OrdersController + OrderService):** both sequence diagrams (order creation, GetById 404-vs-403) match `OrdersController.cs` and `OrderService.cs` including the specific exception types (`InactiveCustomerException`, `NoActiveSupplierException`) and status codes.
- **Module 4 (AdminOrdersController + status update):** sequence diagram's forward-only transition check and audit-row wording (`"{prev} -> {new}"`) match `OrderService.UpdateOrderStatusAsync` verbatim.
- **Module 5 (AdminSuppliersController + AdminConfigService):** sequence diagram's atomic deactivate-then-activate flow matches `AdminConfigService.CreateSupplierAsync`/`DeactivateAllSuppliersAsync` exactly.
- **Module 6 (Authorization, ADR-004):** the three-layer flowchart matches the same code verified against ADR-004 above.

**No LLD-vs-code mismatch found.** Minor line-number citation drift exists in a couple of places (e.g., the architecture doc's ADR-006 cites `AdminConfigService.cs:71-125`, while the private `DeactivateAllSuppliersAsync` helper it describes actually runs through line 145 — the LLD's own citation, `71-144`, is the more accurate one) — this is a citation-precision nit, not a behavioral drift, and is not raised as a driftFinding since the described behavior itself is accurate in both documents.

## Recommendation

No PRD update and no code fix required — this run found no drift to recommend a side for. Two follow-ups worth tracking, neither blocking:
1. Regenerate or backfill an HLD Mermaid diagram into `order-placement-pilot-architecture.md` itself (currently absent; the equivalent HLD lives only in the parent PRD's architecture doc) — a `generateHLD` standalone run would close this without redoing the full design review.
2. The architecture doc's ADR-006 line citation (`AdminConfigService.cs:71-125`) could be tightened to `71-144` to match the LLD's more precise citation — cosmetic only.
