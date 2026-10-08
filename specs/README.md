# Specifications

This folder holds **feature specs** for the TEKKEN 8 Mod Manager. We work
**spec-driven**: every non-trivial change starts as a written spec here *before*
any code is written. The spec is the single source of truth that drives the
implementation, the tests, and the review.

## Why spec-driven

- **Think before coding** — design decisions and edge cases are settled on paper, where they're cheap to change.
- **Shared intent** — the spec explains *what* and *why*; the code only shows *how*.
- **Traceable** — every commit, test, and PR can point back to a spec ID.
- **Testable** — the acceptance criteria in a spec become the test checklist.

## Folder layout

```
specs/
  README.md                      <- this file (the workflow)
  TEMPLATE.md                    <- copy this to start a new spec
  0000-current-state-catalog.md  <- baseline: what the app already does
  0001-multi-root-support.md
  0002-<next-feature>.md
  ...
```

The **baseline catalog** (`0000-current-state-catalog.md`) records the
already-shipped feature set and flags which areas still lack a dedicated spec
("spec debt"). Consult it before starting new work.

## Numbering & naming

- Specs are numbered sequentially, zero-padded to 4 digits: `0001`, `0002`, ...
- Filename: `NNNN-kebab-case-title.md`.
- The number never changes once assigned, even if the title is edited.

## Status lifecycle

Each spec carries a `Status` field in its header:

| Status        | Meaning                                                        |
|---------------|----------------------------------------------------------------|
| `Draft`       | Being written / discussed. Not ready to build.                 |
| `Approved`    | Agreed. Ready to implement.                                    |
| `In Progress` | Implementation underway.                                       |
| `Shipped`     | Merged and released. Records the version it shipped in.        |
| `Superseded`  | Replaced by a newer spec (link to it).                         |
| `Rejected`    | Decided against (keep for the historical record).              |

## Workflow

1. **Create** — copy `TEMPLATE.md` to `NNNN-title.md`, fill in the header and the *Summary*, *Motivation*, and *Requirements* sections. Status = `Draft`.
2. **Review** — discuss and refine. When agreed, set Status = `Approved`.
3. **Implement** — write code + tests to satisfy the *Acceptance Criteria*. Status = `In Progress`.
4. **Verify** — build clean, all acceptance criteria checked, tests green.
5. **Ship** — set Status = `Shipped` and record the version. Reference the spec ID in the release/changelog.

## Conventions

- Keep specs concise; link to code with relative paths, don't paste large code blocks.
- One spec = one coherent feature or change. Split large efforts into multiple specs.
- Acceptance Criteria must be **verifiable** (a reviewer can check each one).
- Follow the repo coding standards (see `.github/copilot-instructions.md`).
