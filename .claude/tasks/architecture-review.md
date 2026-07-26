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
6. Produce a single system-level HLD as a Mermaid diagram (major components, external systems, data flow) — components blocked by an open decision are drawn and labeled `TBD`, not omitted
7. Produce an ADR for each decision with more than one viable option (context, decision, alternatives considered, consequences)
8. Cross-check the design against the PRD's NFRs and the feasibility report's flagged risks
9. Score `approvalStatus`: `APPROVED` only if no HIGH/Critical severity item is unresolved

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
  "hldDiagram": "flowchart TB\n    Customer -->|places order| API[OrderPilot API]\n    API --> DB[(SQL Server)]\n    Admin -->|manual status update| API",
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

### `generateLLD`

Produce per-module low-level diagrams (class diagram + primary-flow sequence diagram) for a PRD's design or its actual implementation.

**Steps:**
1. Load the PRD and, if it exists, its approved architecture doc for the component list; otherwise use the `architectureDesign.components` from a `design` run
2. Determine module scope: use `modules` input if supplied, otherwise every component in the design/implementation
3. For each module:
   a. If `implementationPaths` is supplied or the PRD status indicates an implementation exists, read the actual source file(s) for that module — do not infer behavior from the module's name alone
   b. Produce a Mermaid `classDiagram` for the module's data/domain shape (entities, key fields, relationships it owns or depends on)
   c. If the module has a primary flow (e.g., a controller action, a service method invoked by an API call), produce a Mermaid `sequenceDiagram` tracing that flow across the modules it touches
   d. If no source exists and no design description covers the module, skip it — do not invent structure
4. Label each module's diagrams `grounded in <path>` (implemented) or `proposed, pre-implementation` (design-only)

**Inputs:**
- `prd`: PRD slug or ID (string)
- `modules`: Module/component names to generate LLDs for (array of strings, optional — defaults to all)
- `implementationPaths`: Source paths to read for grounding (array of strings, optional)

**Outputs:**
```json
{
  "prd": "order-placement-pilot",
  "modules": [
    {
      "name": "OrderService",
      "groundedIn": "src/OrderPilot.Api/Services/OrderService.cs",
      "classDiagram": "classDiagram\n    class Order {\n        +Guid Id\n        +Guid CustomerId\n        +OrderStatus Status\n    }",
      "sequenceDiagram": "sequenceDiagram\n    Customer->>OrdersController: POST /api/orders\n    OrdersController->>OrderService: CreateOrderAsync\n    OrderService->>DB: SaveChangesAsync"
    }
  ],
  "skipped": [
    { "name": "MonitoringService", "reason": "No source exists and no design description covers this module" }
  ]
}
```

### `generateHLD`

Standalone HLD refresh, for when the architecture design hasn't changed but the diagram needs regenerating (e.g., after a `conformance` run finds new components) without re-running the full `design` operation.

**Steps:**
1. Load the PRD's current architecture design (from a prior `design` run) or, if none exists, its enrichment/feasibility data
2. Identify major components, external systems, and data flows
3. Produce a single Mermaid diagram; label any component blocked by an open decision as `TBD`

**Inputs:**
- `prd`: PRD slug or ID (string)

**Outputs:**
```json
{
  "prd": "supply-chain-solutions-for-logistics",
  "hldDiagram": "flowchart TB\n    subgraph Client\n      Web[Web App]\n      Mobile[Native Mobile - TBD]\n    end\n    Web --> EnterprisePlatform\n    Mobile --> EnterprisePlatform\n    EnterprisePlatform --> DataLayer[(Data Layer - TBD engine)]\n    EnterprisePlatform --> Monitoring[Monitoring - TBD build/buy]"
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
| Module has no source and no design description (generateLLD) | Skip it, record in `skipped` with a reason — do not invent structure |
| No architecture design or enrichment/feasibility data available (generateHLD) | Warn and suggest running `design` first |

## Dependencies

- **technical-feasibility**: For feasibility data and team/timeline constraints
- **prd-enrichment**: For codebase patterns and risk flags
- **context-loader**: For scanning the actual codebase in `conformance` and `generateLLD` modes
