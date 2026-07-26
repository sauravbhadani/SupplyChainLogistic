---
title: Order Placement Pilot (FR-002 narrow slice)
source: docs/planning/prds/supply-chain-solutions-for-logistics-feasibility.md
sourceType: internal-derived
parentInitiative: supply-chain-solutions-for-logistics
importedAt: 2026-07-26
status: feasibility-assessed
linearInitiative: null
validationReport: docs/planning/prds/order-placement-pilot-validation.md
enrichmentReport: docs/planning/prds/order-placement-pilot-enrichment.md
feasibilityReport: docs/planning/prds/order-placement-pilot-feasibility.md
---

# Order Placement Pilot (FR-002 narrow slice)

> **Origin:** this PRD is a deliberately narrowed slice of `supply-chain-solutions-for-logistics`, carved out by the `/PRDFeasibility` assessment (see "Answering: 4-Week Production Delivery" → "(b) What COULD realistically ship in 4 weeks" in `supply-chain-solutions-for-logistics-feasibility.md`) as the one piece of that PRD's scope that plausibly fits a 4-week window with a 2 BE / 1 FE / 1 QA team. It is explicitly **not** a replacement for the parent PRD — it exists to get something real into production quickly while the parent initiative's open PO decisions and legacy-system discovery proceed in parallel.

## Business Context

Same client and program as `supply-chain-solutions-for-logistics`: a food-industry supply chain platform where authorized business customers place orders with suppliers through an Enterprise Platform. Rather than waiting for the full platform (native mobile rewrite, SSO, real-time data layer, BI pipeline — an XL, multi-quarter program per the parent PRD's enrichment), this pilot validates the core order-placement workflow end-to-end in production with a limited cohort, using only the team and time available today.

## Problem Statement

The business wants proof, within 4 weeks, that a business customer can place and track an order against a real supplier through a production system — without waiting for the full Enterprise Platform (multi-supplier support, SSO, real-time push status, full audit/compliance posture). This pilot de-risks the core ordering workflow early and produces a working reference implementation the full platform build can later extend, rather than design in the abstract.

## User Personas / Target Audience

- **Primary: Business Customer (pilot cohort)** — a small, limited set of authorized business customers who place one order type with one designated supplier. Goal: place an order and see its status without manual/offline follow-up.
- **Secondary: Pilot Supplier** — the single third-party supplier receiving and fulfilling pilot orders.
- **Secondary: Internal Admin/QA** — verifies orders flow correctly end-to-end and monitors the pilot cohort.

## Scope

### Functional Requirements & Acceptance Criteria

| ID | Requirement | Acceptance Criteria |
|----|-------------|----------------------|
| PP-001 | A pilot-cohort business customer can create and submit an order of the single supported order type to the single designated supplier | Customer can complete order creation and submission in the web UI without any manual/offline step; order reaches the supplier's fulfillment endpoint |
| PP-002 | A submitted order's status is visible to the customer | Customer can view current order status (e.g., submitted / accepted / fulfilled) via polling-based refresh (target: status reflects backend state within 60 seconds of a change — **[DRAFT — confirm with PO]**) |
| PP-003 | Basic authenticated access to the pilot ordering flow | Customer authenticates via username/password or a stubbed session (explicitly not full SSO/OIDC — that remains scoped to the parent PRD) |
| PP-004 | Baseline security hygiene on the pilot workflow | TLS in transit for all requests; parameterized queries / input validation on order submission; basic audit log entry (who, what, when) for order creation and status changes |
| PP-005 | Pilot cohort and supplier are configurable, not hardcoded | Admin can designate which customer accounts and which single supplier are active in the pilot without a code change |

### Non-Functional Requirements (NFRs)

- **Performance:** No specific SLA beyond "usable for a small pilot cohort"; formal latency/throughput targets are explicitly deferred to the parent PRD.
- **Security:** TLS in transit, input validation, basic audit logging (see PP-004). Full SSO/OIDC, encryption-at-rest hardening, and compliance posture (GDPR/PCI-DSS/food-traceability) are **out of scope** for this pilot — carried by the parent PRD.
- **Availability:** Best-effort; no 99.99% SLA target for this pilot (that target belongs to the parent PRD's production platform).
- **Scalability:** Not a goal for this pilot — single supplier, single order type, limited cohort by design.

## Out of Scope

- Multiple suppliers or order types (parent PRD scope: "one or more Suppliers" generically)
- Full SSO/OIDC integration (parent PRD scope)
- Push/streaming "near real-time" status updates (parent PRD FR-001/FR-004 — this pilot uses polling)
- Native mobile apps (parent PRD Phase 1 — no mobile engineers on this team)
- BI/reporting pipeline, monitoring tooling, Azure infra cleanup (parent PRD FR-003/FR-005/FR-006 and Phase 2)
- Any compliance posture beyond baseline security hygiene (PCI-DSS, food-traceability — parent PRD open question)
- Auto-scaling / high-availability infrastructure (parent PRD NFR)

## Relationship to Parent Initiative

This pilot does not close or replace any requirement in `supply-chain-solutions-for-logistics`. FR-002 in the parent PRD remains open at its full scope (multi-supplier, full SSO, full audit trail). Learnings and code from this pilot are expected inputs to the parent initiative's eventual FR-002 implementation, not a substitute for it.

## Technical Context

> Full analysis: `docs/planning/prds/order-placement-pilot-enrichment.md`

**Codebase status:** Greenfield — no existing source in this repository (no `src/`, no manifest, no application code). Every PP-00x requirement is net-new build, but the pilot's scope is deliberately small: one order type, one supplier, no push/streaming, no SSO, no multi-tenant complexity. Stack is inherited-but-undecided from the parent PRD (ASP.NET/.NET Core/Azure/SQL Server, or JS/TS/React/Node/Postgres) — either is sufficient; choose based on team familiarity, not requirements.

**Architecture notes:**
- PP-001's order submission is a single schema/form/endpoint (simple), but the supplier fulfillment endpoint's protocol, auth, and retry contract is undocumented — this is the pilot's one hard external dependency and its main schedule risk.
- PP-002's customer-facing polling UI is simple; the backend-side mechanism for *learning* a status changed (supplier webhook vs. backend polling the supplier vs. manual update) is not specified and is a real design decision.
- PP-004's audit log and input validation ride along with the PP-001/PP-002 endpoints rather than being separate build efforts; TLS is a hosting-config item.
- PP-005's admin config work is small but blocked on resolving the authorization boundary below.

**Top risk flags:**
- HIGH — No authorization boundary specified: whether one pilot customer can see another's orders, and how Admin access is distinguished, is undecided. Must resolve before building order views.
- HIGH — PP-003 auth mechanism (real username/password vs. stubbed session) is undecided; this changes the backend build materially and is a security-posture decision for a production system.
- MEDIUM — Supplier fulfillment endpoint contract unknown; backend status-update mechanism unspecified; no data-at-rest position stated; PP-002's 60-second latency target remains [DRAFT].

**Complexity estimate:** Overall **S/M** (small in absolute terms; kept off a clean S only by the unresolved supplier-integration unknown). Per-component: Order submission S, Supplier integration S–M (uncertain), Status polling S, Auth XS–S (pending PP-003 decision), Security hygiene S, Admin config S, QA S. Confidence: **MEDIUM**.

**Preliminary feasibility:** FEASIBLE. No architecturally novel component; standard B2B web-app patterns throughout. Two pre-conditions matter most for the 4-week timeline: resolve the PP-003 auth decision and the customer-data authorization boundary before backend work starts, and get early visibility into the actual supplier endpoint's contract, since it's the one dependency outside the team's control.
