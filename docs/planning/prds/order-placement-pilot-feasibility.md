---
prd: order-placement-pilot
assessedAt: 2026-07-26
analyst: technical-analyst
enrichmentReport: docs/planning/prds/order-placement-pilot-enrichment.md
parentFeasibilityReport: docs/planning/prds/supply-chain-solutions-for-logistics-feasibility.md
team: "2 BE, 1 FE, 1 QA"
timeline: "4 weeks"
---

### Feasibility Report: order-placement-pilot
Team: 2 BE, 1 FE, 1 QA
Timeline: 4 weeks

> **Context:** this PRD is the narrow slice the parent PRD's feasibility report (`supply-chain-solutions-for-logistics-feasibility.md`) itself proposed as "what COULD realistically ship in 4 weeks" for this exact team, after that report rejected the parent's full XL scope outright. This assessment checks whether that proposal holds up under the same rigor once it is its own PRD, rather than assuming the parent report's optimism was correct.

### Technical Feasibility: MEDIUM

Not HIGH, because two HIGH-severity decisions (PP-003 auth mechanism, customer/admin authorization boundary) are still open and one external dependency (supplier fulfillment endpoint) is outside the team's control with an unconfirmed contract. Not LOW, because — unlike the parent PRD — this scope was deliberately sized to match the team's actual composition: no missing specialist roles (no mobile, data, or DevOps gap), no architecturally novel component, and the enrichment's own complexity estimate (S/M) is consistent with the requirement content, not an optimistic label on unchanged scope. The open items are fast, cheap decisions and one access request — not structural capability gaps — which is what keeps this at MEDIUM rather than LOW.

### Resource Estimate

**Available capacity:** 2 BE x 20 days + 1 FE x 20 days + 1 QA x 20 days = **80 person-days** over 4 weeks.

**Planning-level estimate, by component (rounded to whole days; assumptions stated where the enrichment flagged a swing factor):**

