# Codebase audit

Audit all of DM Auth against `.sir/disciplines/` and common stack best practice, and record each
deduplicated finding as a severity-tagged `.sir/backlog.md` row. No fixes are made in this session.

## Context

The previous session (`20261120-0315-sir-bootstrap`) installed SIR and wrote the seven discipline
fragments, but nobody has yet read the code against them. Base `5b7d9a6` has these problems:
- `dotnet build src/DM-Auth.sln` fails restore with NU1605 (`src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:11`).
- `dotnet test` on the unit project prints no test results.
- The CI workflow `.github/workflows/main_app-dmauth-api.yml` has no test step.
- 65 generated coverage files are tracked under `src/DMAuth.Client/coverage/`.

The backlog holds 2 open rows: the `GetUserInfo` layout/validator row and the `didInit` restatement
row in `react.md`. Both stay untouched. Lesson 1 applies: where a rule and the code disagree, the
code is the evidence, and the finding says which side moves.

## Definition of done

1. [ ] The seven layer audit files under `.sir/sessions/20261123-0703-codebase-audit/audit/` on the
   session branch list every file in scope. That is every path printed by the scope commands in
   `contracts/finding-format.md` §1, plus T7's 13 fixed entries.
2. [ ] Each of T1–T6's audit files gives a verdict on every `## ` heading of its wired
   `.sir/disciplines/` fragments.
3. [ ] Every finding record is valid per contract §3:
   - its severity is one of `correctness/security risk`, `convention drift` or `hygiene`
   - each `path:line` reproduces at `5b7d9a6`
   - each discipline heading exists, and each best-practice source carries an `https://` URL
   - each discipline-sourced record says which side moves
4. [ ] The NU1605 restore failure is a `correctness/security risk` record at
   `src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:11`.
5. [ ] The committed coverage output is a record with a `Census` that reproduces.
6. [ ] `.sir/backlog.md` keeps its pre-session 1066 bytes byte-identical. After them it gains one
   row per deduplicated finding, in contract §5's shape, each row with exactly one severity tag.
7. [ ] `audit/README.md` gives every finding ID exactly one disposition: `row`, `existing` or
   `withdrawn`. No appended row duplicates another or either pre-session row.
8. [ ] Across the session's whole commit range, only `.sir/` paths change:
   `git diff --name-only $(git merge-base origin/main chore/codebase-audit) chore/codebase-audit`
   lists nothing outside `.sir/`.
9. [ ] Nothing is published outside the repository: no Jira issues (E1).
10. [ ] The session lands through a pull request from `chore/codebase-audit` into `main` that merges.
11. [ ] `dotnet test` on the unit project running no tests is a `correctness/security risk` record at
   `tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj`. It gives the cause and says whether the
   integration project shares it (Gate 2 D2).

## Scope

All of `src/` and `tests/` (seven layers), and the repo-level surface: `Directory.Build.props`,
`.editorconfig`, both `.gitignore` files, `src/DMAuth.Client/coverage/`, both workflows, `eng/dev/`
(3 files), `README.md`, `docs/architecture-decisions.md` and `CLAUDE.md`.

### Not in scope

- Fixing any finding. Verify fields name a build or test command for the later session that fixes it.
- Anything under `.sir/` as an audit subject. A finding may still say a discipline should move.
- Running builds, tests or linters. None runs in the worktree (see the verification record in
  `state.json`), and nothing is installed.
- The two pre-session backlog rows.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Where do the layer audit files live? | In the worktree, at `.sir/sessions/<id>/audit/`, each owned by its task. `closeout.mjs stage` copies only the files the main checkout's session directory holds, so no file lands on top of another | repo | `closeout.mjs:356-368` (plugin 1.0.54) |
| Which date do the rows carry? | `2026-11-23`, the session's UTC start date | repo | `session.json` `started` |
| Which ref do line numbers use? | `5b7d9a6`. No task edits a scoped file, so the worktree content equals that ref | repo | `git rev-parse origin/main`; every task's diff-scope criterion |
| Can T1–T7 run concurrently? | Yes. No task builds, restores or injects a defect, and their owned paths are disjoint | repo | manifest `owns` and criteria |
| Who runs T7 (repo surface) and T8 (consolidation)? | `sir-architect` in implementer mode. No single implementer domain covers CI, build config and docs | judgment | `sir-phase-implement` permits `sir-architect` as a task agent |
| Must "Rules checked" cover the bundled base sections? | No, only the project fragments' headings. Base sections may still be cited | judgment | discussion: "`.sir/disciplines/` fragments first" |
| Can auditors run toolchain commands for evidence? | No. All 5 probed commands are non-runnable in the worktree, so evidence is read from files | repo | `state.json` `verification` |
| How do T8's backlog rows survive closeout? | Gate 3 syncs first (Gate 2 D1). Before any Gate 3 append, review sets the main checkout's `.sir/backlog.md` to `git show chore/codebase-audit:.sir/backlog.md`, then appends. Without this, `stage` copies a main-checkout copy that differs from HEAD over the branch copy, which would drop T8's rows | repo | `closeout.mjs:370-374` (plugin 1.0.54); user decision D1 |
| Will the backlog growth matter? | The append will very likely exceed `upkeep.backlogGrowthNudge` (20), so the next `/sir:sir-tend` gets nudged. This is expected | repo | `.sir/config.json` |

## Contracts

- [`contracts/finding-format.md`](contracts/finding-format.md) fixes the layer scopes and ID
  prefixes, the finding record, the layer audit file, the backlog row shape and the disposition
  index.

## Open questions

None. Gate 2's decisions are applied before the freeze.
