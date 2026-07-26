---
prd: order-placement-pilot
validatedAt: 2026-07-26
checklist: implementability
overallScore: 74
recheckType: focused-single-dimension-post-implementation
priorFullValidation:
  report: docs/planning/prds/order-placement-pilot-validation.md
  checklist: all
  overallScore: 77
  date: 2026-07-26
implementationRef: src/OrderPilot.Api
testEvidence: "31 passing tests (16 unit + 15 integration), including a dedicated customer-order-isolation authorization suite"
note: >
  This is a focused, single-dimension re-check (checklist=implementability only).
  It does NOT re-score Problem Statement, User Definition, or Security Considerations
  (those were scored in the prior "all" run) and does NOT change the PRD's frontmatter
  status, which reflects the prior full-checklist result.
---

# Validation Report: order-placement-pilot (Implementability-only re-check)

Checklist: **implementability** (per `checklistConfig` scope — not `all`; Problem Statement, User Definition, and Security Considerations are intentionally out of scope for this run and are unchanged from the prior report)

## Why this run exists

The PRD previously scored 77% overall on the `all` checklist, then went through `/PRDEnrich` (S/M complexity) and `/PRDFeasibility` (PROCEED, conditional). Since then, the pilot has actually been **built**: `src/OrderPilot.Api` is a working ASP.NET Core 8 Web API with 31 passing tests. This re-check inspects that implementation against the PRD's PP-001–PP-005 acceptance criteria to answer one question: does the PRD, as written, carry sufficient detail for engineering execution — and where the PRD left things ambiguous, did the real build resolve them, sidestep them, or leave them genuinely open?

## Implementability Score: 74/100

Per the scoring rubric (60-79 = "Significant gaps, needs clarification"). This sits at the upper end of that band: two of the three HIGH-severity ambiguities flagged pre-build were fully and testably resolved in code, but the PRD's single hardest integration point (PP-001's supplier fulfillment endpoint) was never specified and, as the code now confirms, was never actually built — it was quietly narrowed out of scope rather than resolved. That gap, plus the absence of any web UI in this repository, is what keeps the score out of the 80s.

### What the implementation confirms was resolved cleanly

- **PP-003 (authentication) — resolved, no longer ambiguous.** The PRD's AC offered two alternatives ("username/password or a stubbed session") and flagged this in the validation/enrichment/feasibility reports as a HIGH-severity, must-decide-before-backend-work item. The shipped code (`Controllers/AuthController.cs`, `Extensions/ServiceCollectionExtensions.cs`) commits unambiguously to real auth: ASP.NET Identity with hashed password storage, JWT bearer tokens, and role claims (`Customer`/`Admin`). No stubbed-session code path exists. This is the stronger of the two options the PRD left open, and it is exercised by integration tests (`RoleGatingTests.cs`).
- **Authorization boundary — resolved and directly tested.** The PRD/enrichment/feasibility reports all flagged as HIGH risk that no requirement specified whether one customer could see another's orders. The code closes this with `OrderOwnerAuthorizationHandler` + `Policies.OrderOwnerOrAdmin`, applied in `OrdersController.GetById`, and — notably — returns `404` rather than `403` on a denied cross-customer lookup to avoid confirming another customer's order exists (a detail the PRD never specified but that the implementation got right). `OrderIsolationTests.cs` directly proves customer A cannot fetch, list, or infer customer B's orders, and that Admin can see all orders. This was the single largest implementability risk in the PRD and it is now moot — closed, not just claimed.
- **PP-002 backend status-update mechanism — resolved by simplification.** The enrichment flagged three possible designs (supplier webhook, backend-polls-supplier, manual/admin update) as an unstated architectural decision. The build picked the simplest: status changes only occur via `AdminOrdersController.UpdateStatus` (Admin-only, sequential-transition-enforced). No scheduler, poller, or webhook receiver exists. This removed the ambiguity but also means "backend learns status from the supplier" was dropped as a concept — see the gap below, since it's the same underlying issue as PP-001's unimplemented supplier integration.
- **PP-004 (security hygiene) — implemented as specified.** TLS via `UseHttpsRedirection` (hosting-config item, as the enrichment predicted), EF Core parameterized queries throughout, model-bound input validation on order submission, and an append-only audit log (`AuditService`/`AuditLog` entity, `AdminAuditLogsController`) recording who/what/when on order creation and status changes. No gap found here.
- **PP-005 (configurable cohort/supplier) — implemented as specified.** `AdminCustomersController` and `AdminSuppliersController` provide admin CRUD for pilot customers and the active supplier with no code change required, gated behind `Policies.AdminOnly`.

### What remains genuinely open — and whether it still matters

