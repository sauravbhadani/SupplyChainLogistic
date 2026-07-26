---
generatedAt: 2026-07-26
command: /PRDSequence
weights: balanced
initiatives:
  - supply-chain-solutions-for-logistics
  - order-placement-pilot
---

# Sequence Report: supply-chain-solutions-for-logistics + order-placement-pilot

Strategy: **balanced** (equal weighting across risk, value, dependency depth, resource fit)
Initiatives: 2

## A note on why this doesn't look like a typical sequence report

Standard `/PRDSequence` output assumes N roughly-independent initiatives competing for a shared team's attention, where the job is to order peers. That is not the shape of this problem, and forcing it into the peer template would misrepresent the actual decision in front of the reader.

`order-placement-pilot` is not an alternative body of work to `supply-chain-solutions-for-logistics` — it is a **deliberately narrowed subset of one requirement (FR-002)** inside the parent PRD, carved out by the parent's own `/PRDFeasibility` assessment specifically because the parent's full scope was rejected for the team that exists (2 BE, 1 FE, 1 QA, 4 weeks: LOW feasibility, ~6-10x capacity gap, zero mobile/data/DevOps specialists). Both initiatives are staffed from the **same four people**. There is no world in which they run as competing parallel streams for that team — the pilot *is* the only piece of the parent that this team can execute right now, not a separate initiative racing it for capacity.

So the real sequencing question isn't "which of these two should we do first" — it's "what does this specific team do now, given that only one of these two things is actually buildable by them today." The recommendation below answers that directly, and then addresses the parent's full scope as a separate, gated track rather than a normal "position 2."

## Recommended Order

### 1. order-placement-pilot — Score: 84/100

**Rationale:** This is the only one of the two initiatives that is actually resourced and schedulable for the team and timeline in hand. Feasibility: **PROCEED (conditional)**, MEDIUM confidence, 63 of 80 person-days (~79% utilization, ~21% margin — a genuine capacity fit, not an optimistic label). Scope (single order type, single supplier, basic auth, polling, baseline security hygiene) was purpose-built to match this team's actual skillset — no missing specialist roles, no architecturally novel component. The two HIGH-severity open items (PP-003 auth mechanism, customer/admin authorization boundary) are cheap, fast PO/eng decisions, not structural capability gaps, which is what keeps this at PROCEED rather than DEFER. QA has zero slack (20/20 person-days), and the supplier fulfillment-endpoint contract is an external unknown — both are named risks, not blockers, provided they're managed from day 1.

**Score breakdown:** risk 78 (two HIGH items, but both cheap/fast to close) · value 80 (ships a real production reference implementation in 4 weeks; de-risks the ordering workflow early) · dependency depth 95 (no internal dependencies; one external contract to confirm) · resource fit 95 (deliberately sized to this exact team).

### 2. supply-chain-solutions-for-logistics (full scope) — Score: 24/100 — *gated, not "next"*

**Rationale:** Feasibility: **REJECT** for the stated team/timeline. LOW feasibility, ~101-151 person-weeks required against 16 person-weeks available (~6-10x gap), zero mobile/data/DevOps/security specialists on the team, and the PRD's own stated Phase 1 priority (native mobile rewrite) cannot be started at all by this team regardless of timeline. This is not "do it after the pilot" in the normal sequencing sense — no amount of reordering fixes a staffing/scope mismatch this large. Before this initiative re-enters any build sequence, it needs: (a) resolution of 6 outstanding PO decisions (three latency thresholds, compliance scope, persona/phase priority, input-validation/session-management position), (b) a discovery spike against the out-of-repo legacy Xamarin app and Enterprise Platform, and (c) closing a staffing gap to ~10-14 people sustained over 2-3 quarters. The pilot in position 1 does **not** unblock or shrink this — FR-002 remains fully open at its original scope in the parent PRD; the pilot's code/learnings are, at best, an input to the parent's eventual FR-002 build, not progress against it.

**Score breakdown:** risk 15 (RED across staffing, undefined architecture, unavailable legacy source) · value 90 (full platform is high strategic value if delivered — this is the only component pulling the score off zero) · dependency depth 10 (six unresolved PO decisions + an undone discovery spike gate everything) · resource fit 5 (current team is ~25-35% of required headcount, and missing three entire skill categories).

## Dependency Graph

