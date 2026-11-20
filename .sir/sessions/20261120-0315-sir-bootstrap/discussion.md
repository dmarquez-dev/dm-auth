# SIR project bootstrap for DM Auth

Give DM Auth a committed SIR project setup — `.sir/config.json`, self-contained per-domain
discipline fragments under `.sir/disciplines/`, and an ignored worktree root — so every later
SIR session follows this repository's conventions without being told them, and so the
conventions live in one place.

## Context

- At base `2e0f9e2` (main, clean) no `.sir/` directory exists; this is the project's first SIR session.
- `docs/` holds 9 files, 4135 lines total by `wc -l docs/*.md` at `2e0f9e2`:
  `architecture-decisions.md` (237), `backend-conventions.md` (251), `dotnet-format.md` (618),
  `dotnet-guidelines.md` (532), `frontend-conventions.md` (116), `playwright-guidelines.md` (362),
  `react-format.md` (276), `react-guidelines.md` (1403), `vitest-guidelines.md` (340).
- The plugin ships bundled base disciplines `backend.md`, `database.md`, `frontend.md`, `test.md`
  under `<plugin root>/disciplines/`, and a config template at `<plugin root>/config.default.json`
  that says a session never creates `.sir/config.json` — here the user explicitly asked for the
  bootstrap, which is that request.
- `.gitignore` ignores `/.claude` at line 420 (`grep -n` at `2e0f9e2`) but not `.claude-worktrees/`,
  the default `git.worktreeRoot`.
- `README.md` links to convention docs this session deletes: `docs/vitest-guidelines.md` at line 438
  and `docs/playwright-guidelines.md` at line 497 (Grep over the repo at `2e0f9e2`). It also links
  `docs/architecture-decisions.md` (line 535) and `docs/architecture-decisions/001-cqrs-lite.md`
  (line 214), which are not convention docs.
- Project conventions recorded outside `docs/`, in the user's Claude memory: XML doc comment text
  indented with two tabs after `///` and wrapped rather than overlong; descriptive lambda parameter
  names, never single letters; connection string named `DmAuth` (Key Vault secret
  `ConnectionStrings--DmAuth`); private static fields `_camelCase`; sensitive settings `null` in
  every `appsettings*.json`, overridden via user secrets or Key Vault, non-secret settings keep safe
  defaults. The memory also names `docs/react-guidelines.md` as canonical for DMAuth.Client work.

## Decisions

- **Track: Standard** (Gate 1). One subsystem, many independently checkable rules.
- **E1 — fragments are self-contained** (Gate 1, user's answer outside the menu): each
  `.sir/disciplines/<name>.md` carries its domain's rules inline rather than pointing at `docs/`.
- **E1 — the `docs/` convention files are deleted once ported** (same answer): after their rules
  are carried into the fragments, the convention docs are removed so each rule has one home.
  Assumption for the spec to confirm from evidence: "convention files" means the eight
  convention/format/guideline docs; `architecture-decisions.md` (and the `docs/architecture-decisions/`
  path README line 214 links) is a decision record, not a discipline, and stays.
- Porting must be lossless for rules: the spec should give a way to check that every rule in a
  deleted doc is either in a fragment or deliberately dropped with a reason, since the deleted
  file is no longer there to compare against.
- References to deleted docs must be repointed or removed: at least `README.md` lines 438 and 497.
  The user's memory entry pointing at `docs/react-guidelines.md` will be stale after merge; updating
  it is outside the repository and is noted for the user at Gate 3.
- The memory-only conventions listed in Context are folded into the backend fragment (and test,
  where they apply).
- `.sir/config.json` is a copy of `config.default.json` with `disciplines` wired to the new
  fragments; `modelTiers`, `tierToModel`, `gates`, `upkeep` keep their defaults; `git.worktreeRoot`
  stays `.claude-worktrees/` and `git.branchPrefix` stays `sir/`.
- `.gitignore` gains `.claude-worktrees/`.
- No `.sir/lessons.md` or `.sir/backlog.md` is seeded.
- No runtime behaviour changes; no human checks expected.
