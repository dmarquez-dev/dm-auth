# SIR project bootstrap for DM Auth

Give DM Auth a committed SIR project setup — `.sir/config.json`, seven self-contained discipline
fragments under `.sir/disciplines/`, and an ignored worktree root — with the eight `docs/`
convention files ported into the fragments and then deleted, so each rule has one home.

## Context

- Base `main` at `2e0f9e2`. No `.sir/config.json` or `.sir/disciplines/` exists; this is the
  project's first SIR session, so every domain currently composes the bundled base only.
- `docs/` holds 9 files. Eight are convention docs (3898 lines by `wc -l`, of which 2340 are
  inside code fences by a fence-aware count); `architecture-decisions.md` is the ADR record and
  stays.
- References to the eight docs outside `docs/`: `README.md:148–155` (directory tree), `:438`
  (Vitest link) and `:497` (Playwright link) — `grep -n` at `2e0f9e2`.
- Five conventions exist only in the user's private memory (M1–M5 in
  `contracts/fragments.md` §5) and are folded into the fragments.
- `.gitignore` does not ignore `.claude-worktrees/` (`git check-ignore` exits 1 at base).

## Definition of done

- [ ] **D1** `.sir/config.json` parses as JSON and equals `config.default.json` in every key except
  `disciplines` and `$comment`; its top-level `$comment` no longer says a session never creates it.
- [ ] **D2** Every fragment name in `disciplines` resolves to `.sir/disciplines/<name>.md`, every
  `.sir/disciplines/*.md` is listed, and the value is exactly `contracts/fragments.md` §2.
- [ ] **D3** No fragment names or links any of the eight deleted docs.
- [ ] **D4** For each of backend, frontend, database and test, the bound in `contracts/fragments.md`
  §2 (base `wc -l` + Σ(fragment `wc -l` + 1)) is ≤ 500.
- [ ] **D5** Lossless port: every unit (contract §3) of each of the eight docs at `2e0f9e2` has exactly
  one ledger row — `carried`, `dropped` with a true reason, or `container` — and every `carried`
  row's rules are stated in the named fragment.
- [ ] **D6** M1, M2 and M4 are stated in `csharp-style.md`; M3 and M5 in `dotnet.md`.
- [ ] **D7** The eight convention docs are deleted; `docs/architecture-decisions.md` is unchanged from
  `2e0f9e2`.
- [ ] **D8** No tracked file outside `.sir/sessions/` names any of the eight docs at the tip; the README
  tree lists `.sir/config.json` and `.sir/disciplines/`, and `:438`/`:497` link to
  `.sir/disciplines/client-tests.md`.
- [ ] **D9** `.gitignore` gains a `.claude-worktrees/` entry and `git check-ignore` reports it ignored.
- [ ] **D10** The session changes exactly 19 paths against `2e0f9e2`: 7 fragments, `.sir/config.json`,
  `.gitignore`, `README.md`, `CLAUDE.md`, and the 8 deletions — nothing under `src/`, `tests/`,
  `.github/`, `eng/`, nor `Directory.Build.props` or `.editorconfig`.
- [ ] **D11** (added at Gate 2) A root `CLAUDE.md` states the branching policy: every SIR session starts
  from an up-to-date `main`, works on a fresh feature branch created from it, commits and pushes on that
  branch, opens a pull request into `main`, and holds until it is merged before the session ends, so the
  next session starts from the latest changes.

## Scope

Seven fragments (`csharp-style`, `dotnet`, `efcore`, `dotnet-tests`, `client-tests`,
`typescript-style`, `react`) written as imperative rule lists, wired into four domains by
`.sir/config.json`; the `.gitignore` entry; README repointed; a new root `CLAUDE.md` carrying the
branching policy; the eight docs deleted. A port
ledger per porting task proves losslessness and is kept with the session records, not in the
repository tree.

### Not in scope

- Seeding `.sir/lessons.md` or `.sir/backlog.md` (D10's 19-path count enforces it).
- Any code, test, workflow or build-config change (D10).
- The pre-existing broken README link at `:214` to `docs/architecture-decisions/001-cqrs-lite.md`
  — kept exactly as is (T4's link check requires it to be the only broken link).
- The user's private memory entry naming `docs/react-guidelines.md`, and its stale note placing
  `_allowedScopes` in `Scope.cs` (it is at `src/DMAuth.Domain/Policies/ScopePolicy.cs:18`) — outside
  the repository; raised for the user at Gate 3.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Which docs are "convention files"? | The eight convention/format/guideline docs; `architecture-decisions.md` is a decision record and stays | 1 | `docs/architecture-decisions.md` headings are ADR-001…ADR-009 (`grep -n '^#'`); discussion.md Decisions |
| How are the fragments split? | Seven fragments; the two style fragments are listed under every domain whose files they bind, so each rule has one home | 4 | `contracts/fragments.md` §1–2; doc structure by heading |
| What is the port unit? | Each `^#{1,3} ` line outside a fence; 211 units (T1 74, T2 52, T3 85) | 4 (count measured) | Lister command in the criteria, run at base; split verified at the four boundaries (`:210`, `:510`, `:95`, `:219`) |
| Where a doc contradicts a memory convention, which wins? | The memory convention (M1–M5); the ledger row says so | 4 | `docs/dotnet-format.md:243` `RuleFor(x =>`, `docs/dotnet-guidelines.md:141` `u =>` against M2 |
| SWR or TanStack Query for client server-state? | TanStack Query; SWR guidance is not prescribed | 1 | `src/DMAuth.Client/package.json:19`; `docs/frontend-conventions.md:55`; `docs/react-guidelines.md:275` itself allows "SWR (or React Query)" |
| Do SSR/RSC/Next.js rules port? | No; react.md states the client is a CSR-only Vite SPA | 1 | `docs/react-guidelines.md:5`, `:949` |
| Do M3 and M5 bind tests? | No; they reach the backend domain only | 4 | M3/M5 concern runtime configuration |
| Is private-static `_camelCase` (M4) consistent with tooling? | Yes | 1 | `.editorconfig:150–159`, `private_fields` has no required modifiers |
| Where do the README links at `:438`/`:497` point? | `.sir/disciplines/client-tests.md`, two links | 4 | discussion.md allows repoint or remove |
| What happens to config.default.json's `$comment`? | Rewritten for this project; the copied text says a session never creates the file, which would be false here | 4 | `config.default.json` `$comment` |
| Do builds or injected defects constrain scheduling? | No: no criterion runs dotnet or npm, and no task injects a defect, so T1–T3 run in parallel | 1 | every criterion command, run at base; verification record |
| Fragment frontmatter? | Not required; composition drops it | 1 | sir-disciplines, Composition |
| Does a `CLAUDE.md` already exist to update? | No; T4 creates it at the root | 1 | Glob `**/CLAUDE.md` over the worktree at `2e0f9e2`: no files |
| What does "hold on a completed PR" mean? | The session waits until its pull request is merged into `main` before it ends | 4 | Gate 2 answer: "so main is up to date before the session ends" |
| Does the ledger's "no extra key" span the whole file? | No; each ledger criterion is scoped to its own doc's keys (Gate 2 D1) | — | Gate 2 answer |
| Does M2 bind TypeScript? | No, C# only (Gate 2 D2) | — | Gate 2 answer |

## Contracts

- [`contracts/fragments.md`](contracts/fragments.md) — fragment set and budgets, the `disciplines`
  value, port unit and assignment, M1–M5 placement, ledger shape.

## Open questions

None.
