sir-sdlc 1.0.55 — Gate 3 report — 20261124-0129-external-sso-readiness

## 1. Decisions needed

**D1:** E5 left a deferred action: the 4 lint errors that were already in `tests/e2e` (the reason T5's lint criterion was scoped by A3) were to go to the backlog at Gate 3. They were re-measured at `84f3cf9`: `npm run lint` reports 12 problems, 4 errors and 8 warnings. The errors are `auth.spec.ts:12` (unused `request`), `fixtures/auth.ts:46` (rules-of-hooks on Playwright's `use`), and `profile.spec.ts:25` and `:46` (unused `freshUser`). Carry it?

- **Carry as E5 resolved** ← *recommended*: add a backlog row to fix the 4 errors in the 3 e2e files, so `npm --prefix src/DMAuth.Client run lint` reports 0 errors. This is E5's chosen option and keeps the session inside its signed files.
- **Fix it now (E5's option 2)**: widen T5 to the 3 e2e files and fix all 4 errors in a revision round. That means touching e2e fixtures outside the SSO scope, and one of the errors is a false positive on Playwright's `use`.
- **Drop**: no row. Repo-wide lint keeps exiting 1, and nothing schedules the fix.

## 2. Acceptance criteria

- **T1** · backend · 5 criteria · 5 met
- **T2** · backend · 13 criteria · 13 met
- **T3** · backend · 22 criteria · 22 met
- **T4** · backend · 8 criteria · 8 met
- **T5** · frontend · 14 criteria · 14 met

Criteria resting on a demonstration:

- **T1 C1** · met · red step: pre-edit build exit 1, `error NU1605 ... Detected package downgrade: Microsoft.Extensions.DependencyInjection.Abstractions from 10.0.15 to 10.0.7`.
- **T1 C2, C3, C4, C5** · met · reviewer-reproduced: `dotnet restore` exited 0. `dotnet build` exited 0 with `Build succeeded. 0 Warning(s) 0 Error(s)`, and `grep -c NU1605` printed 0. The floating-version grep prints 0 on Infrastructure and 2 on Web. The three-package grep prints 3.
- **T2 C1** · met · red step: the pre-edit unit run exited 0 with restore lines only, and `grep -c 'Total:'` returned 0.
- **T2 C4, C7, C9** · met · reviewer-reproduced: unit `Failed: 0, Passed: 234, Total: 234`. Integration `Failed: 0, Passed: 109, Total: 109` = 107 + 2 added. The appsettings node command exits 0.
- **T2 C11** · met · amended (A1) · injected defect / reviewer-reproduced: Program.cs:23 was changed from `true` to `false`. The filter run gave `Failed: 1, Passed: 1, Total: 2`, and the failing test was `Startup_WithNoAppSettingsFiles_ThrowsInvalidOperationException`. Restored, `cmp` IDENTICAL.
- **T3 C1, C2, C3, C4, C7, C8** · met · red step: red-unit.txt:109 shows `Failed: 7, Total: 37` with 0 `error CS` lines, all `Expected result.IsSuccess... to be False`. The green re-run gives `Failed: 0, Total: 37`.
- **T3 C4** · met · injected defect: with the `IsNullOrWhiteSpace(ClientSecretHash)` clause at ExchangeTokenHandler.cs:184 removed, the run produced `DomainException: Hashed password cannot be empty`.
- **T3 C13, C14** · met · injected defect: with both handler checks disabled (`if (false && ...)`), the omitted secret got 200 (red-int.txt:30) and the wrong rotate secret got 200 (red-int.txt:43).
- **T3 C16** · met · red step: the pre-fix Location was `...callback?tenant=a?code=...` (red-int.txt:15).
- **T3 C11, C15, C18, C19** · met · reviewer-reproduced: the ClientSecretHash grep prints 4 lines, and the secret-logging grep prints nothing. Unit `Failed: 0, Total: 245`. Integration `Failed: 0, Total: 115`.
- **T3 C21** · met · amended (A4): now reads "asserts only with FluentAssertions, verifying substitute calls with NSubstitute Received/DidNotReceive". No xUnit `Assert.` was added.
- **T4 C1, C3, C4** · met · red step: `Expected result.IsSuccess to be False, but found True.` before the fix. The consent tests were red on Add and on Update.
- **T4 C2** · met · red step / reviewer-reproduced: `Contains` was replaced with a `StartsWith` prefix match, and only this test went red (`Failed: 1, Passed: 12, Total: 13`). Restored, `cmp` identical.
- **T4 C5, C7, C8** · met · reviewer-reproduced: 8 `[Fact]` at ba33897, and the filter gives `Failed: 0, Passed: 13, Total: 13`. The build exited 0 with `0 Error(s)` and NU1605 count 0. Unit `Failed: 0, Passed: 250, Total: 250`. Integration `Failed: 0, Passed: 115, Total: 115`.
- **T5 C1** · met · amended (A2): the install is `npm ci --legacy-peer-deps --prefix src/DMAuth.Client`, and package.json and package-lock.json are unchanged.
- **T5 C2, C7, C8, C9, C10** · met · red step: `action="/connect/consent"` against the expected `https://api.example.test/connect/consent`. `Number of calls: 0`. "Unable to find ... Dashboard page" before the fix.
- **T5 C4, C5, C11, C12, C14** · met · reviewer-reproduced: `action={apiUrl('/connect/consent')}`, and the env read is only in `src/api/client.ts`. The selector grep exits 1. `Test Files 16 passed (16)`, `Tests 141 passed (141)`. The build exited 0.
- **T5 C13** · met · amended (A3) · reviewer-reproduced: the six owned files give 0 errors and 0 warnings. `npm run lint` gives `12 problems (4 errors, 8 warnings)`, all 4 errors in tests/e2e.

Every row is in `reviews/T1.md` through `reviews/T5.md`.

**Coverage**, as mapped at Gate 2, amended where an amendment changed a contract, and checked at Gate 3:

- **Items 1–18** · met: each joined to its task's criteria, all met.
- **Item 19** · met as amended by A3: repo-wide `npm run lint` still exits 1 on the 4 e2e errors in D1.
- **Item 20** · uncovered at Gate 2 · out of scope: the deployed consent approval goes to the backlog as a human check (Gate 2 D3).
- **Item 21** · uncovered at Gate 2 · out of scope: the live signed-out resume is deferred behind a new backlog row for `AuthController`'s API-relative redirects (Gate 2 D4).
- **Contract operations**: none. The spec froze no contracts.

**Limits:** none — every verification command ran in the worktree.

## 3. What ran

- **T1** · sir-backend-engineer, then sir-reviewer · backend · opus / opus · 1 review · 0 rounds · 0 redispatches · 73ae2d8
- **T2** · sir-backend-engineer, then sir-reviewer · backend · opus / opus · 2 reviews (the second a re-review under amendment A1, not charged) · 0 rounds · 0 redispatches · eaaa50d
- **T3** · sir-backend-engineer, then sir-reviewer · backend · opus / opus · 1 review · 0 rounds · 0 redispatches · 3692577
- **T4** · sir-backend-engineer, then sir-reviewer · backend · opus / opus · 1 review · 0 rounds · 0 redispatches · 84f3cf9
- **T5** · sir-frontend-engineer, then sir-reviewer · frontend · opus / opus · 1 review · 0 rounds · 0 redispatches · 7de4346

Also dispatched: sir-architect (decomposition, opus), and sir-git (haiku) for init-session and the five task commits. T5 ran concurrently with T3.

**Change surface:** `ba33897..84f3cf9` (merge-base with `main` to the tip of `fix/external-sso-readiness`). It is 5 commits, 26 files changed, 998 insertions and 76 deletions. `git diff ba33897..84f3cf9` shows it in full. Under PR mode, GitHub performs the merge after this gate, so there is no local merged tree to compare.

**Amendments (4):**

- **A1** · manifest.json · E3: T2 criterion 11's demonstrator changed from the criterion-10 ProgramTests test to `Startup_WithNoAppSettingsFiles_ThrowsInvalidOperationException` (empty content root).
- **A2** · manifest.json · E4: T5 criterion 1 changed from `npm ci --prefix src/DMAuth.Client` to `npm ci --legacy-peer-deps --prefix src/DMAuth.Client`.
- **A3** · manifest.json · E5: T5's lint criterion changed from `npm run lint` exits 0 to: eslint over the six owned files exits 0, and repo-wide lint reports exactly the 4 pre-existing e2e errors.
- **A4** · manifest.json · E6: T3 criterion 21 changed from "asserts only with FluentAssertions" to "..., verifying substitute calls with NSubstitute Received/DidNotReceive".

## 4. Advisory findings

Correctness risks: 3

- `src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:11` (removed line): the direct `Microsoft.Extensions.DependencyInjection.Abstractions` reference is gone, so Infrastructure now gets it only transitively, at 10.0.15 through DataProtection.EntityFrameworkCore (T1 A1).
- `src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj`: SqlServer and Tools were pinned at 10.0.15 without checking what `10.*` had resolved them to before the edit, so their resolved versions may have changed (T1 A2).
- `src/DMAuth.Client/src/pages/LoginPage.tsx`: two returnUrl classification choices are covered by no criterion. With the env unset, an absolute same-origin `/connect/authorize` URL is accepted. `/\host` falls back to `/dashboard` (T5 A4).

Session conduct: 3

- T1, `log.md`: the `DISPATCH commit-T1` line was written just after its sir-git dispatch went out, not before it. It still precedes `COMMIT T1 73ae2d8`.
- T4, reviewer: its injection rebuilt `DMAuth.Tests.Unit/bin` from the injected source. The source was restored byte-identically.
- T5, implementer: it briefly created and deleted `src/DMAuth.Client/src/api/client.ts.new`, which is outside its owns. Nothing remains.

Test and CI concerns: 5, in `reviews/T2.md`, `reviews/T3.md` and `reviews/T4.md`.

Cosmetics: 3, in `reviews/T3.md` and `reviews/T5.md`.

## 5. Metrics

- **userInteractions** · 6: the session's interactions, this gate and its closeout question included.
- **escalationsRaised** · 6: E1 (Gate 1), E2 (Gate 2), E3–E6 (implement).
- **reviewRounds** · 0: the sum of the per-task rounds in section 3. T2's second review was amendment-caused and is excluded.
