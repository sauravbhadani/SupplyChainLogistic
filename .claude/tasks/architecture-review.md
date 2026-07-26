---
name: architecture-review
description: Pre-implementation architecture design, review, and post-implementation conformance checking
---

# Architecture Review Task

Sits between `technical-feasibility` and implementation. Produces a reviewed architecture design for a feasibility-assessed PRD before code is written, and can later re-check already-implemented code against that design to catch drift (e.g., an acceptance criterion describing an integration the build doesn't actually have).

## Operations

### `design`

Produce an architecture design and ADRs for a feasibility-assessed PRD.

**Steps:**
1. Load the feasibility-assessed PRD, its enrichment data, and its feasibility report
2. Extract every ID-tagged acceptance criterion as a traceability target
3. Identify architecture-relevant decisions still open (from enrichment risk flags and feasibility prerequisites — e.g., auth mechanism, integration boundaries, data model shape)
4. For any decision supplied via the `decisions` input, incorporate it directly; for any decision still open, generate an explicit question rather than guessing
5. Produce a component/data-model/API design, mapping each element to the requirement IDs it satisfies
6. Produce an ADR for each decision with more than one viable option (context, decision, alternatives considered, consequences)
7. Cross-check the design against the PRD's NFRs and the feasibility report's flagged risks
8. Score `approvalStatus`: `APPROVED` only if no HIGH/Critical severity item is unresolved

**Inputs:**
- `prd`: PRD slug or ID (string)
- `decisions`: Map of pre-made architecture decisions to incorporate (object, optional — e.g. `{"stack": "ASP.NET Core + SQL Server", "auth": "JWT + Identity"}`)

**Outputs:**
```json
{
  "prd": "order-placement-pilot",
  "approvalStatus": "APPROVED",
  "architectureDesign": {
    "components": ["OrdersController", "AdminOrdersController", "OrderService", "AuditService"],
    "dataModel": ["ApplicationUser", "Supplier", "Order", "AuditLog"],
    "apiSurface": ["POST /api/orders", "GET /api/orders", "PATCH /api/admin/orders/{id}/status"]
  },
  "adrs": [
    {
      "id": "ADR-001",
      "title": "Order status updates are Admin-managed, not supplier-integrated",
      "context": "Feasibility report flagged the supplier fulfillment-endpoint contract as unconfirmed and the single biggest schedule risk.",
      "decision": "No outbound call to any supplier system. Admin advances order status manually via a PATCH endpoint.",
      "alternativesConsidered": ["Poll supplier API", "Supplier webhook callback"],
      "consequences": "Removes the pilot's largest schedule risk; parent PRD's full supplier integration remains a separate, later workstream."
    }
  ],
  "traceability": [
    { "requirementId": "PP-001", "designElement": "OrdersController.Create + OrderService.CreateOrderAsync", "status": "satisfied" },
    { "requirementId": "PP-002", "designElement": "GET /api/orders (client polling)", "status": "satisfied" }
  ],
  "risks": [
    { "severity": "high", "description": "No authorization boundary specified in the PRD text; must be resolved before order-view endpoints are built." }
  ],
  "openQuestions": [
    "Should customer authentication be real username/password or a stubbed session for this pilot?"
  ]
}
```

### `checkConformance`

Compare already-implemented code against a previously approved architecture design (or, if none exists, against the PRD's stated acceptance criteria directly).

**Steps:**
1. Load the approved architecture doc and ADRs for the PRD, if one exists
2. Scan the actual codebase for the relevant components, endpoints, and integration points
3. Diff the real implementation against each ADR and each requirement-ID mapping from the `design` operation
4. For any requirement whose acceptance criterion no longer matches what was built, record a drift finding rather than silently ignoring it
5. Recommend either a PRD/acceptance-criteria update or a code fix — do not assume which side is "correct" without checking which one reflects a deliberate decision

**Inputs:**
- `prd`: PRD slug or ID (string)
- `codePaths`: Paths to scan (array of strings, optional — defaults to the repository's src/ directory)

**Outputs:**
```json
{
  "prd": "order-placement-pilot",
  "driftFindings": [
    {
      "requirementId": "PP-001",
      "expected": "Order reaches the supplier's fulfillment endpoint",
      "actual": "No outbound supplier-integration code exists anywhere in src/OrderPilot.Api; status is Admin-managed only",
      "recommendation": "Update PP-001's acceptance criteria to match the implemented Admin-managed design — this was a deliberate decision, not an oversight."
    }
  ]
}
```

## Configuration

Approval thresholds:
- **APPROVED**: no Critical or High severity risk/open-question remains
- **NEEDS_DECISIONS**: one or more High-severity open questions remain — halt, surface them, do not proceed to implementation
- **REJECTED**: the design cannot satisfy a stated acceptance criterion and no viable design change was identified

## Error Handling

| Error Type | Action |
|------------|--------|
| PRD not feasibility-assessed | Warn and suggest running `/PRDFeasibility` first |
| No enrichment/feasibility data available | Proceed with a lighter design pass, flag reduced confidence in the output |
| Architecture decision genuinely unknown and not supplied | Add to `openQuestions`, do not guess |
| No prior approved design exists (conformance mode) | Fall back to diffing directly against the PRD's acceptance criteria |

## Dependencies

- **technical-feasibility**: For feasibility data and team/timeline constraints
- **prd-enrichment**: For codebase patterns and risk flags
- **context-loader**: For scanning the actual codebase in conformance mode
