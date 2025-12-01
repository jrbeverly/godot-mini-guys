<!-- DO NOT EDIT: Generated from the repository structure by an external tool. -->

# Code Map

This document is a navigation guide for `Labs/godot-mini-guys`. The areas listed
below are the ones this repository actually has; an area it does not use is
simply absent.

Open the local `README.md` of an area for detail. This page only says what each
area is for and where to go next.

## Structure

The tree below is the shape these conventions expect, not a directory listing.
Names in angle brackets are placeholders for whatever a repository puts there,
and only the top-level areas this repository uses are shown.

```text
[Project Root]/
├── src/                               # Backend services and APIs (logical grouping)
│   ├── <service-name>/                # One deployable service
│   │   ├── <Service>.Api/             # API host
│   │   ├── <Service>.Application/     # Application and business logic
│   │   ├── <Service>.Domain/          # Domain models and core types
│   │   ├── <Service>.Infrastructure/  # Database and external integrations
│   │   ├── <Service>.Tests/           # Test projects, siblings at the service root
│   │   ├── config/                    # Non-secret service configuration
│   │   └── Dockerfile                 # How this service is built for deployment
│   └── <another-service-name>/        # Additional services, same shape
│
├── tests/                             # Cross-boundary tests; component-local tests live with the component
│   └── <area>/                        # One test area: integration, architecture, contract
│
├── .devcontainer/                     # Development container configuration
├── Makefile                           # The repository command surface; rules live in scripts/build/
├── .linter/                           # Shared lint, format, and scanner configuration
├── .gitea/                            # Gitea Actions workflows and local actions
│
├── docs/                              # Documentation (logical grouping)
│   ├── designs/                       # Designs, diagrams, decision records
│   ├── specs/                         # Feature and API specifications
│   └── pdlc/                          # Durable product context: problems, constraints, decisions
│
├── .descriptor/                       # Machine-readable repository identity for catalog systems
│
├── ARCHITECTURE.md                    # Architectural overview: high level system design
├── CODEMAP.md                         # This page: how the repository is organised
├── VISION.md                          # What the system solves, and for whom
└── README.md                          # Project overview and quick start
```

## Quick Lookup

- Backend Services: `src`
- Test Suites: `tests`
- Development Container: `.devcontainer`
- Command Surface: `Makefile`
- Linting And Static Analysis: `.linter`
- Continuous Integration: `.gitea`
- Documentation: `docs`
- Product Development Life Cycle: `docs/pdlc`
- Catalog Metadata: `.descriptor`

## Product Code

### Backend Services

Backend services live under `src/`. Each child directory is one service
boundary and owns its own runtime projects, configuration, and persistence.

## Tests And Validation

### Test Suites

Cross-boundary tests live under `tests/`. Tests that belong to a single
component stay beside that component instead.

## Development Environment And Tooling

### Development Container

The canonical development environment is defined in `.devcontainer/`. Work on this
repository is expected to happen inside that container, so required toolchains
come from it rather than from ad hoc host installs.

Read:

- [.devcontainer/README.md](.devcontainer/README.md)
- [.devcontainer/lifecycle/README.md](.devcontainer/lifecycle/README.md)

### Command Surface

The root `Makefile` is the command surface for this repository. It is
deliberately generic: the concrete component lists live in
`scripts/build/repository.mk`, and the shared rules come from the make modules
beside it.

### Linting And Static Analysis

Linting, formatting, and scanner configuration lives under `.linter/`. Local
tooling, the development container, and CI all read the same files from here, so
a check behaves identically wherever it runs.

Read:

- [.linter/README.md](.linter/README.md)

### Continuous Integration

Workflow and action definitions live under `.gitea/`. Workflows orchestrate
the repository commands; they should not reimplement the work those commands do.

Read:

- [.gitea/README.md](.gitea/README.md)
- [.gitea/workflows/README.md](.gitea/workflows/README.md)
- [.gitea/actions/README.md](.gitea/actions/README.md)

## Documentation And Metadata

### Documentation

Repository documentation lives under `docs/`. This is the place for
operating guides, architecture notes, and process documents that explain how the
repository works.

### Product Development Life Cycle

Durable product context lives under `docs/pdlc/`. It records the problems,
constraints, and decisions behind the repository, and is deliberately not a
place for short-lived status or task tracking.

Read:

- [docs/pdlc/README.md](docs/pdlc/README.md)

### Catalog Metadata

Machine-readable repository identity lives under `.descriptor/`. Catalog and
discovery systems read it; it has no effect on build, test, or runtime
behaviour.

Read:

- [.descriptor/README.md](.descriptor/README.md)
