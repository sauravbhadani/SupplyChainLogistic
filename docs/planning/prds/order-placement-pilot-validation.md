---
prd: order-placement-pilot
validatedAt: 2026-07-26
checklist: all
overallScore: 77
status: validated
---

# Validation Report: order-placement-pilot

Checklist: all

## Document Type Note

This PRD is a deliberately narrow, derived pilot scope — a single-order-type, single-supplier, basic-auth, polling-status slice carved out of `supply-chain-solutions-for-logistics` by that PRD's feasibility assessment, sized to ship in 4 weeks with a 2 BE / 1 FE / 1 QA team. All structurally-required sections are present: Business Context, Problem Statement, User Personas, an ID-tagged Functional Requirements table (PP-001–PP-005) with acceptance criteria, Non-Functional Requirements, Out of Scope, and a "Relationship to Parent Initiative" section. Scores below reflect content actually written in the document; scope deliberately deferred to the parent PRD (and explicitly labeled as such) is not penalized as a silent gap, but is still tracked as a real open item where it affects this pilot's own implementability.

## Scores

- Problem Statement:       80/100
- User Definition:         65/100
- Acceptance Criteria:     88/100
- Security Considerations: 72/100
- **Overall Completeness: 77%**

Weighting used (same as `supply-chain-solutions-for-logistics` report): Problem Statement 25%, User Definition 20%, Acceptance Criteria 30%, Security Considerations 25%.

### Problem Statement — 80/100

Present:
- Clear, specific problem framing: prove within 4 weeks that a business customer can place and track an order against a real supplier in production, without waiting for the full Enterprise Platform.
- Target audience identified: business customers (pilot cohort), via the pilot supplier.
- Business Context section grounds the pilot in the same client/program as the parent PRD, and states the rationale for narrowing (team size and timebox available today vs. the full multi-quarter platform).
- The stated goal is itself testable at the PRD level ("produces a working reference implementation") rather than vague aspiration.

Gaps:
- Impact is not quantified. There is no baseline metric (e.g., cost of continuing manual/offline ordering, risk reduced, revenue/relationship value of the pilot cohort) — only the team/timebox constraint (4 weeks, 2 BE/1 FE/1 QA), which is a delivery constraint, not a quantified problem impact.
- This is the same category of gap noted in the parent PRD's validation report and is the one criterion (of the three in the rubric: clear description, quantified impact, target audience) not met here either.

### User Definition — 65/100

Present:
- Three personas: Business Customer (pilot cohort, primary), Pilot Supplier (secondary), Internal Admin/QA (secondary) — explicit primary/secondary priority stated, unlike an unordered list.
- A goal is stated for each persona (place/track an order without manual follow-up; receive and fulfill pilot orders; verify orders flow end-to-end and monitor the cohort).

Gaps:
- No persona has an explicit, dedicated pain-point statement. The primary persona's pain point is only inferable from its goal ("without manual/offline follow-up" implies the current pain is manual/offline follow-up) rather than stated directly.
- The two secondary personas (Pilot Supplier, Internal Admin/QA) have no pain points at all, not even inferred ones.
- No "Who" framing (role, context) beyond a one-line label — thinner than the parent PRD's persona section, which gives each persona a "Who" and "Goals" line plus explicitly-flagged assumed pain points.

### Acceptance Criteria — 88/100

Present:
- All 5 functional requirements (PP-001–PP-005) carry unique IDs and a concrete, testable acceptance criterion each.
- 4 of 5 (PP-001, PP-003, PP-004, PP-005) have fully-specified, unambiguous acceptance criteria with no open numeric placeholders (e.g., PP-005: "Admin can designate which customer accounts and which single supplier are active in the pilot without a code change").
- PP-002 has a concrete draft target (60 seconds) rather than a bare "TBD" — explicitly marked **[DRAFT — confirm with PO]** — which per the same scoring convention used for the parent PRD counts as present/testable structurally, since the pass/fail mechanism is already defined and only the number needs PO sign-off.
- No ambiguous qualitative language ("fast", "easy", "user-friendly") appears in any acceptance criterion.

Gaps:
- PP-002's 60-second polling-latency target is still unconfirmed by the PO — the one requirement not yet fully locked.
- PP-003's acceptance criterion offers two alternative implementations ("via username/password or a stubbed session") without stating which one is actually intended for the pilot's production deployment — this is an implementability ambiguity, not just a wording one, since a stubbed session is a materially different (and weaker) posture than real username/password auth for a system running in production with real orders.

### Security Considerations — 72/100

