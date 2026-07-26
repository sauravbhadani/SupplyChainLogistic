# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Project Overview

SupplyChainLogistic - Supply chain & logistics management system

## Repository Structure

```
SupplyChainLogistic/
├── .claude/            # Claude Workflow System (commands, agents, tasks)
├── .github/workflows/  # CI (code review, security review, .NET build+test)
├── src/
│   └── OrderPilot.Api/         # ASP.NET Core Web API — order-placement-pilot PRD
│       ├── Controllers/        # Auth, Orders (customer), Admin/ (orders, customers, suppliers, audit-logs)
│       ├── Domain/Entities/    # ApplicationUser, Supplier, Order, OrderStatus, AuditLog
│       ├── Dtos/                # Request/response DTOs (Auth, Orders, Admin)
│       ├── Data/                # ApplicationDbContext, Migrations/, Seed/DbSeeder
│       ├── Services/            # OrderService, AdminConfigService, AuditService, TokenService
│       ├── Authorization/        # OrderOwnerOrAdmin policy (customer-scoped order access)
│       └── Extensions/           # DI wiring (Identity, JWT auth, authorization, services)
├── tests/
│   ├── OrderPilot.Api.UnitTests/         # xUnit + EF Core InMemory — service/handler logic
│   └── OrderPilot.Api.IntegrationTests/  # xUnit + WebApplicationFactory + SQLite — real HTTP/relational behavior
├── docs/
│   ├── planning/
│   │   ├── prds/       # Imported/derived PRDs and their validation/enrichment/feasibility reports
│   │   └── ...          # Session plans and current state
│   ├── specs/          # Technical specifications
│   └── reports/        # Generated reports
└── knowledge/
    ├── prd/            # Product requirements documents (templates)
    └── architecture/   # Architecture documentation
```

## Development Guidelines

### Code Style
<!-- Customize based on project language/framework -->
- Follow the established patterns in the codebase
- Use consistent naming conventions
- Keep functions focused and single-purpose
- Write self-documenting code with clear variable names

### Architecture Principles
<!-- Customize based on project architecture -->
- Maintain separation of concerns
- Follow the established module boundaries
- Keep dependencies minimal and explicit
- Prefer composition over inheritance

### Testing Requirements
- Write tests for new functionality
- Maintain existing test coverage
- Use meaningful test descriptions
- Test edge cases and error conditions

### Documentation
- All planning, specs, and report documents MUST go in `/docs/` subfolders
- Reports and generated result documents go in `/docs/reports/`
- Never create markdown documents in root or outside `/docs/` and `/knowledge/`
- PRD source documents go in `/knowledge/prd/`
- Architecture documentation goes in `/knowledge/architecture/`
- Session plans and state files go in `/docs/planning/`
- Technical specifications go in `/docs/specs/`
- Update documentation when changing public APIs

## Workflow Integration

This project uses the Claude Workflow System for development operations.

### Available Commands
<!-- These are populated from the workflow system -->
- `/StartSession` - Initialize a development session
- `/EndSession` - Close session and optionally trigger CI
- `/SessionStatus` - View current session state
- `/SetupProjectMeta` - Reconfigure Linear, Coda, and GitHub integrations
- `/PRDIntake`, `/PRDValidate`, `/PRDEnrich`, `/PRDFeasibility` - PRD intake-through-feasibility pipeline
- `/ArchitectureReview` - Pre-implementation architecture design/review gate (run after `/PRDFeasibility`, before `/ImplementFeature`); `mode="conformance"` checks a built feature against its approved design
- `/ImplementFeature` - Start feature implementation workflow
- `/FixBug` - Start bug investigation workflow
- `/ReviewCode` - Request code review
- `/GenerateTests` - Generate test specifications

### Session Management
- Start each development session with `/StartSession`
- Session state is tracked in `/docs/planning/session-state.json`
- End sessions with `/EndSession` to persist progress

### Project Integrations

This project is configured with the following integrations:

| Service | Configuration |
|---------|---------------|
| **Linear** | Not configured — run `/SetupProjectMeta` |
| **Coda** | Not configured — run `/SetupProjectMeta` |
| **GitHub** | Repository: `sauravbhadani/SupplyChainLogistic` |

To reconfigure integrations, run `/SetupProjectMeta`.

