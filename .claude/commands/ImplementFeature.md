---
description: Implement a feature end-to-end with specification, code generation, and task updates
---

# ImplementFeature

Full feature implementation workflow from requirements to code delivery.

## Purpose

Orchestrates the complete feature implementation process: discovers requirements, generates specifications, routes work to appropriate agents, and updates tracking systems.

## Arguments

- `feature`: Feature name or description (required)
- `linearIssue`: Task management issue ID (optional, for linking)
- `stack`: Technology requirements array (optional)
  - `database` - Requires schema changes
  - `api` - Requires API endpoints
  - `ui` - Requires UI components
- `skipSpec`: Skip specification generation (default: false)

## Execution

1. Invoke `feature-workflow` task with action: `implement`
   - Discovery: Finds requirements from PRDs, documentation, task management. If the PRD has an approved architecture doc (`{prd}-architecture.md`, status `architecture-approved`), load it and use its component/data-model/API design and ADRs directly instead of re-deriving architecture ad hoc.
   - Specification: Generates or loads feature spec
   - Approval: Presents plan for confirmation
   - Implementation: Routes to agents (schema-designer, code-writer)
   - Update: Syncs task management with progress

2. Display:
   - Requirements summary
   - Implementation plan
   - Generated code locations
   - Next steps

## Prerequisites

- Requirements exist in PRD, documentation, or task management
- Session active (recommended)
- For non-trivial features: run `/ArchitectureReview` first so architecture decisions are made deliberately and traced to requirements, rather than decided ad hoc mid-implementation

## Output Files

- `/docs/specs/{feature-name}.md` (if generated)
- Code files as specified

## Example

```
/ImplementFeature feature="User profile integration" linearIssue="PROJ-211" stack=["database", "api", "ui"]
```

## Related

- `/ArchitectureReview` - Run before this command for non-trivial features; provides the design this command should implement against
- `/FixBug` - For bug fixes
- `/RefactorCode` - For refactoring
- `/ReviewCode` - Review implementation

## Tasks Invoked

- `feature-workflow.implement`
- `requirement-discovery.discover`
- `context-loader.loadForFeature`
- `mcp-sync.syncLinear`

## Agents Used

- `schema-designer` - Database design (if stack includes database)
- `code-writer` - Code generation

## Approval Checkpoints

The workflow pauses for approval:
1. After specification generation
2. Before database migrations
3. Before major file creation