```
supply-chain-solutions-for-logistics (PARENT, FULL SCOPE)
  [REJECT @ 2BE/1FE/1QA/4wk — LOW feasibility, ~6-10x capacity gap]
  │
  ├── gated by: 6 unresolved PO decisions
  │      (3 latency thresholds, compliance scope [PCI-DSS/food-traceability],
  │       persona/phase priority, input-validation & session-mgmt position)
  │
  ├── gated by: discovery spike (out-of-repo legacy sources)
  │      (existing Xamarin app, existing Enterprise Platform — code unavailable,
  │       feature-parity surface undiscovered)
  │
  ├── gated by: staffing gap closure
  │      (needs ~10-14 people incl. 2 native mobile, 1 data engineer,
  │       1 DevOps/SRE, 1 security engineer, tech lead — current team = 4 generalists)
  │
  └── carved subset (FR-002 only, narrowed) ──▶ order-placement-pilot
                                                  [PROCEED (conditional) @ same 2BE/1FE/1QA/4wk
                                                   — MEDIUM feasibility, 63/80 pd]
                                                  │
                                                  ├── blocked by: PP-003 auth mechanism decision (HIGH, cheap)
                                                  ├── blocked by: authorization boundary decision (HIGH, cheap)
                                                  └── blocked by: pilot supplier API contract (external, MEDIUM,
                                                                  outside team's control)

           ┌─────────────────────────────────────────────────────────┐
           │  feedback (NOT a dependency-satisfying relationship):    │
           │  pilot code/learnings ──▶ input to parent's eventual     │
           │  FR-002 build. FR-002 remains fully OPEN in the parent   │
           │  PRD regardless of pilot outcome.                        │
           └─────────────────────────────────────────────────────────┘
```

Key structural point the graph is trying to make: the parent's three gating tracks (PO decisions, discovery spike, staffing) do **not** require the 2BE/1FE/1QA dev team to execute — they sit with product/PO and engineering leadership. That means they can run **in calendar parallel** with the pilot build without resource contention, since they don't compete for the same four people. This is the one form of "parallelism" that legitimately exists in this sequencing problem — not two initiatives running side-by-side on the same team, but a dev-team workstream (pilot) and a decisions/staffing workstream (parent preconditions) running concurrently on different people.

## Alternative Sequences

### Alt A: Wait-for-clarity (risk-minimized on architecture, not on delivery)

**Sequence:** [resolve 6 PO decisions + run discovery spike] → *then* order-placement-pilot → *then* parent full build (once staffed)

**Trade-off:** Building the pilot now (as recommended) locks in some decisions early — most notably PP-003's auth mechanism and the polling-based status architecture — before the parent's eventual SSO/OIDC and real-time (push/streaming) architecture decisions are made. If the parent later resolves those differently than the pilot assumed, some pilot backend work (auth, status mechanism) is thrown away rather than reused. Waiting avoids that rework risk entirely. The cost: 0 production value shipped for however long the PO-decision cycle and discovery spike take (the feasibility report estimates a 2-3 week "sprint zero" at minimum, plausibly longer), and no early validation of the core ordering workflow with real users — the exact de-risking the pilot exists to provide is deferred along with everything else.

### Alt B: Parent's stated priority first (mobile-first, as the PRD itself directs)

**Sequence:** Phase 1 native mobile rewrite → Enterprise Platform / FR-002 → data/monitoring/infra

**Trade-off:** The parent PRD explicitly labels mobile as "Phase 1 — priority." Honoring that literal priority ordering is not viable with the current team: zero native iOS/Android engineers exist on the 2BE/1FE/1QA roster, so this isn't "tight," it's not startable at all — no reordering or reprioritization within the current team fixes a missing skill set. This alternative is included mainly to show why it's rejected rather than as a live option: pursuing it would require hiring/contracting at least 2 dedicated native engineers before any sequencing question is even meaningful, which is a staffing decision that sits outside this sequencing exercise, not a reordering of the same team's work.

## Summary Recommendation

Run `order-placement-pilot` now with the existing team — it is the only buildable, resourced work available. In parallel (not in sequence), route the parent's six PO decisions, the legacy discovery spike, and the staffing conversation to product/engineering leadership, since none of that requires the dev team's time. Do not treat the pilot as progress against the parent's FR-002 — it closes nothing in the parent PRD — and do not schedule the parent's full build until its staffing gap (4 people vs. ~10-14 needed) is actually closed.