### Task Management Integration
<!-- Configure based on your task management tool -->
- Issues are tracked in Linear (not yet configured)
- Use `/SyncLinear` or equivalent to sync task status
- Reference issue IDs in commits when applicable
- All issues are scoped to the configured Linear project

### Linear Issue Description Standard

All Linear issues MUST use this description format. Pass descriptions as multi-line strings with real newlines (never escaped `\n`).

```markdown
## Overview

{2-4 sentences explaining what this task is and why it matters. Include context about where it fits in the system.}

{If prior work exists, add a paragraph starting with **Prior work:** describing what's already built.}

## Deliverables

- {Specific item 1 — concrete enough to verify}
- {Specific item 2}
- {Specific item 3}

## Definition of Done

- [ ] {Testable acceptance criterion 1}
- [ ] {Testable acceptance criterion 2}
- [ ] {Testable acceptance criterion 3}
- [ ] {Unit/integration tests written and passing}
```

**Rules:**
- Overview must be at least 2 sentences, never a single line
- Deliverables must list specific, verifiable items (not vague goals)
- Definition of Done must have checkbox items that can be objectively verified
- Always include a testing criterion in Definition of Done
- Reference prior work when extending existing functionality

### CI/CD Integration
<!-- Configure based on your CI/CD setup -->
- Pull requests trigger automated code review
- Security review runs on security-sensitive changes
- Quality gates must pass before merge

## Environment Setup

### Prerequisites
- .NET 8 SDK
- SQL Server / SQL Server Express LocalDB (dev) or Azure SQL (prod) for `OrderPilot.Api`
- `dotnet tool install --global dotnet-ef` (for migrations)

### Installation
```bash
dotnet restore
```

### Development Server
```bash
# ASPNETCORE_ENVIRONMENT=Development picks up src/OrderPilot.Api/appsettings.Development.json
# (LocalDB connection string, dev-only JWT key, seeded dev admin — dev-admin@orderpilot.local)
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/OrderPilot.Api
# Swagger UI at https://localhost:{port}/swagger
```

### Running Tests
```bash
dotnet test
```

### Building
```bash
dotnet build
```

### Database Migrations
```bash
dotnet ef migrations add <Name> --project src/OrderPilot.Api --startup-project src/OrderPilot.Api
dotnet ef database update --project src/OrderPilot.Api --startup-project src/OrderPilot.Api
```

## Key Directories

| Directory | Purpose |
|-----------|---------|
| `/src/OrderPilot.Api/` | The order-placement-pilot ASP.NET Core Web API |
| `/tests/` | Unit and integration test projects |
| `/docs/planning/` | Session plans and current state |
| `/docs/planning/prds/` | Imported/derived PRDs and their validation/enrichment/feasibility reports |
| `/docs/specs/` | Technical specifications |
| `/knowledge/prd/` | Product requirements documents (templates) |
| `/knowledge/architecture/` | Architecture documentation |

## Important Conventions

### Commit Messages
Follow conventional commits format:
```
type(scope): description

[optional body]

[optional footer]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

### Branch Naming
- Features: `feature/{issue-id}-{description}`
- Bug fixes: `fix/{issue-id}-{description}`
- Refactoring: `refactor/{description}`

### Pull Request Process
1. Create feature branch from main
2. Make changes and commit with meaningful messages
3. Push and create pull request
4. Address review feedback
5. Merge when approved and CI passes

## Additional Context

<!-- Add project-specific context, external integrations, or special considerations -->
This repository was initialized with the Claude Workflow System (base variant) and pushed to `https://github.com/sauravbhadani/SupplyChainLogistic` (private). Run `/SetupProjectMeta` to configure Linear/Coda once ready.

The first code in the repo is `src/OrderPilot.Api` — implementation of the `order-placement-pilot` PRD (`docs/planning/prds/order-placement-pilot.md`), a deliberately narrow slice of the larger `supply-chain-solutions-for-logistics` initiative (see `docs/planning/prds/supply-chain-solutions-for-logistics.md` and its enrichment/feasibility reports for why: the full scope was rejected as infeasible for a 2 BE/1 FE/1 QA team in 4 weeks). Key architecture decisions for this pilot: real username/password auth via ASP.NET Core Identity + JWT (not SSO — that's parent-PRD scope), order status is updated manually by an Admin (no supplier-side integration at all), and customer-scoped authorization is enforced via a resource-based `OrderOwnerOrAdmin` policy — see `docs/planning/sequence-report-2026-07-26.md` for how this pilot relates to the parent initiative.
