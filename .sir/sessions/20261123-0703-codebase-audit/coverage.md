# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. Seven layer audit files list every file in scope | T1, T2, T3, T4, T5, T6, T7 | covered | met — T1 9/9, T2 10/10, T3 9/9, T4 9/9, T5 10/10, T6 9/9, T7 10/10 met |
| 2. Each layer file covers every `## ` heading of its wired fragments | T1, T2, T3, T4, T5, T6 | covered | met — T1 9/9, T2 10/10, T3 9/9, T4 9/9, T5 10/10, T6 9/9 met |
| 3. Every finding record is valid per contract §3 | T1, T2, T3, T4, T5, T6, T7 | covered | met — T1 9/9, T2 10/10, T3 9/9, T4 9/9, T5 10/10, T6 9/9, T7 10/10 met; T8 criterion 12 re-checked all 151 records at the blob, 0 withdrawn |
| 4. NU1605 restore failure recorded at `DMAuth.Infrastructure.csproj:11` | T2 | covered | met — T2 10/10 met (IW-001) |
| 5. Committed coverage output recorded with a reproducing Census | T7 | covered | met — T7 10/10 met (RP-001, Census 65) |
| 6. Backlog pre-session bytes preserved; one well-formed, singly tagged row per deduplicated finding | T8 | covered | met — T8 14/14 met, re-met after revision round 1 (working-tree reading, 1066 bytes); at Gate 3 the blob reading also holds: `git show c20cd6d:.sir/backlog.md \| head -c 1062 \| sha256sum` equals `git show 5b7d9a6:.sir/backlog.md \| sha256sum` (`1ba14238…`) |
| 7. Every finding ID has exactly one disposition; no duplicate rows | T8 | covered | met — T8 14/14 met, re-met after revision round 1 |
| 8. Only `.sir/` paths change across the session range | orchestrator | uncovered — D4: orchestrator, from `git diff --name-only $(git merge-base origin/main chore/codebase-audit) chore/codebase-audit` run before closeout, output recorded in this cell | met — orchestrator row, run at Gate 3 at tip `c20cd6d` (merge-base `5b7d9a6`): 9 paths, `.sir/backlog.md` and 8 under `.sir/sessions/20261123-0703-codebase-audit/audit/`; none outside `.sir/` |
| 9. Nothing published outside the repository (E1) | orchestrator | uncovered — D4: orchestrator, stated in the Gate 3 report (no Jira or other external tool call made), and `reviews/` holds no external link | met — orchestrator row: no Jira or other external tool call made this session; `grep -ohE 'https?://[A-Za-z0-9][^ ]*' reviews/*.md \| wc -l` prints 0 (control: the same pattern finds 1 in `see https://example.com/x`) |
| 10. Lands through a merged PR into `main` | orchestrator | uncovered — D4: orchestrator, from the PR URL and the merge commit `closeout.mjs wait` reports, recorded in the report | applied after approval — the PR and its merge commit exist only after the records commit; recorded on the PR and in the exit summary |
| 11. Silent `dotnet test` recorded at `DMAuth.Tests.Unit.csproj` with cause (D2) | T5 | covered | met — T5 10/10 met (NT-001) |

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| §1 layer table and scope commands | T1–T7 | T8 | covered | applied — T1–T7 scoped their audit files by it; T8 read all seven |
| §2 finding ID scheme | T1–T7 | T8 | covered | applied — per-prefix IDs DA 26, IW 21, DB 10, FE 24, NT 20, CT 20, RP 30 (T8 criterion 6) |
| §3 finding record | T1–T7 | T8 | covered | applied — 151 records checked by T8 criterion 12 |
| §4 layer audit file | T1–T7 | T8 | covered | applied — seven files, each reviewer criterion 1 |
| §5 backlog row | T8 | none in-session (T8 defines it) | uncovered — D3: kept in contract as an own-task format declaration, no in-session consumer; no end-state search at Gate 3 | out of scope |
| §6 disposition index | T8 | none in-session (T8 defines it) | uncovered — D3: kept in contract as an own-task format declaration, no in-session consumer; no end-state search at Gate 3 | out of scope |
