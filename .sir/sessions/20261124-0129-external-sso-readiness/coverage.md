# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. restore and build exit 0, no NU1605 | T1 (re-gated by T4) | covered | met: T1 5/5 met (criteria 2, 3); T4 criterion 7 met |
| 2. Infrastructure refs pinned exactly | T1 | covered | met: T1 5/5 met (criteria 4, 5) |
| 3. unit summary Total 234, 0 failed | T2 | covered | met: T2 13/13 met (criterion 4) |
| 4. integration summary 0 failed, Total 107 + T2's tests | T2 | covered | met: T2 13/13 met (criterion 7, Total 109 = 107 + 2) |
| 5. one runner (VSTest) for both | T2 | covered | met: T2 13/13 met (criteria 2, 3) |
| 6. integration tests need no Azure credentials; prod fail-fast kept | T2 | covered (Gate 2 D1: `KeyVault:Enabled` switch) | met: T2 13/13 met (criteria 7, 8, 10, 11 amended A1) |
| 7. wrong secret, exchange: invalid_client before code lookup | T3 | covered | met: T3 22/22 met |
| 8. wrong secret, rotation: invalid_client before token lookup | T3 | covered | met: T3 22/22 met |
| 9. missing secret: invalid_client on both grants | T3 | covered | met: T3 22/22 met |
| 10. public clients unchanged | T3 | covered | met: T3 22/22 met |
| 11. client_secret_post end to end | T3 | covered | met: T3 22/22 met |
| 12. ClientSecretHash read in Application | T3 | covered | met: T3 22/22 met (criterion 11) |
| 13. redirect URI query retained | T3 | covered | met: T3 22/22 met (criteria 16, 17) |
| 14. GrantConsent rejects unregistered / extended redirect URI | T4 | covered | met: T4 8/8 met (criteria 1, 2) |
| 15. GrantConsent rejects disallowed scope | T4 | covered | met: T4 8/8 met (criterion 3) |
| 16. consent action from API base URL | T5 | covered | met: T5 14/14 met |
| 17. login classifies returnUrl | T5 | covered | met: T5 14/14 met |
| 18. every fix has a failing-then-passing test | T1-T5 | covered | met: T1 5/5, T2 13/13, T3 22/22, T4 8/8, T5 14/14 met |
| 19. solution tests green; client test, lint, build green | T4, T5 | covered | met: T4 8/8, T5 14/14 met, lint as amended by A3; repo-wide `npm run lint` still exits 1 on 4 pre-existing errors in tests/e2e (re-measured at 84f3cf9 at Gate 3: 12 problems, 4 errors, 8 warnings) |
| 20. deployed consent approval returns the API's 302 | none | uncovered: accepted out of scope, carried to `.sir/backlog.md` at Gate 3 as a human check (Gate 2 D3): after deploy, signed in on the SPA, open `/consent?...` directly for a registered client, click Allow, land on the redirect URI with `code=` and `state=` | out of scope |
| 21. signed-out authorize resumes after sign-in (live) | none | uncovered: deferred behind a new `.sir/backlog.md` row, appended at Gate 3, for `AuthController`'s API-relative `/login` and `/consent` redirects; item 21 is that row's Verify (Gate 2 D4) | out of scope |

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
