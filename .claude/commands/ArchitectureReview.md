---
description: Produce and review a pre-implementation architecture design for a feasibility-assessed PRD, or check an implementation for drift against it
---

# ArchitectureReview

Design-stage gate between `/PRDFeasibility` and `/ImplementFeature`. Produces a reviewed architecture design and ADRs before code is written, so design mismatches are caught before implementation rather than after.

## Purpose

Closes the gap between "feasibility says proceed" and "code gets written." Produces a component/data-model/API design traced to the PRD's ID-tagged acceptance criteria, generates ADRs for major decisions, and explicitly surfaces any decision that's still open rather than letting implementation start on an assumption. Can also run in `conformance` mode after implementation to check whether the built code has drifted from the approved design or from the PRD's stated acceptance criteria.

## Arguments

- `prd`: PRD identifier — feature slug or Linear Initiative ID (required)
- `decisions`: Architecture decisions already made, to incorporate directly instead of re-asking (optional, e.g. `decisions="stack=ASP.NET Core + SQL Server; auth=JWT + Identity"`)
- `mode`: `design` (default) — produce a new architecture design | `conformance` — compare existing code against the previously approved design

## Execution

### `design` mode (default)

1. Load PRD from `/docs/planning/prds/{prd}.md`
   - Verify PRD status is `feasibility-assessed` or later
   - Load its enrichment and feasibility reports for risk flags and constraints

2. Invoke `architecture-review.design` task
   - Extracts ID-tagged acceptance criteria as traceability targets
   - Incorporates any `decisions` supplied
   - Identifies remaining open architecture decisions

3. If open decisions remain and were not supplied via `decisions`
   - Ask the user directly, the same way `/PRDIntake`/`/PRDValidate` surface questions for the PO
   - Do **not** proceed to write the design with an invented decision

4. Generate the architecture design, ADRs, and requirement-traceability table via the `architecture-reviewer` agent

5. Score approval status
   - If `NEEDS_DECISIONS`: halt, surface the open questions, do not mark the PRD ready for implementation
   - If `APPROVED`: proceed to store the report and update PRD status

6. Store report in `/docs/planning/prds/{prd}-architecture.md`
   - Update PRD status to `architecture-approved` (or leave at `architecture-review` if decisions are still pending)
   - Link the report in PRD frontmatter

### `conformance` mode

1. Load the PRD and its previously approved architecture doc (if none exists, fall back to diffing directly against the PRD's acceptance criteria)
2. Invoke `architecture-review.checkConformance` task, scanning the actual codebase
3. Report drift findings: requirements whose acceptance criteria no longer match what was built
4. Recommend either a PRD update or a code fix per finding — do not assume which side is correct without checking whether the divergence was a deliberate decision

## Prerequisites

- PRD feasibility-assessed via `/PRDFeasibility` (status: `feasibility-assessed` or later)
- `.claude/tasks/architecture-review.md` task definition available
- For `conformance` mode: implementation code exists to scan

## Output Files

- `/docs/planning/prds/{prd}-architecture.md` (architecture design + ADRs, `design` mode)
- `/docs/planning/prds/{prd}.md` (updated status in frontmatter, `design` mode)
- `/docs/planning/prds/{prd}-architecture-conformance-{date}.md` (drift report, `conformance` mode)

## Output Format

```
### Architecture Review: {prd}
Mode: design

### Approval Status: APPROVED

### Architecture Design
{component breakdown, data model, API surface}

### ADRs
1. **ADR-001: {decision title}**
   - Decision: ...
   - Alternatives considered: ...
   - Consequences: ...

### Requirement Traceability
| ID | Design Element | Status |
|----|-----------------|--------|
| PP-001 | OrderService.CreateOrderAsync | Satisfied |

### Risks
- HIGH: ...

### Recommendation
Ready for /ImplementFeature.
```

## Example

```
/ArchitectureReview prd="order-placement-pilot"
```

```
/ArchitectureReview prd="order-placement-pilot" decisions="auth=JWT + Identity; status-updates=Admin-managed, no supplier integration"
```

```
/ArchitectureReview prd="order-placement-pilot" mode="conformance"
```

## Related

- `/PRDFeasibility` - Run before this command; provides the constraints this review checks against
- `/PRDEnrich` - Provides the risk flags and patterns this review builds on
- `/ImplementFeature` - Run after this command; should consume the approved design instead of re-deciding architecture ad hoc
- `/ReviewCode` - Post-implementation code review; `conformance` mode here complements it by checking design-level drift, not just code quality

## Tasks Invoked

- `architecture-review.design`
- `architecture-review.checkConformance` (conformance mode only)

## Agents Used

- `architecture-reviewer` - Architecture design, ADR generation, and requirement traceability