- **PP-001's supplier fulfillment endpoint integration — still open, and now confirmed unbuilt, not just unspecified.** The PRD's acceptance criterion states the order "reaches the supplier's fulfillment endpoint." Nothing in `src/OrderPilot.Api` calls out to any external system — there is no HTTP client, no webhook receiver, no outbound integration code of any kind (confirmed by search). `OrderService.CreateOrderAsync` only persists the order locally with `Status.Submitted`; every subsequent status change is a manual Admin action. This was flagged as the single biggest schedule risk in the enrichment and feasibility reports (the one dependency outside the team's control), and the implementation resolves it not by integrating but by **not integrating at all**. This is not moot: the PRD's AC as literally written is not satisfied by the shipped system, and anyone reading the PRD text today would reasonably expect supplier connectivity that doesn't exist. If this pilot's code is meant to be a reference implementation for the parent initiative's full FR-002 (per the PRD's "Relationship to Parent Initiative" section), this gap should be corrected in the PRD text now — either by documenting the manual-Admin-update design as the actual pilot behavior, or by tracking supplier integration as explicit unfinished work rather than an implied-done AC.
- **No web UI exists in this repository.** PP-001's AC requires order creation "in the web UI." `src/` contains only `OrderPilot.Api` — a Web API project, with no frontend anywhere in the repo. This may be intentional (frontend built/owned elsewhere, out of this deliverable's scope) but the PRD does not say so, and as written PP-001 cannot be called fully implemented from this repository alone. Worth a PO/eng clarification on where the customer-facing UI lives, since it changes whether PP-001 is "done" or "half done."
- **PP-002's 60-second [DRAFT] latency target — still unconfirmed, and now unverifiable rather than resolved.** The backend does not hard-code a polling interval (no rate limiter or fixed schedule observed on `GetMine`/`GetById`), so the target is a property of whatever polling client calls this API — a client that doesn't exist in this repo yet. This makes the ambiguity functionally low-risk for the backend (nothing here violates the target), but it is not actually closed out; it simply moved downstream to a not-yet-built client. Should still be confirmed with the PO before that client is built, per the original recommendation.
- **Data-at-rest position and GDPR applicability — unaffected by the code.** No encryption-at-rest configuration is visible beyond SQL Server defaults, and nothing in the codebase resolves the compliance question one way or the other. These remain exactly as open as they were at the prior validation; the build gave no new information here.

## Testability and Coverage Assessment

The 31 passing tests (16 unit, 15 integration) are a strong positive signal for the PRD's implementability as far as they go: PP-003's auth, PP-005's admin config, PP-004's audit logging, and — most importantly — the previously-unspecified authorization boundary are all directly covered by named, intention-revealing tests (`OrderIsolationTests.CustomerB_CannotGetCustomerA_OrderById`, `RoleGatingTests`, `AdminConfigServiceSupplierTests`, `AuditServiceTests`, `OrderOwnerAuthorizationHandlerTests`). This confirms that once the HIGH-risk decisions were made, the PRD's remaining requirements were concrete enough to design a real test suite against — validating the "Acceptance Criteria: 88/100" score from the prior full run. However, there is **no test coverage for PP-001's supplier-endpoint AC**, because there is no code path to test — the test suite correctly reflects what was built, not what the PRD's AC describes.

## Outstanding Items From the Prior Report — Status Update

| # | Prior outstanding item | Status after implementation |
|---|---|---|
| 1 | PP-002 60-second latency target [DRAFT] | Still open — not hardcoded in backend, so not violated, but not confirmed or verifiable without a polling client |
| 2 | PP-003 auth mechanism (real vs. stubbed) | **Resolved** — real auth (Identity + JWT), tested |
| 4 | Authorization model (customer isolation, Admin gating) | **Resolved** — `OrderOwnerOrAdmin` policy, tested directly, 404-not-403 design |
| 5 | Data-at-rest protection baseline | Still open — no information added by the build |
| 6 | Compliance/GDPR applicability | Still open — no information added by the build |
| — | (New, discovered during this re-check) PP-001 supplier fulfillment endpoint integration | **Not resolved — confirmed unbuilt.** Highest-priority open item now that the rest of the system is live. |
| — | (New, discovered during this re-check) Web UI location/existence | **Newly surfaced gap** — no frontend in this repo; PP-001 AC not fully verifiable from `src/` alone |

## Recommended Actions

1. Update the PRD text (or a change log alongside it) to reflect that PP-001's supplier-fulfillment-endpoint AC was not implemented as written — the shipped pilot uses Admin-manual status updates only. This is the most important correction, since the PRD is meant to seed the parent initiative's full FR-002 implementation and currently overstates what was built.
2. Get a PO/eng answer on where the PP-001 web UI lives (separate repo/deliverable vs. not yet built) so PP-001's completion status is accurate.
3. Confirm the PP-002 60-second target before any polling client is built against this API, since the backend imposes no constraint either way.
4. Data-at-rest and GDPR items remain low-priority per the prior report's assessment — no new urgency introduced by the build.

## Next Steps

This focused implementability score (74/100) does not override the PRD's frontmatter status, which reflects the prior full-checklist (`all`) validation at 77%. Treat this report as a supplementary, post-implementation check: the two HIGH-risk items that mattered most for *safe* implementation (auth, authorization) are demonstrably closed, but the one item that mattered most for *complete* implementation against the PRD's literal text (supplier integration) is now confirmed open rather than merely ambiguous, and should be tracked explicitly rather than left implicit now that a "done" pilot exists for people to point to.
