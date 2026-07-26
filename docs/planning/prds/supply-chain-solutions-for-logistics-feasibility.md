---
prd: supply-chain-solutions-for-logistics
assessedAt: 2026-07-26
analyst: technical-analyst
enrichmentReport: docs/planning/prds/supply-chain-solutions-for-logistics-enrichment.md
team: "2 BE, 1 FE, 1 QA"
timeline: "4 weeks"
---

### Feasibility Report: supply-chain-solutions-for-logistics
Team: 2 BE, 1 FE, 1 QA
Timeline: 4 weeks

### Technical Feasibility: LOW

This rating applies to **the full PRD scope (all 8 functional requirements across mobile, web/platform, data, monitoring, security, and infra) against the stated team and timeline.** The enrichment report already classified the overall complexity as **XL — a multi-quarter program**, with every component built from zero (confirmed greenfield: no `src/`, no dependency manifest, no application code anywhere in this repository). A 4-person generalist team (2 BE, 1 FE, 1 QA) with no dedicated mobile, data, DevOps, or security specialists, working for 4 weeks, is being measured against a program the enrichment itself sizes at 4+ weeks *per component, minimum*, before aggregation. See the dedicated section below for the direct answer to the team-composition question — this section covers the standard feasibility mechanics first.

### Resource Estimate

**A. What the stated team can deliver in the stated window**

- Backend (2 BE x 20 working days): 40 person-days
- Frontend (1 FE x 20 working days): 20 person-days
- Testing (1 QA x 20 working days): 20 person-days
- DevOps: 0 person-days (no dedicated role on this team)
- **Total available: 80 person-days (16 person-weeks)**

**B. What the full PRD scope requires (rough, assumption-laden — see enrichment "Unknowns")**

| Component | T-shirt (enrichment) | Estimated effort, correctly staffed | Rationale |
|---|---|---|---|
| Native mobile rewrite (iOS + Android) | L–XL | 40–60 person-weeks | 2 native platforms, greenfield, feature-parity discovery against an undocumented legacy Xamarin app; needs dedicated Swift and Kotlin engineers — none exist on the stated team |
| Enterprise Platform / web (FR-002) | M–L | 15–24 person-weeks | "Maintain, not replace" reduces scope, but the "without manual intervention" acceptance criterion implies real workflow gaps; legacy platform source unavailable for verification |
| Real-time inventory/delivery data layer (FR-001, FR-004) | L | 12–18 person-weeks | Architecture (polling vs. push/streaming) is undetermined pending 3 TBD latency thresholds; must serve web + 2 native clients consistently |
| Data/BI pipeline + Tableau→PowerBI (FR-003, FR-005) | M–L | 10–15 person-weeks | Net-new ETL/ELT; needs a data engineer — not on the stated team; report-conversion volume unstated |
| Monitoring/observability (FR-006) | M | 6–8 person-weeks | Build-vs-buy undecided; needs DevOps/SRE — not on the stated team |
| Security/compliance (SSO/OIDC, audit trail, encryption) | M–L | 10–16 person-weeks | Net-new IdP integration; doubles if PCI-DSS or food-traceability regulations are confirmed in-scope |
| Azure infra cleanup (Phase 2) | M | 6–8 person-weeks | Bounded cloud-ops work, but scheduled concurrently with Phase 1 on shared infra |
| Process (Agile PM, wiki) | S | ~2 person-weeks ongoing | Not a one-time cost; a cadence, not a delivery item |
| **Total (low end)** | | **~101–151 person-weeks (~505–755 person-days)** | Excludes rework risk from the 6 unresolved PO items and undiscovered legacy scope |

**Timeline Fit: NO.** Available capacity (80 person-days) covers roughly 11–16% of even the low end of the full-scope estimate — and that comparison is generous, because the 80 person-days come from generalists, while a large fraction of the required effort (mobile, data engineering, DevOps, security) needs skills this team does not have at any allocation.

### Integration Touchpoints

1. **Existing Enterprise Platform** (external, source not in this repo) — order placement/fulfillment logic FR-002 depends on. Risk: HIGH (unverifiable integration contract).
2. **Existing Xamarin mobile apps** (external, source not in this repo) — feature-parity baseline for the native rewrite. Risk: HIGH (undiscovered scope).
3. **Supplier-side fulfillment systems** — integration contracts for FR-002 "one or more Suppliers" not described. Risk: HIGH.
4. **SAP Cloud** — present in the suggested stack, role never explained in the PRD body. Risk: MEDIUM-HIGH (unknown surface).
5. **Existing Tableau reporting layer** — source for the Phase 2 PowerBI conversion; report count/complexity unknown. Risk: MEDIUM.
6. **OIDC/OAuth2 IdP** (likely Azure AD / Entra ID, unconfirmed) — net-new SSO wiring across web + 2 native apps. Risk: MEDIUM.
7. **APNs / FCM push notifications**, fronted by a net-new Azure APIM layer. Risk: MEDIUM.
8. **PowerBI** — target BI tool, net-new integration. Risk: LOW-MEDIUM.
9. **Existing Azure subscription / DevOps** — concurrent Phase 2 cleanup (Key Vault, App Service autoscaling, subscription/repo cleanup) touching infra Phase 1 mobile work will depend on (push notifications via the new APIM). Risk: MEDIUM (coordination, not technical).
10. **TAU (Transport Admin UI)** — separate app targeted for a .NET 6.0 upgrade; source not available for compatibility assessment. Risk: MEDIUM.

