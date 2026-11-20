# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 config equals template except `disciplines`/`$comment` | T4 | covered | met — T4 14/14 met |
| D2 every listed fragment exists, none orphaned, value matches §2 | T4 | covered | met — T4 14/14 met |
| D3 fragments name no deleted doc | T1, T2, T3, T4 | covered | met — T1 13/13, T2 11/11, T3 9/9, T4 14/14 met |
| D4 composed bound ≤ 500 per domain | T4 (T1–T3 budgets feed it) | covered | met — T4 14/14 met; T1 13/13, T2 11/11, T3 9/9 met |
| D5 lossless port ledger | T1, T2, T3; orchestrator persists each ledger to `port-ledger/T<n>.md` before review | covered, partly orchestrator | met — T1 13/13, T2 11/11, T3 9/9 met; orchestrator: `port-ledger/T1.md`, `T2.md`, `T3.md` present |
| D6 M1–M5 stated | T1 | covered | met — T1 13/13 met |
| D7 eight docs deleted, ADR doc unchanged | T4; at the tip, orchestrator: `git diff --quiet 2e0f9e2 <tip> -- docs/architecture-decisions.md` exits 0, recorded in the report | covered, partly orchestrator | met — T4 14/14 met; orchestrator (this row's own evidence): `git diff --quiet 2e0f9e2 0d926bf -- docs/architecture-decisions.md` exit=0 (also exit=0 at dcc3054) |
| D8 no tracked file outside `.sir/sessions/` names a deleted doc | T4; after merge, orchestrator: `git grep -lE '<8 stems>' HEAD -- . ':!.sir/sessions'` prints nothing, recorded in the report | covered, partly orchestrator | met — T4 14/14 met; orchestrator (this row's own evidence): `git grep -lE '<8 stems>' 0d926bf -- . ':!.sir/sessions'` printed nothing, exit=1 (also at dcc3054); control `git grep -c 'architecture-decisions' dcc3054 -- README.md` printed 3 |
| D9 `.claude-worktrees/` ignored | T4 (entry); orchestrator runs `git check-ignore -q .claude-worktrees/probe` in the worktree after T4, recorded in `reviews/T4.md` | covered, partly orchestrator | met — T4 14/14 met; `reviews/T4.md` criterion 14: exit 0 |
| D10 exactly 19 paths changed, none in code/build paths | orchestrator: `git diff --name-only 2e0f9e2 <tip>` lists exactly the 19 paths, recorded in the report | orchestrator (Gate 2 D3 accepted) | met — orchestrator (this row's own evidence): `git diff --name-status 2e0f9e2 dcc3054` lists 19 paths (19 again at 0d926bf by `--name-only \| wc -l`): 7 `.sir/disciplines/*.md` A, `.sir/config.json` A, `CLAUDE.md` A, `.gitignore` M, `README.md` M, 8 `docs/*.md` D; none under `src/`, `tests/`, `.github/`, `eng/` |
| D11 root `CLAUDE.md` states the branching policy | T4 | covered (added at Gate 2) | met — T4 14/14 met |

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| §1 fragment set and paths | T1, T2, T3 | T4 | covered | applied — the seven fragments by T1–T3; T4 criterion 3 printed `MISSING 0 ORPHAN 0 NONMD 0` |
| §2 `disciplines` value | T4 | orchestrator: composes backend, frontend, database and test from the tip `.sir/config.json` per sir-disciplines and counts each file's lines, recorded in `coverage.md` | uncovered → orchestrator (Gate 2 D3 accepted) | applied — by T4's `.sir/config.json`; orchestrator composition from the worktree at 0d926bf (base + fragments, provenance headers, frontmatter dropped): backend 297, frontend 221, database 153, test 373 lines (at dcc3054: 298, 220, 154, 375), each ≤ 500; control: 5 source headers in `test`, 4 in `backend` |
| §2 budgets and composed bound | T1, T2, T3 | T1, T2, T3, T4 | covered | applied — T1–T3 criterion 1 met; T4 criterion 4 `backend 298 OK`, `frontend 221 OK`, `database 155 OK`, `test 374 OK` |
| §4 unit assignment | T1, T2, T3 | T1, T2, T3 | covered | applied — T1 criteria 6–8, T2 criteria 6–11, T3 criteria 7–9 met |
| §5 M1–M5 placement | T1 | T1 | covered | applied — T1 criteria 9–13 met; A1 precedence line applied by T2 (criteria 8–10, `reviews/T2.md` round 3) |
| §6 port ledger | T1, T2, T3 | orchestrator persists; T1–T3 reviewers read | covered, orchestrator | applied — `port-ledger/T1.md`, `T2.md`, `T3.md` persisted and read by each reviewer |