| Component | Role | Person-days | Notes / assumption |
|---|---|---|---|
| Order submission form + endpoint + validation (PP-001) | BE | 3 | Single schema, single endpoint — standard CRUD shape |
| Supplier fulfillment integration (PP-001) | BE | 7 | **Swing factor.** Assumes a conventional, already-existing REST-ish API to call into (enrichment's stated assumption). Could run to 12-15+ pd if the endpoint is undocumented, requires negotiation, or credentialing is slow — see Risk Summary. |
| Backend status-update mechanism + polling endpoint (PP-002) | BE | 3 | Assumes backend polls the supplier on a schedule (simplest of the three unresolved options); +1-2 pd if a webhook receiver is required instead |
| Auth (PP-003) | BE | 4 | Conservative assumption: **real** username/password (credential storage, hashing, session lifecycle) — appropriate default for a production system handling real orders. Drops to ~1 pd if PO approves a stubbed session instead. |
| Security hygiene / audit log (PP-004) | BE | 2 | Rides along with PP-001/PP-002 endpoints per enrichment; TLS is hosting-config, not build effort |
| Admin config CRUD + authorization boundary enforcement (PP-005) | BE | 3 | Blocked on the authorization-boundary decision — estimate assumes a simple stated rule ("customers see only their own orders; only Admin edits config"), not a role/permission system |
| Backend integration/code-review buffer | BE | 5 | Cross-cutting; absorbs minor rework across the above |
| **Backend subtotal** | | **27** | of 40 available (2 BE x 20d) — **13 pd slack**, mostly there to absorb the supplier-integration swing risk |
| Order creation form UI (PP-001) | FE | 3 | |
| Status view + polling UI (PP-002) | FE | 3 | |
| Login/auth UI (PP-003) | FE | 2 | Basic login form; grows if real auth needs password reset/account provisioning UI |
| Admin config screen (PP-005) | FE | 3 | |
| UI states (loading/error/empty) + polish | FE | 2 | |
| Backend integration + bug-fixing | FE | 3 | |
| **Frontend subtotal** | | **16** | of 20 available (1 FE x 20d) — **4 pd slack** |
| Test plan across PP-001–PP-005 | QA | 2 | Cannot finalize PP-002's acceptance test until the 60-second [DRAFT] target is confirmed |
| Functional test execution (order flow, status, auth, admin config) | QA | 6 | |
| Supplier integration testing | QA | 3 | May require a mocked supplier endpoint if sandbox access lags — see Prerequisites |
| Security/authorization testing (data isolation, input validation, TLS) | QA | 3 | Cannot be written meaningfully until the authorization boundary is decided |
| Regression + pilot-cohort UAT support | QA | 3 | |
| Bug verification / retest buffer | QA | 3 | |
| **QA subtotal** | | **20** | of 20 available (1 QA x 20d) — **0 pd slack** |
| DevOps (hosting/DB provisioning, TLS config) | — | folded into BE buffer | No dedicated DevOps role on this team; one of the 2 BE engineers must own this in week 1 |
| **Total** | | **63 of 80 person-days (~79% utilization)** | |

**Timeline Fit: YES, but tight and sequencing-dependent.** The person-day math fits with a genuine ~17 pd (21%) margin — a real difference from the parent PRD's 6-10x shortfall. However, most of that slack sits in backend (to absorb the supplier-integration swing) and none of it sits in QA (20/20 pd, no buffer). This is a capacity fit on paper that can still miss the calendar if the two HIGH-risk decisions below don't land in week 1: several of the backend/FE line items (admin config, order views, auth UI) cannot start correctly until those decisions are made, so a slow decision costs calendar days even though it doesn't add person-days to the total.

### Integration Touchpoints

1. **Pilot supplier's fulfillment endpoint** (external, contract unconfirmed) — protocol, auth, idempotency/retry behavior not documented anywhere in the PRD or its enrichment. Risk: **HIGH**. This is the pilot's only dependency the team does not control, and the one component that can consume the whole 4-week window regardless of internal execution speed.
2. **Backend-to-supplier status-update mechanism** (internal design decision: webhook vs. backend-polls-supplier vs. manual/admin update) — not a separate external system, but an unresolved architectural choice that determines whether a scheduler/job component needs to be built at all. Risk: **MEDIUM**.
3. **Credential/identity store** (net-new, only if PP-003 resolves to real auth) — standard build (hashed credentials, session/cookie management), no exotic technology. Risk: **LOW-MEDIUM**, contingent entirely on the PP-003 decision landing early.
4. **Hosting/TLS PaaS configuration** for the pilot's production environment — standard modern PaaS/App Service concern, not custom build work. Risk: **LOW**.
5. **No queue, event bus, cache layer, or streaming infrastructure required** — polling-based status and single-supplier scope explicitly avoid this class of integration (contrast with the parent PRD's FR-001/FR-004 real-time data layer). Risk: **GREEN** — this materially shrinks integration surface versus the parent PRD.

### Prerequisites

1. **PO/eng decision — PP-003 auth mechanism (real vs. stubbed).** Needed by day 1-2 of week 1; blocks backend auth build and the FE login screen. Cheap decision, not a discovery task (per enrichment).
2. **PO/eng decision — authorization boundary** (can one pilot customer see another's orders; how is Admin access distinguished). Needed by day 1-2 of week 1; blocks PP-001 order-view implementation and PP-005 admin config. Building either without this decision risks a customer data-isolation bug or a rebuild.
3. **Access — pilot supplier fulfillment-endpoint documentation and sandbox credentials.** Request on day 1; this is outside the team's control and is the single biggest schedule risk. Must be confirmed before week 2 backend integration work begins, or the 4-week date is at risk regardless of internal readiness.
4. **Technical design decision — backend status-update mechanism** (webhook / poll-the-supplier / manual). Resolve in week 1 technical design so backend scope for PP-002 is fixed before coding starts.
5. **Stack selection** (ASP.NET/.NET/Azure/SQL Server vs. JS/TS/React/Node/Postgres) — decide day 1 based on team familiarity; does not change the size estimate but determines who can start immediately and in parallel.
6. **PO confirmation — PP-002's 60-second [DRAFT] latency target.** Needed before QA can finalize a firm acceptance test; target end of week 1.
7. **PO one-line confirmation — data-at-rest position** (platform-default encryption only vs. additional hardening) **and GDPR applicability** to the pilot cohort. Low effort, low likelihood of blocking, but should close early rather than under time pressure in week 4.
8. **Hosting/DB environment provisioned** in week 1, in parallel with the decisions above. No dedicated DevOps role exists on this team — one of the 2 BE engineers must own this explicitly, or it will silently compete with backend feature work.

### Risk Summary

- **RED** — Authorization boundary undecided: whether one pilot customer can see another's orders, and how Admin access is gated, is unresolved. Blocks correct implementation of PP-001 order views and PP-005 admin config; must land before that backend work starts.
- **RED** — PP-003 auth mechanism undecided (real vs. stubbed): this is a different backend build and a different production security posture, not a wording ambiguity. Must land before backend work starts.
- **RED** — Supplier fulfillment endpoint contract unconfirmed and outside the team's control: protocol, auth, idempotency/retry, and reliability are unknown. This is the one item that can single-handedly consume the 4-week window even if every other component ships on time; access must be pursued starting day 1.
- **AMBER** — QA has zero capacity slack (20 of 20 person-days). Any slippage elsewhere (most likely the supplier integration) has no QA buffer to absorb it — a fallback descope decision should be made now, not under pressure in week 4.
- **AMBER** — Backend status-update mechanism (webhook vs. poll vs. manual) unresolved; affects whether a scheduler/job component must be built. Should be settled in week 1 technical design, not discovered mid-build.
- **AMBER** — PP-002's 60-second latency target remains [DRAFT]; QA cannot write a firm acceptance test against it until confirmed.
- **AMBER** — Data-at-rest position and GDPR applicability to the pilot cohort are unconfirmed; low likelihood of blocking but currently silent on a production system handling real customer data.
- **GREEN** — No internal dependencies to integrate against; greenfield means no legacy-regression risk for anything newly built.
- **GREEN** — No queue/streaming/cache infrastructure required; polling-based design keeps the integration surface small and standard-PaaS-hostable.
- **GREEN** — Scope matches the team's actual skillset (BE/FE/QA generalists, standard web-app patterns) — no missing specialist roles, unlike the parent PRD's mobile/data/DevOps gaps.
- **GREEN** — Resource math fits with a real ~21% margin (63 of 80 person-days) at planning-level estimates, assuming decisions land on time.

### Recommendation: PROCEED
*(conditional — this is a "tight but achievable" call, not a rubber stamp)*

The person-day math fits, and the two HIGH risks are fast, cheap PO/eng decisions rather than structural capability gaps like the parent PRD's missing mobile/data/DevOps specialists — that distinction is why this is PROCEED rather than DEFER. But the margin is real only if the following land on schedule; if they slip past week 1, the honest fallback is a short DEFER of backend/admin coding (not the whole pilot) until they do, because starting that work without them risks the exact rework the enrichment warned about.

Conditions:
1. Resolve the PP-003 auth decision and the customer/admin authorization boundary within the first 1-2 days of week 1, before any backend order-view or admin-config code is written. Treat these as blocking pre-work, not week-1 nice-to-haves.
2. Request the pilot supplier's fulfillment-endpoint documentation and sandbox credentials on day 1 of week 1; escalate if not confirmed by end of week 1. If access is not secured before week 2 backend integration work begins, treat the 4-week ship date as at risk and consider a mocked supplier interface for pilot-demo purposes while integration continues, rather than letting this silently consume the timeline.
3. Resolve the backend status-update mechanism (webhook vs. poll vs. manual) and confirm PP-002's 60-second latency target as part of week 1 technical design, so QA can write firm acceptance tests starting week 2.
4. Choose the stack on day 1 based on team familiarity, and provision the hosting/DB environment during week 1 in parallel with the decisions above — one of the 2 BE engineers should own this explicitly given no dedicated DevOps role exists.
5. Get one-line PO confirmation on data-at-rest position and GDPR applicability early, rather than leaving it open until the end.
6. Because QA has no capacity slack, agree now on a fallback descope (reduced regression depth, a narrower pilot cohort, or a short extension) to use if any upstream item — most likely the supplier integration — slips, rather than deciding this under pressure in week 4.
