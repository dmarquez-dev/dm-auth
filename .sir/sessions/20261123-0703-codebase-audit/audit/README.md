# Audit disposition index (T8)

Ref: 5b7d9a6

Consolidation of the seven layer audits into `.sir/backlog.md`, per `contracts/finding-format.md` section 6.

## Audit files

- `.sir/sessions/20261123-0703-codebase-audit/audit/backend-domain-application.md` — T1, prefix `DA`, 26 findings
- `.sir/sessions/20261123-0703-codebase-audit/audit/backend-infrastructure-web.md` — T2, prefix `IW`, 21 findings
- `.sir/sessions/20261123-0703-codebase-audit/audit/database.md` — T3, prefix `DB`, 10 findings
- `.sir/sessions/20261123-0703-codebase-audit/audit/frontend.md` — T4, prefix `FE`, 24 findings
- `.sir/sessions/20261123-0703-codebase-audit/audit/tests-dotnet.md` — T5, prefix `NT`, 20 findings
- `.sir/sessions/20261123-0703-codebase-audit/audit/tests-client.md` — T6, prefix `CT`, 20 findings
- `.sir/sessions/20261123-0703-codebase-audit/audit/repo.md` — T7, prefix `RP`, 30 findings

## Dispositions

- DA-001 — row
- DA-002 — row
- DA-003 — row
- DA-004 — row
- DA-005 — row
- DA-006 — row
- DA-007 — row
- DA-008 — row
- DA-009 — row
- DA-010 — row
- DA-011 — row
- DA-012 — row
- DA-013 — existing: Bring `src/DMAuth.Application/Features/Auth/GetUserInfo/` in line with
- DA-014 — row
- DA-015 — row
- DA-016 — row
- DA-017 — row
- DA-018 — row
- DA-019 — row
- DA-020 — row
- DA-021 — row
- DA-022 — row
- DA-023 — row
- DA-024 — row
- DA-025 — row
- DA-026 — row
- IW-001 — row
- IW-002 — row
- IW-003 — row
- IW-004 — row
- IW-005 — row
- IW-006 — row
- IW-007 — row
- IW-008 — row
- IW-009 — row
- IW-010 — row
- IW-011 — row
- IW-012 — row
- IW-013 — row
- IW-014 — row
- IW-015 — row
- IW-016 — row
- IW-017 — row
- IW-018 — row
- IW-019 — row
- IW-020 — row
- IW-021 — row
- DB-001 — row
- DB-002 — row
- DB-003 — row
- DB-004 — row
- DB-005 — row
- DB-006 — row
- DB-007 — row
- DB-008 — row
- DB-009 — row
- DB-010 — row
- FE-001 — row
- FE-002 — row
- FE-003 — row
- FE-004 — row
- FE-005 — row
- FE-006 — row
- FE-007 — row
- FE-008 — row
- FE-009 — row
- FE-010 — row
- FE-011 — row
- FE-012 — row
- FE-013 — row
- FE-014 — row
- FE-015 — row
- FE-016 — row
- FE-017 — row
- FE-018 — row
- FE-019 — row
- FE-020 — row
- FE-021 — existing: Restate the one-time-initialization rule in
- FE-022 — row
- FE-023 — row
- FE-024 — row
- NT-001 — row
- NT-002 — row
- NT-003 — row
- NT-004 — row
- NT-005 — row
- NT-006 — row
- NT-007 — row
- NT-008 — row
- NT-009 — row
- NT-010 — row
- NT-011 — row
- NT-012 — row
- NT-013 — row
- NT-014 — row
- NT-015 — row
- NT-016 — row
- NT-017 — row
- NT-018 — row
- NT-019 — row
- NT-020 — row
- CT-001 — row
- CT-002 — row
- CT-003 — row
- CT-004 — row
- CT-005 — row
- CT-006 — row
- CT-007 — row
- CT-008 — row
- CT-009 — row
- CT-010 — row
- CT-011 — row
- CT-012 — row
- CT-013 — row
- CT-014 — row
- CT-015 — row
- CT-016 — row
- CT-017 — row
- CT-018 — row
- CT-019 — row
- CT-020 — row
- RP-001 — row
- RP-002 — row
- RP-003 — row
- RP-004 — row
- RP-005 — row
- RP-006 — row
- RP-007 — row
- RP-008 — row
- RP-009 — row
- RP-010 — row
- RP-011 — row
- RP-012 — row
- RP-013 — row
- RP-014 — row
- RP-015 — row
- RP-016 — row
- RP-017 — row
- RP-018 — row
- RP-019 — row
- RP-020 — row
- RP-021 — row
- RP-022 — row
- RP-023 — row
- RP-024 — row
- RP-025 — row
- RP-026 — row
- RP-027 — row
- RP-028 — row
- RP-029 — row
- RP-030 — row

## Merges

Rows are appended in severity order (correctness/security risk, convention drift, hygiene), then by first audit ID, where IDs compare as plain strings (so `CT` < `DA` < `DB` < `FE` < `IW` < `NT` < `RP`). A merged row's `Where` lists the union of its records' locations in audit-list order, duplicates dropped; its severity is the highest of its records'.

Related but not merged, because each asks for a different change: FE-022 (shared `Spinner` with `role="status"`) and CT-001 (tests query the loading state by role; CT-001's row defers the spinner's role to FE-022's); FE-019 (label the inputs) and CT-013 (e2e locates the display-name input by that label; CT-013's row defers the label to FE-019's); RP-002 (SHA pinning) and RP-005 (upgrade majors); RP-003 (test steps in the deploy workflows), RP-004 (a pull-request CI workflow) and NT-001 (make `dotnet test` discover the tests); RP-027 (correct the SameSite docs) and the CSRF row; DB-005 (`AsNoTracking` on `FindByClientIdAsync`) and the client-list paging row; RP-007 (`global.json` pinning the SDK) and NT-001 (whose MTP option also uses `global.json`).

- FE-002, IW-002 — both ask for the same CSRF control on the consent POST; FE-002 is the SPA half (token in the form) and IW-002 the server half (validation), and neither is done without the other.
- CT-009, FE-015 — both are the same rule against the same client tree (FE-015 the source files, CT-009 the test files) and they pull opposite ways (FE-015 moves the code, CT-009 the rule), so they are one change: restating the rule settles both at once and is the recommended option, because `.sir/lessons.md:3` holds that where a `.sir/disciplines` rule and the code disagree the code is the evidence and the rule is what gets restated; the alternative, reordering every file under a lint rule, would also settle both.
- DA-012, DB-004 — both add paging parameters to the same `IClientRepository.FindByOwnerIdAsync` call chain; DA-012 is the query/interface half and DB-004 the EF query half, and paging cannot land in one layer without the other.
- IW-009, RP-022 — both restate the same rule at `.sir/disciplines/dotnet.md:12` to `Infrastructure → Application, Domain`; RP-022 adds the README copies of that rule, which the same edit corrects.