Present:
- Authentication addressed explicitly and scoped down deliberately (PP-003: username/password or stubbed session; full SSO/OIDC explicitly deferred to the parent PRD).
- Input validation addressed explicitly (PP-004: parameterized queries / input validation on order submission) — stronger and more concrete here than in the parent PRD, which deferred input validation entirely to a later task.
- Data protection in transit addressed explicitly (TLS for all requests).
- Audit logging addressed explicitly and concretely (who/what/when, for order creation and status changes).
- Compliance scope is not silently dropped: GDPR/PCI-DSS/food-traceability are explicitly named and explicitly deferred to the parent PRD, rather than omitted without comment.

Gaps:
- Authorization is not addressed. PP-005 implies an admin capability distinct from customer access ("Admin can designate which customer accounts and which single supplier are active"), but no requirement specifies how admin access is authenticated/authorized, or what a customer is prevented from doing (e.g., viewing another customer's orders).
- Data protection at rest is not addressed at all — TLS in transit is specified, but there is no statement on how order/customer data is protected at rest, even at a "baseline hygiene" level, for a pilot running in production.
- Compliance requirements are entirely deferred to the parent PRD with no baseline position for the pilot itself — reasonable for a narrowed pilot, but worth an explicit PO confirmation that no GDPR-relevant personal data is handled by the pilot cohort in a way that would require baseline compliance now rather than later.

## Out of Scope / Relationship to Parent Initiative — not separately scored, but reviewed

Both sections are well-formed and reduce scope-creep risk: the Out of Scope list enumerates seven explicit exclusions each tied back to a specific parent-PRD scope item (multi-supplier, SSO/OIDC, push-based status, native mobile, BI/reporting, compliance posture, HA infra), and the "Relationship to Parent Initiative" section correctly states this pilot does not close or replace parent FR-002. No gaps identified in these two sections.

## Outstanding PO Sign-Off Items

1. PP-002's 60-second status-refresh latency target — currently **[DRAFT — confirm with PO]**.
2. PP-003's authentication mechanism — confirm whether the pilot's production deployment uses real username/password auth, a stubbed session, or an environment-dependent mix of the two.
3. Pain points for all three personas — currently unstated (Business Customer) or entirely absent (Pilot Supplier, Internal Admin/QA).
4. Authorization model — confirm what, if anything, distinguishes admin access from pilot-customer access, and whether one customer can see another's orders.
5. Data-at-rest protection baseline — confirm whether any at-rest protection (even minimal) is expected for this pilot, or whether it is genuinely deferred to the parent PRD's encryption-at-rest hardening.
6. Compliance applicability — confirm no GDPR-relevant personal data handling occurs in the pilot cohort in a way that would require baseline compliance ahead of the parent PRD's full compliance posture.

## Recommended Actions

1. Resolve the PP-002 draft latency number with the PO; remove the **[DRAFT]** marker once confirmed.
2. Pick one authentication mechanism for PP-003 (real auth vs. stubbed session) rather than leaving both as acceptable — this affects both security posture and QA test design.
3. Add a one-line pain point for each of the three personas, matching the depth already established in the parent PRD's persona section.
4. Add an explicit authorization statement to PP-004 or PP-005 (e.g., "customers can only view their own orders; only Admin role can change pilot cohort/supplier configuration").
5. Add a one-line explicit position on data-at-rest protection for the pilot, even if the position is "no additional protection beyond platform defaults, hardening deferred to parent PRD" — so it is a stated decision rather than an absence.

## Questions for Product Owner

1. Is the PP-002 status-refresh target of 60 seconds acceptable, or does the pilot cohort need a tighter number?
2. Should PP-003 use real username/password authentication in the pilot's production deployment, a stubbed session, or does the choice depend on environment (e.g., stubbed only in a lower environment, real auth in production)?
3. What are the actual pain points for the Business Customer, Pilot Supplier, and Internal Admin/QA personas — is "manual/offline follow-up" the full picture, or are there others (e.g., visibility into order status internally, supplier-side friction receiving orders)?
4. Beyond Admin being able to configure the pilot cohort/supplier, is any authorization boundary needed between customers (e.g., can one pilot customer see another's orders), or is the cohort trusted/small enough that this isn't a concern for the pilot?
5. Is any data-at-rest protection expected for pilot order/customer data, or is this genuinely acceptable to defer entirely to the parent PRD given the pilot's limited cohort and 4-week scope?
6. Can you confirm the pilot cohort's data does not trigger GDPR obligations that would need to be addressed before the parent PRD's full compliance posture lands?

## Next Steps

Overall completeness (77%) is above the 70% validation threshold — the PRD is marked `validated` in its frontmatter. The gaps identified (persona pain points, PP-003's dual authentication option, authorization boundary, data-at-rest position, and the PP-002 draft number) do not block moving forward, since the pilot's narrow scope and 4-week timebox make each of them a small, well-defined confirmation rather than a structural rewrite. Recommend resolving the six PO sign-off items above — particularly the PP-003 authentication choice and the PP-002 latency number, since both directly affect implementation and QA test design — before or early in the 4-week build, rather than deferring them to `/PRDEnrich`.
