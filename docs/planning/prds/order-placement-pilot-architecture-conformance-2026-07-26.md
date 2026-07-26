---
prd: order-placement-pilot
reviewedAt: 2026-07-26
mode: conformance
priorApprovedDesign: none
fallback: "No /ArchitectureReview design-mode doc was ever produced for this PRD (built before the command existed). Per architecture-review.md's error-handling rule, diffed directly against the PRD's stated acceptance criteria (order-placement-pilot.md, current revision) instead of a prior approved design."
codeScanned:
  - src/OrderPilot.Api (all controllers, services, entities, authorization, Program.cs)
  - tests/OrderPilot.Api.UnitTests
  - tests/OrderPilot.Api.IntegrationTests
relatedReports:
  - docs/planning/prds/order-placement-pilot-validation-implementability-2026-07-26.md
---

# Architecture Review: order-placement-pilot
Mode: conformance (fallback — no prior approved architecture doc; diffed against PRD acceptance criteria directly)

## Overall Verdict

**No drift found.** All five PP-00x acceptance criteria, as currently written in `order-placement-pilot.md`, match the implementation in `src/OrderPilot.Api`. PP-001 was previously corrected (2026-07-26 revision) to describe the Admin-managed, no-supplier-integration design that was actually built, and this review confirms that fix now holds cleanly against the code. No new drift was found on PP-002 through PP-005 beyond the single already-known open item (PP-002's `[DRAFT]` 60-second latency target, which the PRD itself marks unconfirmed and which remains unconfirmed — not a divergence, since nothing in the code contradicts it).

## Requirement Traceability & Drift Summary

| ID | Design Element(s) | Status |
|----|--------------------|--------|
| PP-001 | `OrdersController.Create` → `OrderService.CreateOrderAsync` | No drift |
| PP-002 | `OrdersController.GetMine` / `GetById` (client-polled GET) | No drift (pre-existing DRAFT item, not new) |
| PP-003 | `AuthController.Login`, `ServiceCollectionExtensions.AddAppDataAndIdentity/AddAppAuthentication` | No drift |
| PP-004 | `Program.cs` (`UseHttpsRedirection`), `CreateOrderRequest` validation, `AuditService`/`AuditLog` | No drift |
| PP-005 | `AdminCustomersController`, `AdminSuppliersController`, `AdminConfigService` | No drift |

## Per-Requirement Detail

### PP-001 — Order creation and submission, Admin-managed status, no supplier integration

**Expected (PRD, current revision):** Customer completes order creation/submission in the web UI without manual/offline steps; order is persisted and visible to Admin; explicitly **no outbound call to the supplier's systems** — status advanced exclusively by Admin manual update.

**Actual:** `OrdersController.Create` (`src/OrderPilot.Api/Controllers/OrdersController.cs:25-41`) accepts the request and delegates to `OrderService.CreateOrderAsync`, which persists the order with `Status.Submitted` and resolves the single active supplier from the DB (`OrderService.cs:18-54`) — no HTTP client, webhook receiver, or any outbound integration code exists anywhere in `src/OrderPilot.Api` (confirmed by search for `HttpClient`/`webhook`/`IHttpClientFactory` — zero matches). Status only changes via `AdminOrdersController.UpdateStatus` (Admin-only, sequential-transition-enforced by `OrderService.UpdateOrderStatusAsync`). This matches the PRD's post-revision text exactly.

**Note on "web UI":** the AC also says "in the web UI." No frontend exists in this repo (`src/` contains only `OrderPilot.Api`, a Web API project) — this was already flagged in the prior implementability report as a separate open item (UI location/ownership unclear) and is not new. It is not treated as drift here because the PRD text does not claim the UI lives in this repository, but it remains an open question worth a PO/eng answer.

**Recommendation:** No PRD or code change needed for the supplier-integration claim — confirmed fixed. Carry forward the pre-existing open question about where the customer-facing web UI lives; that is unchanged by this review.

---

### PP-002 — Order status visible to customer via polling

**Expected (PRD):** Customer can view current status (submitted/accepted/fulfilled) via polling-based refresh; target status reflects backend state within 60 seconds of a change — marked `[DRAFT — confirm with PO]`.

**Actual:** `OrdersController.GetMine` and `GetById` (`OrdersController.cs:43-67`) return current order status on demand with no rate limiting, caching, or fixed refresh interval in the backend — polling cadence is entirely a property of whatever client calls this API. `OrderStatus` enum (`Domain/Entities/OrderStatus.cs`) models `Submitted/Accepted/Fulfilled`-style sequential states, matching the AC's example values. No code contradicts the 60-second target, but nothing in the backend enforces or verifies it either, since there is no polling client in this repository to measure against.

**This is not new drift** — the PRD itself marks this value `[DRAFT]` and unconfirmed, and the prior implementability report (2026-07-26) already identified that the target moved downstream to a not-yet-built client rather than being resolved. Re-confirmed here: still open, still not contradicted by code, still not verifiable from this repo alone.

**Recommendation:** Confirm the 60-second target with the PO before any polling client is built against this API (carried forward from the prior report — no new action needed from this review).

---

### PP-003 — Basic authenticated access

**Expected (PRD):** Customer authenticates via username/password or a stubbed session (explicitly not full SSO/OIDC).

**Actual:** `AuthController.Login` (`Controllers/AuthController.cs:22-51`) validates credentials via `UserManager<ApplicationUser>.CheckPasswordAsync` (ASP.NET Identity, hashed password storage) and issues a JWT via `TokenService`. `ServiceCollectionExtensions.AddAppDataAndIdentity`/`AddAppAuthentication` (`Extensions/ServiceCollectionExtensions.cs:15-57`) wire up Identity + JWT bearer auth with role claims (`Customer`/`Admin`). No stubbed-session code path exists anywhere. No SSO/OIDC integration exists, matching the "explicitly not full SSO/OIDC" exclusion. Directly exercised by `tests/OrderPilot.Api.IntegrationTests/RoleGatingTests.cs` (401 for anonymous, 403 for wrong role) and `AdminOrderWorkflowTests.InactivePilotCustomer_CannotLogIn`.

**Verdict:** No drift. Real auth is the stronger of the PRD's two stated options and is fully implemented and tested.

---

### PP-004 — Baseline security hygiene

**Expected (PRD):** TLS in transit for all requests; parameterized queries / input validation on order submission; basic audit log entry (who/what/when) for order creation and status changes.

**Actual:**
- **TLS:** `Program.cs:49` calls `app.UseHttpsRedirection()` — a hosting-config-level control, as the enrichment predicted.
- **Parameterized queries:** All data access goes through EF Core LINQ (`ApplicationDbContext`, `OrderService`, `AdminConfigService`) — inherently parameterized, no raw SQL concatenation anywhere in the scanned code.
- **Input validation:** `CreateOrderRequest` (`Dtos/Orders/CreateOrderRequest.cs`) uses `[Required]`, `[StringLength]`, `[Range(1, int.MaxValue)]` data-annotation validation, enforced by ASP.NET model binding. Verified by `AdminOrderWorkflowTests.CreateOrder_MalformedRequest_Returns400`, which confirms a 400 for missing `ProductDescription`/non-positive `Quantity`.
- **Audit log (who/what/when):** `AuditLog` entity (`Domain/Entities/AuditLog.cs`) records `UserId`, `Action`, `EntityType`, `EntityId`, `Details`, `TimestampUtc`. `AuditService.Log` is called from both `OrderService.CreateOrderAsync` (order creation) and `OrderService.UpdateOrderStatusAsync` (status changes), matching the AC's "order creation and status changes" scope exactly. Verified by `AuditServiceTests` (unit) and `AdminOrderWorkflowTests.Admin_UpdatesOrderStatus_ProducesCorrectAuditRow` (integration, asserts the acting user ID and the `"Submitted -> Accepted"` transition text land in the audit row).

**Verdict:** No drift. All three sub-claims implemented and tested as stated.

---

### PP-005 — Configurable pilot cohort and supplier

**Expected (PRD):** Admin can designate which customer accounts and which single supplier are active in the pilot without a code change.

**Actual:** `AdminCustomersController` (`Controllers/Admin/AdminCustomersController.cs`) exposes `GET/POST/PATCH` for customer accounts, including `IsPilotActive` toggling. `AdminSuppliersController` (`Controllers/Admin/AdminSuppliersController.cs`) exposes `GET/POST/PATCH` for suppliers, with `AdminConfigService.CreateSupplierAsync`/`UpdateSupplierAsync` enforcing a single-active-supplier invariant (`DeactivateAllSuppliersAsync`) entirely through data, not code. `DbSeeder` (`Data/Seed/DbSeeder.cs`) only seeds roles and an optional dev/local Admin account from configuration — no customer or supplier is hardcoded anywhere in source. Verified by `AdminConfigServiceSupplierTests` (duplicate-code rejection, single-active-supplier enforcement) and `AdminOrderWorkflowTests.Admin_ActivatingSupplier_DeactivatesPreviouslyActiveSupplier`. Both admin routes are gated `[Authorize(Roles = "Admin")]` and covered by `RoleGatingTests`.

**Verdict:** No drift. Fully implemented, no code-change-required violations found.

## Test Evidence Cross-Check

Reconfirmed test coverage cited in the prior implementability report is current and accurate: `OrderIsolationTests` (customer/customer and customer/admin isolation, 404-not-403 design on `OrdersController.GetById` + `OrderOwnerAuthorizationHandler`), `RoleGatingTests` (401/403 boundaries), `AdminOrderWorkflowTests` (status transition, audit row content, supplier single-active invariant, inactive-customer login block, input validation), `AuditServiceTests`, `AdminConfigServiceSupplierTests`, `OrderServiceTests`, `OrderOwnerAuthorizationHandlerTests`. No test asserts or requires supplier-side outbound integration — consistent with PP-001 as currently written.

## Risks

- LOW: PP-001's "in the web UI" phrase is unverifiable from this repository alone (no frontend present). Not scored as drift since the PRD doesn't claim the UI lives here, but worth a PO/eng confirmation for completeness of the pilot as a whole.
- LOW: PP-002's 60-second target remains unconfirmed and unverifiable until a polling client exists. Carried forward, not new.

## Recommendation

No PRD updates and no code fixes required as a result of this conformance check. The 2026-07-26 PP-001 revision is confirmed to hold against the actual implementation and its test suite. The two residual open items (web UI location, PP-002 latency target) are pre-existing and already tracked in `order-placement-pilot-validation-implementability-2026-07-26.md` — this review found no additional drift beyond those.