### Prerequisites

1. Resolve all 6 outstanding PO sign-off items before any estimate derived from them is trusted: 3 latency thresholds (FR-001/FR-004/FR-006 + the proposed p95 target), PCI-DSS/food-traceability compliance scope, persona priority/phase sequencing, input-validation/session-management position.
2. Discovery spike against the actual (out-of-repo) Xamarin app and Enterprise Platform source — requires PO/vendor access to legacy codebases not present in this repository.
3. Explicit architecture decisions: IdP choice (Azure AD vs. alternative), SQL Server vs. PostgreSQL, monitoring build-vs-buy, SAP Cloud's actual role.
4. Tableau report inventory (count + complexity) before sizing the PowerBI conversion.
5. Staffing gap closure: no native iOS/Android engineer, no dedicated data engineer, no DevOps/SRE, no dedicated security engineer exists on the stated team — each is required by at least one in-scope component.
6. Azure subscription access/permissions and environment provisioning for whichever team executes this.

### Risk Summary

- **RED** — Full scope is an XL/multi-quarter program; a 4-person generalist team with no mobile, data, DevOps, or security specialists cannot deliver it in 4 weeks under any internal reallocation of those 4 people.
- **RED** — Zero native mobile engineers on the stated team. Phase 1 (the PRD's own stated priority) is a two-platform native rewrite; this team cannot start that work at all, let alone finish it, regardless of timeline.
- **RED** — 3 undefined latency thresholds + unresolved compliance scope block committing to a data-layer and security architecture; starting build before these land risks expensive rework.
- **RED** — Legacy Xamarin/Enterprise Platform source is unavailable for discovery; any integration or feature-parity plan is an unverified assumption.
- **AMBER** — SSO/IdP choice unconfirmed; net-new integration with no fallback if Azure AD is not the eventual choice.
- **AMBER** — Tableau→PowerBI conversion scope is unbounded (no report inventory).
- **AMBER** — SAP Cloud's role in the architecture is unclarified.
- **AMBER** — Concurrent Azure infra changes (Phase 2 cleanup) on infrastructure Phase 1 depends on creates sequencing risk.
- **GREEN** — The suggested Azure-centric stack (ASP.NET, ReactJS, Azure, SQL Server/PostgreSQL) is standard and well-supported; no exotic-technology risk once scope and stack are narrowed and confirmed.
- **GREEN** — Greenfield means no legacy regression risk for whatever is newly built — no existing system to accidentally break.
- **GREEN** — Process requirements (Agile cadence, Azure DevOps wiki) are low-risk and partially already supported by this repo's existing Claude Workflow scaffolding.

### Recommendation: REJECT
*(for the full PRD scope, against the stated team of 2 BE/1 FE/1 QA and a 4-week timeline)*

This is not a "tight but doable with discipline" situation — the gap between required effort (~101–151 person-weeks, needing skills the team doesn't have) and available capacity (16 person-weeks of generalist effort) is roughly 6-10x on effort alone, before even accounting for the missing mobile/data/DevOps/security skill sets. No amount of process optimization within this team+timeline closes that gap.

Conditions under which any part of this PRD *could* proceed in the next 4 weeks are laid out below, because "reject the full scope" is not the same as "there is nothing useful to do in 4 weeks."

---

## Answering: 4-Week Production Delivery

**Direct question: given the XL/multi-quarter complexity, is any team composition realistic for a 4-week full-scope production delivery?**

**No.** This needs to be stated plainly rather than softened: no team composition — not 4 people, not 40 — can take this PRD's full scope (native mobile rewrite on two platforms, Enterprise Platform ordering, a real-time data layer, a BI pipeline, new monitoring, SSO/security/compliance work, and Azure infra cleanup) from a completely empty repository to a production-deployed state in 4 calendar weeks. This isn't a staffing-math problem that more headcount instantly fixes, for three structural reasons independent of team size:

1. **Six architecture-blocking decisions are still open** (3 latency thresholds, compliance scope, phase sequencing, input-validation/session-management position). Nobody can architect the data layer, the auth/session model, or the security posture correctly until these land — throwing more engineers at an undefined architecture produces more rework, not more done. These need a PO decision cycle that itself typically takes days-to-weeks, before code should start.
2. **Two of the PRD's dependencies live outside this repo and were unavailable for analysis** (the existing Xamarin app, the existing Enterprise Platform). A discovery/feature-parity spike against that legacy source is a hard prerequisite for the mobile rewrite and for FR-002 — and it hasn't happened yet. You cannot compress a discovery-then-build sequence into 4 weeks when the discovery alone is realistically 1-2 weeks for the mobile surface once you even get access.
3. **The stated team has zero specialists in three of the six required domains** (native mobile, data engineering, DevOps/SRE) and only thin security coverage. Even an infinite timeline doesn't fix a skills gap — you'd need to hire/contract those roles regardless of how much time you allow.

### (a) What full-scope delivery would actually require

A realistic plan for the **entire PRD**, based on the enrichment's own component-level estimates (~101–151 person-weeks minimum, XL/multi-quarter, HIGH uncertainty):

- **Team size/composition:** ~10–14 people sustained, not 4:
  - 2 native mobile engineers (1 iOS/Swift, 1 Android/Kotlin) minimum — realistically 2 per platform (4 total) if the "L–XL" mobile estimate holds
  - 2-3 backend engineers (Enterprise Platform + real-time data layer)
  - 1 frontend engineer (web)
  - 1 data engineer (BI pipeline, Tableau→PowerBI)
  - 1 DevOps/SRE (monitoring, Azure infra cleanup, APIM)
  - 1 security engineer (SSO/OIDC, audit trail, compliance — larger if PCI-DSS/food-traceability confirmed)
  - 1-2 QA engineers
  - 1 tech lead/architect to own the cross-cutting decisions (data layer architecture, IdP choice, DB engine)
- **Realistic timeline:** 2–3 quarters (roughly 6-9 months), sequenced — not parallelized to a single sprint — because the auth/IdP decision and the data-layer architecture decision block almost every other workstream. This matches the enrichment's own "multi-quarter program" framing; it is not a padded estimate.
- **Precondition, before that clock even starts:** all 6 PO decisions resolved, and the Xamarin/Enterprise Platform discovery spike completed, ideally in a 2-3 week "sprint zero" ahead of the timeline above.

### (b) What COULD realistically ship to production in 4 weeks

Given the stated team (2 BE, 1 FE, 1 QA) has no mobile, data, or DevOps specialists, **mobile scope is off the table entirely for this team regardless of timeline** — not "tight," genuinely not startable, since nobody on the team can write native iOS/Android code. The only realistic candidate for a 4-week production slice is a narrow cut of **FR-002 (Enterprise Platform ordering)**, because it's the one in-scope requirement that matches the team's actual skills (BE + FE + QA, standard web stack).

**Proposed 4-week scope: a single-workflow order-placement pilot, not the full FR-002 acceptance criteria.**

- One order type, one supplier integration (not "one or more Suppliers" generically) — create, submit, track to a single fulfillment endpoint.
- Basic authenticated access (username/password or a stubbed session), explicitly **not** the full SSO/OIDC integration — that's a separate, net-new IdP workstream that alone needs weeks.
- Status visibility via polling (not the push/streaming architecture FR-001/FR-004 imply) — deferring the "near real-time" architectural fork entirely, since its own thresholds aren't defined yet.
- Baseline security hygiene (TLS in transit, parameterized queries/input validation, basic audit logging of the one workflow) rather than the full audited-old/new-values-every-operation NFR, and rather than any compliance posture (PCI-DSS/food-traceability remain explicitly out of this slice).
- Deployed to a limited-availability production environment (a real pilot with a small user cohort), not the fully-scaled, 99.99%-SLA, auto-scaling platform the NFRs describe.

**Why this is plausible in 4 weeks and the rest isn't:** it's the one slice where the team's actual composition (BE-heavy, one FE, one QA) matches the work, it doesn't require any of the 3 undefined latency thresholds to be resolved (polling sidesteps the fork), and it doesn't require the legacy Xamarin discovery spike. It still requires two things happening in week 1, before any code: (1) PO picks the single order-type/supplier pair to pilot, and (2) the team commits to a DB engine and auth approach so they aren't re-deciding architecture mid-sprint. Even so, treat "production" here as "pilot release to a limited cohort," not the fully-compliant, fully-scaled platform the PRD describes — that framing needs to be explicit to whoever is sponsoring this, so a 4-week pilot isn't mistaken for the PRD being "done."

**If mobile is truly the priority** (the PRD states Phase 1 is mobile-first), the honest 4-week answer for mobile is: staff 2 dedicated native engineers (1 iOS, 1 Android) plus 1 BE for supporting APIs and use the 4 weeks entirely for the discovery spike against the legacy Xamarin app plus a thin vertical skeleton (e.g., login + read-only stock/delivery view for one screen) — not feature parity, not full production replacement. That is the realistic ceiling for mobile in this window with any team, because feature-parity discovery hasn't even started.
