# Codebase audit

Audit the whole DM Auth repository against common .NET, React/TypeScript, EF Core and testing
best practices and the project's own `.sir/disciplines/` conventions, and record every
hygiene/refactor item found as a backlog row. No fixes are applied in this session.

## Context

- The previous session (`20261120-0315-sir-bootstrap`) installed SIR: it wrote the seven
  discipline fragments under `.sir/disciplines/` (csharp-style, dotnet, efcore, react,
  typescript-style, dotnet-tests, client-tests), wired by `.sir/config.json`, and left two open
  rows in `.sir/backlog.md` (lines 3 and 4: the `GetUserInfo` layout/validator row and the
  `didInit` restatement in `react.md`).
- Measured at base `5b7d9a6` in this session: 278 tracked source files under `src/` and `tests/`
  (`.cs`, `.ts`, `.tsx`, `.csproj`, `.json`, excluding `src/DMAuth.Client/coverage`), and 65
  tracked files under `src/DMAuth.Client/coverage/` (generated coverage output committed to git).
- Repo-level surface: `Directory.Build.props`, `.editorconfig`, `.gitignore`,
  `.github/workflows/main_app-dmauth-api.yml`,
  `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml`, `eng/dev/`
  (docker-compose, seed-data.sql, setup.ps1), `README.md`, `docs/architecture-decisions.md`,
  `CLAUDE.md`.
- Lesson 1 in `.sir/lessons.md`: where a discipline rule and the code disagree, the code is the
  evidence; a finding must say which side should move.

## Decisions

- Track: Standard (Gate 1). The deliverable is many independent, individually checkable claims.
- Branch: `chore/codebase-audit` (`branchType` `chore`).
- E1 (Gate 1): findings land in `.sir/backlog.md` only. No Jira issues, nothing published outside
  the repository.
- Scope: every layer — backend (`DMAuth.Domain`, `DMAuth.Application`, `DMAuth.Infrastructure`,
  `DMAuth.Web`), frontend (`DMAuth.Client`), tests (`DMAuth.Tests.Unit`,
  `DMAuth.Tests.Integration`, Playwright e2e under `src/DMAuth.Client/tests/e2e`), and the
  repo-level surface listed above (build config, CI workflows, dev scripts, docs).
- Reference standards: the project's `.sir/disciplines/` fragments first, then common
  best practices for each stack (current .NET/ASP.NET Core, EF Core, React + TypeScript with
  Vite, xUnit/integration testing, Playwright, GitHub Actions). A best-practice finding cites
  its source.
- No product code, test, config or discipline file is modified. The only repository writes are
  this session's records and the appended `.sir/backlog.md` rows.
- Each finding becomes one backlog row in the file's existing shape (date, session id, text):
  what to change, where (`file:line` from `grep -n` at a named ref), why it matters, and how to
  verify it is done. Rows are deduplicated against each other and against the 2 existing rows.
- Each row carries a severity tag: correctness/security risk, convention drift, or hygiene, so
  short-term picks are easy.
- A full audit report (findings grouped by layer, with evidence) is kept in the session
  directory as the record the backlog rows are checked against.
- The 2 existing backlog rows (lines 3 and 4) stay open and untouched; this session does not
  implement them.
- Audit agents run read-only on opus; sir-git on haiku.
- Integration: pull request into `main`, per `CLAUDE.md`; the session is not finished until that
  PR merges.
