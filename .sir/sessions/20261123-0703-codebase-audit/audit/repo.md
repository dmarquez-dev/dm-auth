# Audit: repo (T7)
Ref: 5b7d9a6

## Scope

T7 fixed list, no command: `Directory.Build.props`, `.editorconfig`, `.gitignore`,
`src/DMAuth.Client/.gitignore`, `src/DMAuth.Client/coverage/` (one entry for the whole
directory), `.github/workflows/main_app-dmauth-api.yml`,
`.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml`,
`eng/dev/docker-compose.yml`, `eng/dev/seed-data.sql`, `eng/dev/setup.ps1`, `README.md`,
`docs/architecture-decisions.md`, `CLAUDE.md`.

Evidence was read at `5b7d9a6` in the session worktree, whose files have CRLF line endings; quoted
lines are given without the trailing CR. Every `path:line` and Census command below is relative to
the worktree root.

## Files

- `Directory.Build.props` — RP-008
- `.editorconfig` — RP-008, RP-009
- `.gitignore` — RP-001, RP-010, RP-011
- `src/DMAuth.Client/.gitignore` — RP-001
- `src/DMAuth.Client/coverage/` — RP-001
- `.github/workflows/main_app-dmauth-api.yml` — RP-002, RP-003, RP-004, RP-006, RP-007
- `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml` — RP-002, RP-003, RP-004, RP-005, RP-006
- `eng/dev/docker-compose.yml` — RP-012, RP-013
- `eng/dev/seed-data.sql` — RP-014, RP-015, RP-019
- `eng/dev/setup.ps1` — RP-014, RP-015
- `README.md` — RP-010, RP-011, RP-012, RP-016, RP-017, RP-018, RP-019, RP-020, RP-021, RP-022, RP-023, RP-024, RP-025, RP-026, RP-027, RP-030
- `docs/architecture-decisions.md` — RP-021, RP-023, RP-027, RP-028, RP-029
- `CLAUDE.md` — no finding

## Rules checked

- GitHub Actions workflow security and hygiene (actions pinned to full commit SHAs, `permissions:` least privilege, secrets handling, test gate before deploy, PR validation, concurrency, deployment environments) — RP-002, RP-003, RP-004, RP-005, RP-006, RP-007 — external Security hardening for GitHub Actions https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions; external Controlling permissions for GITHUB_TOKEN https://docs.github.com/en/actions/writing-workflows/choosing-what-your-workflow-does/controlling-permissions-for-github_token
- .NET build configuration (`Directory.Build.props`, `.editorconfig`, SDK pinning) — RP-007, RP-008, RP-009 — external Code analysis in .NET: code-style analysis https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview; external Customize your build by folder https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory
- Git ignore of generated output — RP-001, RP-010, RP-011 — external gitignore documentation https://git-scm.com/docs/gitignore
- Local dev environment scripts (`eng/dev/`: credentials, exposure, idempotence, guards) — RP-012, RP-013, RP-014, RP-015 — external OWASP Secrets Management Cheat Sheet https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html; external Compose file reference: ports https://docs.docker.com/reference/compose-file/services/#ports
- Documentation accuracy against the code (`README.md`, `docs/architecture-decisions.md`, `CLAUDE.md`) — RP-016 to RP-030; `CLAUDE.md` conforms (its `.sir/config.json` wiring claim holds) — external Write the Docs: Docs as Code https://www.writethedocs.org/guide/docs-as-code/

## Findings

### RP-001 Generated Vitest coverage report is committed under src/DMAuth.Client/coverage/
- Severity: hygiene
- Layer: repo
- Location: `src/DMAuth.Client/coverage/`; `src/DMAuth.Client/.gitignore`; `.gitignore`
- Rule: external gitignore documentation https://git-scm.com/docs/gitignore; external Vitest coverage guide https://vitest.dev/guide/coverage
- Moves: n/a
- What: Add the ignore rule `coverage/` to `src/DMAuth.Client/.gitignore` (neither ignore file matches the directory: the root patterns `coverage*.json`, `coverage*.xml`, `coverage*.info` match none of its files), then remove the generated output from the index with `git rm -r --cached src/DMAuth.Client/coverage` and commit.
- Why: `npm run test:coverage` (vitest reporter `html`, `lcov`) regenerates every one of these files, so the committed copy is stale on every source change and pollutes diffs and reviews.
- Verify: `find src/DMAuth.Client/coverage -type f | wc -l` prints 0 in a fresh checkout, and `git check-ignore src/DMAuth.Client/coverage/index.html` prints the path.
- Census: 65 via `find src/DMAuth.Client/coverage -type f | wc -l`
- Evidence:
  ````
  $ grep -n -i 'coverage' src/DMAuth.Client/.gitignore; echo "exit=$?"
  exit=1
  $ grep -n -i 'coverage' .gitignore
  138:# DotCover is a Code Coverage Tool
  141:# AxoCover is a Code Coverage Tool
  145:# Coverlet is a free, cross platform Code Coverage Tool
  146:coverage*.json
  147:coverage*.xml
  148:coverage*.info
  150:# Visual Studio code coverage results
  151:*.coverage
  152:*.coveragexml
  $ find src/DMAuth.Client/coverage -type f -name 'coverage*' | wc -l
  0
  $ find src/DMAuth.Client/coverage -type f | wc -l
  65
  ````

### RP-002 Workflow actions are pinned to mutable tags, not full commit SHAs
- Severity: correctness/security risk
- Layer: repo
- Location: `.github/workflows/main_app-dmauth-api.yml:19`; `.github/workflows/main_app-dmauth-api.yml:22`; `.github/workflows/main_app-dmauth-api.yml:33`; `.github/workflows/main_app-dmauth-api.yml:47`; `.github/workflows/main_app-dmauth-api.yml:52`; `.github/workflows/main_app-dmauth-api.yml:60`; `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml:17`; `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml:24`; `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml:33`
- Rule: external Security hardening for GitHub Actions: using third-party actions https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions#using-third-party-actions
- Moves: n/a
- What: Pin every `uses:` to a full-length commit SHA with the tag as a trailing comment (e.g. `actions/checkout@<40-hex-sha> # v4`), and enable Dependabot `github-actions` updates to keep the pins current.
- Why: a tag can be moved by whoever controls the action repository; these jobs hold `id-token: write` and Azure deploy credentials, so a hijacked tag runs attacker code with production deploy rights.
- Verify: `grep -n -E 'uses: [^@]+@' .github/workflows/*.yml | grep -v -E '@[0-9a-f]{40}'` prints nothing.
- Evidence:
  ````
  .github/workflows/main_app-dmauth-api.yml
  19:      - uses: actions/checkout@v4
  22:        uses: actions/setup-dotnet@v4
  33:        uses: actions/upload-artifact@v4
  47:        uses: actions/download-artifact@v4
  52:        uses: azure/login@v2
  60:        uses: azure/webapps-deploy@v3
  .github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml
  17:      - uses: actions/checkout@v3
  24:        uses: actions/github-script@v6
  33:        uses: Azure/static-web-apps-deploy@v1
  ````

### RP-003 Neither deploy workflow runs tests (or lint) before deploying to production
- Severity: correctness/security risk
- Layer: repo
- Location: `.github/workflows/main_app-dmauth-api.yml:27`; `.github/workflows/main_app-dmauth-api.yml:30`; `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml:31`
- Rule: external Building and testing .NET (GitHub Actions) https://docs.github.com/en/actions/use-cases-and-examples/building-and-testing/building-and-testing-net; external Building and testing Node.js https://docs.github.com/en/actions/use-cases-and-examples/building-and-testing/building-and-testing-nodejs
- Moves: n/a
- What: Add a `dotnet test src/DM-Auth.sln -c Release --no-build` step between build and publish in the API workflow, and add `npm ci`, `npm run lint` and `npm test` steps (with `actions/setup-node`) before `Build And Deploy` in the Static Web Apps workflow; fail the job on any failure.
- Why: every push to `main` builds and deploys straight to the `Production` slot with no test gate, so a regression the unit, integration or Vitest suites would catch ships to production.
- Verify: `grep -n -E 'dotnet test' .github/workflows/main_app-dmauth-api.yml` and `grep -n -E 'npm (run lint|test)' .github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml` each print a step that precedes the deploy step.
- Evidence:
  ````
  .github/workflows/main_app-dmauth-api.yml
  27:        run: dotnet build src/DM-Auth.sln --configuration Release
  30:        run: dotnet publish src/DMAuth.Web/DMAuth.Web.csproj -c Release -o ${{env.DOTNET_ROOT}}/myapp
  .github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml
  31:      - name: Build And Deploy
  $ grep -n -E 'dotnet test|npm (run )?(test|lint)' .github/workflows/main_app-dmauth-api.yml .github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml; echo "exit=$?"
  exit=1
  ````

### RP-004 No workflow runs on pull requests, so the PR-only branching policy merges unvalidated code
- Severity: hygiene
- Layer: repo
- Location: `.github/workflows/main_app-dmauth-api.yml:7`; `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml:4`
- Rule: external Events that trigger workflows: pull_request https://docs.github.com/en/actions/writing-workflows/choosing-when-your-workflow-runs/events-that-trigger-workflows#pull_request; external About protected branches: require status checks https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches#require-status-checks-before-merging
- Moves: n/a
- What: Add a CI workflow triggered on `pull_request` to `main` that builds and tests both the .NET solution and the client (no deploy, `permissions: contents: read`), and make it a required status check on `main`.
- Why: `CLAUDE.md` requires every change to reach `main` through a pull request, but both workflows trigger only on `push` to `main` and `workflow_dispatch`, so a PR is merged with no automated check and the first build runs after the merge, during deploy.
- Verify: `grep -n 'pull_request' .github/workflows/*.yml` prints a trigger, and the branch protection for `main` lists the CI job as required.
- Evidence:
  ````
  .github/workflows/main_app-dmauth-api.yml
  7:  push:
  .github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml
  4:  push:
  $ grep -n -E 'pull_request|concurrency|environment:' .github/workflows/main_app-dmauth-api.yml .github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml; echo "exit=$?"
  exit=1
  ````

### RP-005 Static Web Apps workflow uses superseded action majors built for the retired Node 16 runtime
- Severity: hygiene
- Layer: repo
- Location: `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml:17`; `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml:24`
- Rule: external GitHub Actions: transitioning from Node 16 to Node 20 https://github.blog/changelog/2023-09-22-github-actions-transitioning-from-node-16-to-node-20/
- Moves: n/a
- What: Upgrade `actions/checkout@v3` and `actions/github-script@v6` to their current majors (the API workflow already uses `actions/checkout@v4`), pinned per RP-002.
- Why: these majors target the Node 16 runtime GitHub has retired; runs are force-migrated with deprecation warnings and the two workflows drift onto different checkout versions.
- Verify: `grep -n -E 'checkout@v3|github-script@v6' .github/workflows/*.yml` prints nothing.
- Evidence:
  ````
  17:      - uses: actions/checkout@v3
  24:        uses: actions/github-script@v6
  ````

### RP-006 Production deploy jobs have no deployment environment and no concurrency guard
- Severity: hygiene
- Layer: repo
- Location: `.github/workflows/main_app-dmauth-api.yml:38`; `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml:10`
- Rule: external Managing environments for deployment https://docs.github.com/en/actions/managing-workflow-runs-and-deployments/managing-deployments/managing-environments-for-deployment; external Control the concurrency of workflows and jobs https://docs.github.com/en/actions/writing-workflows/choosing-what-your-workflow-does/control-the-concurrency-of-workflows-and-jobs
- Moves: n/a
- What: Give each deploy job `environment: production` (with protection rules, and scope the Azure federated credential to that environment) and add a `concurrency:` group per workflow with `cancel-in-progress: false`.
- Why: without an environment there is no deployment record, approval or environment-scoped secret, and without a concurrency group two quick pushes to `main` can deploy out of order, leaving the older build live.
- Verify: `grep -n -E '^\s*(environment|concurrency):' .github/workflows/*.yml` prints one of each per workflow.
- Evidence:
  ````
  .github/workflows/main_app-dmauth-api.yml
  38:  deploy:
  .github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml
  10:  build_and_deploy_job:
  $ grep -n -E 'pull_request|concurrency|environment:' .github/workflows/main_app-dmauth-api.yml .github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml; echo "exit=$?"
  exit=1
  ````

### RP-007 CI installs a floating .NET SDK and the repository has no global.json
- Severity: hygiene
- Layer: repo
- Location: `.github/workflows/main_app-dmauth-api.yml:24`
- Rule: external global.json overview https://learn.microsoft.com/en-us/dotnet/core/tools/global-json; external actions/setup-dotnet: global-json-file input https://github.com/actions/setup-dotnet#using-the-global-json-file-input
- Moves: n/a
- What: Add a root `global.json` pinning the SDK (with a `rollForward` policy) and switch the step to `global-json-file: global.json` instead of `dotnet-version: '10.x'`.
- Why: `10.x` resolves to whatever SDK is newest on each run, so CI and developer machines build with different SDKs and analyzer/NuGet behaviour can change between runs with no commit.
- Verify: `test -f global.json` succeeds and `grep -n 'global-json-file' .github/workflows/main_app-dmauth-api.yml` prints the step.
- Evidence:
  ````
  24:          dotnet-version: '10.x'
  $ ls global.json
  ls: cannot access 'global.json': No such file or directory
  ````

### RP-008 .editorconfig code-style warnings are not enforced at build time
- Severity: hygiene
- Layer: repo
- Location: `Directory.Build.props:2`; `.editorconfig:42`
- Rule: external Code-style analysis: EnforceCodeStyleInBuild https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview#code-style-analysis
- Moves: n/a
- What: Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` (and decide on `<TreatWarningsAsErrors>` / `<AnalysisLevel>`) to the `PropertyGroup` in `Directory.Build.props`, so the `IDExxxx` rules `.editorconfig` sets to `:warning` fire in `dotnet build` and CI.
- Why: code-style (IDE) rules surface only in the IDE unless `EnforceCodeStyleInBuild` is set; no `Directory.Build.props` or project file sets it, so the project's `var`, braces, file-scoped namespace and naming rules are never checked by a build.
- Verify: `grep -n 'EnforceCodeStyleInBuild' Directory.Build.props` prints the property, and `dotnet build src/DM-Auth.sln` reports IDE diagnostics on a seeded violation.
- Census: 24 via `grep -c ':warning' .editorconfig`
- Evidence:
  ````
  2:  <PropertyGroup>
  42:csharp_style_var_for_built_in_types = true:warning
  $ grep -n -E 'EnforceCodeStyleInBuild|TreatWarningsAsErrors' Directory.Build.props; echo "exit=$?"
  exit=1
  ````

### RP-009 .editorconfig sets csharp_style_prefer_readonly_struct twice
- Severity: hygiene
- Layer: repo
- Location: `.editorconfig:74`; `.editorconfig:85`
- Rule: external EditorConfig specification https://spec.editorconfig.org/
- Moves: n/a
- What: Delete the duplicate key at line 85 under `# Modifier preferences`, keeping the one at line 74.
- Why: a repeated key silently lets the later value win, so a future edit to one copy is overridden by the other.
- Verify: `grep -c '^csharp_style_prefer_readonly_struct ' .editorconfig` prints 1.
- Evidence:
  ````
  74:csharp_style_prefer_readonly_struct = true:suggestion
  85:csharp_style_prefer_readonly_struct = true:suggestion
  ````

### RP-010 Stale dmauth-web name in the ignore file and the commit-scope table
- Severity: hygiene
- Layer: repo
- Location: `.gitignore:411`; `README.md:565`
- Rule: external gitignore documentation https://git-scm.com/docs/gitignore
- Moves: n/a
- What: Remove the dead `dmauth-web/dist/` rule (the SPA is `src/DMAuth.Client`, whose own `.gitignore` already ignores `dist`) and change the `spa` scope description to `DMAuth.Client React SPA`.
- Why: no `dmauth-web` directory exists, so the rule ignores nothing and both lines point readers at a project name that is gone.
- Verify: `grep -rn 'dmauth-web' .gitignore README.md` prints nothing.
- Evidence:
  ````
  .gitignore
  411:dmauth-web/dist/
  README.md
  565:| `spa` | dmauth-web React SPA |
  ````

### RP-011 README's reportgenerator output directory coverage-report/ is not ignored
- Severity: hygiene
- Layer: repo
- Location: `README.md:394`; `.gitignore`
- Rule: external gitignore documentation https://git-scm.com/docs/gitignore
- Moves: n/a
- What: Add `coverage-report/` to the root `.gitignore` (the file lacks it), next to the Coverlet patterns.
- Why: following the README's .NET coverage instructions writes an HTML report to `coverage-report/` at the repository root, which `git status` then offers for commit, repeating RP-001 for the .NET side.
- Verify: `git check-ignore coverage-report/index.html` prints the path.
- Evidence:
  ````
  394:reportgenerator -reports:"tests/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
  $ grep -n -i 'coverage-report' .gitignore; echo "exit=$?"
  exit=1
  ````

### RP-012 Local SQL Server publishes port 1433 on every host interface with the sa login
- Severity: correctness/security risk
- Layer: repo
- Location: `eng/dev/docker-compose.yml:10`; `README.md:97`
- Rule: external Compose file reference: ports https://docs.docker.com/reference/compose-file/services/#ports
- Moves: n/a
- What: Bind the port to loopback, `- "127.0.0.1:1433:1433"`, so the container is reachable only from the developer machine, matching the README's `localhost,1433` description.
- Why: the short form `"1433:1433"` publishes on all interfaces, exposing an `sa` login (with the README's example password `YourStrong!Pass123`) to anyone on the same network.
- Verify: `grep -n '127.0.0.1:1433:1433' eng/dev/docker-compose.yml` prints the mapping.
- Evidence:
  ````
  eng/dev/docker-compose.yml
  10:      - "1433:1433"
  README.md
  97:The container exposes SQL Server on `localhost,1433`. Data is persisted in the `dmauth-db-data` Docker volume across restarts.
  ````

### RP-013 docker-compose uses a floating image tag and does not require SA_PASSWORD
- Severity: hygiene
- Layer: repo
- Location: `eng/dev/docker-compose.yml:3`; `eng/dev/docker-compose.yml:7`
- Rule: external Compose interpolation (required variables) https://docs.docker.com/reference/compose-file/interpolation/; external Docker build best practices: pin image versions https://docs.docker.com/build/building/best-practices/#pin-base-image-versions
- Moves: n/a
- What: Pin the image to a specific SQL Server 2022 CU tag (or digest) and write the variable as `"${SA_PASSWORD:?Set SA_PASSWORD before docker compose up}"`.
- Why: `2022-latest` changes under contributors without a commit, and an unset `SA_PASSWORD` interpolates to an empty string, so the container starts and then fails its password policy with a confusing error instead of compose refusing up front.
- Verify: `grep -n -E 'server:2022-latest' eng/dev/docker-compose.yml` prints nothing and `grep -n 'SA_PASSWORD:?' eng/dev/docker-compose.yml` prints line 7.
- Evidence:
  ````
  3:    image: mcr.microsoft.com/mssql/server:2022-latest
  7:      MSSQL_SA_PASSWORD: "${SA_PASSWORD}"
  ````

### RP-014 setup.ps1 seeds a known-password active account into whatever vault it is given
- Severity: correctness/security risk
- Layer: repo
- Location: `eng/dev/setup.ps1:10`; `eng/dev/setup.ps1:69`; `eng/dev/seed-data.sql:5`
- Rule: external OWASP Secrets Management Cheat Sheet https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html
- Moves: n/a
- What: Guard `setup.ps1` so it refuses to run unless the vault is an approved development vault (e.g. a `ValidateSet` or a check against the `kv-dmauth-dev` URI in `appsettings.Development.json`) and require an explicit `-Seed` switch before running `seed-data.sql`.
- Why: the script reads `ConnectionStrings--DmAuth` from any vault name and seeds an active, email-verified `test@example.com` whose password `testpassword123` is published in the repo; production uses the same secret name, so one wrong `-VaultName` plants a known credential in the production database.
- Verify: running `.\eng\dev\setup.ps1 -VaultName "<non-dev vault>"` exits non-zero before any `az keyvault secret show` call.
- Evidence:
  ````
  eng/dev/setup.ps1
  10:    [string]$VaultName
  69:    Invoke-Sqlcmd -ConnectionString $ConnectionString -InputFile $SeedScriptPath -ErrorAction Stop
  eng/dev/seed-data.sql
  5:-- Password: testpassword123 (BCrypt work factor 12)
  ````

### RP-015 seed-data.sql is not idempotent, so re-running setup.ps1 fails
- Severity: hygiene
- Layer: repo
- Location: `eng/dev/seed-data.sql:6`; `eng/dev/seed-data.sql:27`
- Rule: external EF Core data seeding https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
- Moves: n/a
- What: Wrap each insert in `IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'test@example.com')` / `IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE [ClientId] = 'dmauth_test_client')`.
- Why: `UserConfiguration` declares unique indexes, so a second run violates them, `Invoke-Sqlcmd -ErrorAction Stop` throws, and `setup.ps1` reports `[FAIL] Seeding failed` and exits 1 on an already-set-up database.
- Verify: running `sqlcmd ... -i eng/dev/seed-data.sql` twice against the same database succeeds both times.
- Evidence:
  ````
  6:INSERT INTO [Users] (
  27:INSERT INTO [Clients] (
  ````

### RP-016 README says setup.ps1 needs PowerShell 7.1 for the ?? operator, which it does not use
- Severity: hygiene
- Layer: repo
- Location: `README.md:18`
- Rule: external about_Requires https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_requires
- Moves: n/a
- What: Either drop the `??` justification and state the real minimum, or add `#Requires -Version 7.1` to `setup.ps1` if 7.1 is truly required, so the README and the script agree.
- Why: the stated reason is false (`setup.ps1` contains no `??`), and the script declares no version requirement, so the prerequisite is neither true nor enforced.
- Verify: `grep -n -E '\?\?|#Requires' eng/dev/setup.ps1` agrees with the README's stated prerequisite.
- Evidence:
  ````
  18:- [PowerShell 7.1+](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell) — required by `setup.ps1` (`??` operator); the `SqlServer` module is installed automatically on first run
  $ grep -n '??' eng/dev/setup.ps1; echo "exit=$?"
  exit=1
  ````

### RP-017 README's Node.js 18+ prerequisite is below the locked Vite's engine range
- Severity: hygiene
- Layer: repo
- Location: `README.md:20`
- Rule: external Vite guide: Node.js version requirement https://vite.dev/guide/
- Moves: n/a
- What: Change the prerequisite to `Node.js 20.19+ or 22.12+`, the `engines.node` range `^20.19.0 || >=22.12.0` that `src/DMAuth.Client/package-lock.json` records for `vite` 8.0.8, and consider adding `engines` to `package.json`.
- Why: a contributor on Node 18 following the README gets engine warnings and a Vite that does not run.
- Verify: the README's Node prerequisite matches the `engines.node` of `node_modules/vite` in `src/DMAuth.Client/package-lock.json`.
- Evidence:
  ````
  20:- [Node.js 18+](https://nodejs.org/) (for the React SPA)
  ````

### RP-018 README's Key Vault name dmauth-dev differs from the configured dev vault
- Severity: hygiene
- Layer: repo
- Location: `README.md:55`; `README.md:70`
- Rule: external Write the Docs: Docs as Code https://www.writethedocs.org/guide/docs-as-code/
- Moves: n/a
- What: Replace `dmauth-dev` with `kv-dmauth-dev`, the vault `src/DMAuth.Web/appsettings.Development.json` configures (`"KeyVault": "https://kv-dmauth-dev.vault.azure.net/"`).
- Why: the copy-paste commands for Options A and B target a vault name the application does not use, so they fail or hit the wrong vault.
- Verify: `grep -n 'dmauth-dev' README.md | grep -v 'kv-dmauth-dev'` prints nothing.
- Evidence:
  ````
  55:.\eng\dev\setup.ps1 -VaultName "dmauth-dev"
  70:CS=$(az keyvault secret show --vault-name dmauth-dev --name "ConnectionStrings--DmAuth" --query "value" -o tsv)
  ````

### RP-019 README names the seeded client test_client; the seed inserts dmauth_test_client
- Severity: hygiene
- Layer: repo
- Location: `README.md:83`; `eng/dev/seed-data.sql:40`
- Rule: external Write the Docs: Docs as Code https://www.writethedocs.org/guide/docs-as-code/
- Moves: n/a
- What: Change the README's client id to `dmauth_test_client`.
- Why: a developer testing the OAuth flow with the documented `client_id=test_client` gets an unknown-client error.
- Verify: `grep -n 'dmauth_test_client' README.md` prints the seed description.
- Evidence:
  ````
  README.md
  83:This inserts a test user (`test@example.com` / `testpassword123`) and a public test client (`test_client`).
  eng/dev/seed-data.sql
  40:    'dmauth_test_client',
  ````

### RP-020 README's user-secrets key DmAuthConnection is not the connection string the code reads
- Severity: convention drift
- Layer: repo
- Location: `README.md:103`
- Rule: discipline .sir/disciplines/dotnet.md § Configuration and secrets
- Moves: code
- What: Change the key to `ConnectionStrings:DmAuth`, the name the discipline fixes and `src/DMAuth.Infrastructure/DependencyInjection.cs` reads with `GetConnectionString("DmAuth")`.
- Why: a secret stored under `DmAuthConnection` is never read, so Option C silently uses whatever `DmAuth` resolves to instead of the local container.
- Verify: `grep -n 'DmAuthConnection' README.md` prints nothing.
- Evidence:
  ````
  103:dotnet user-secrets set "ConnectionStrings:DmAuthConnection" \
  ````

### RP-021 "No Azure access required" for the Docker option is false: startup requires Key Vault
- Severity: hygiene
- Layer: repo
- Location: `README.md:48`; `README.md:85`; `docs/architecture-decisions.md:229`
- Rule: external Azure Key Vault configuration provider in ASP.NET Core https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration; external Configuration in ASP.NET Core: provider order https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/
- Moves: n/a
- What: Either correct Option C and ADR-009 to state that Key Vault access is still required (and that the vault's `ConnectionStrings--DmAuth` overrides a user-secret connection string), or make the Key Vault provider optional in Development so a local-only setup works as documented.
- Why: `src/DMAuth.Web/Program.cs:22` throws `InvalidOperationException` when `ConnectionStrings:KeyVault` is empty and then registers `AddAzureKeyVault` with `DefaultAzureCredential`, added after user secrets so its values win; a contributor without Azure access cannot start the API, and one with access gets the Azure dev database instead of the container.
- Verify: with no `az login`, `dotnet run --project src/DMAuth.Web` using only the Option C steps starts and connects to `localhost,1433`, or the docs no longer claim it does.
- Evidence:
  ````
  README.md
  48:Choose one of three options based on your setup. Options A and B use the shared Azure dev database. Option C spins up a local SQL Server container — no Azure access required.
  85:#### Option C — Local Docker (no Azure access required)
  docs/architecture-decisions.md
  229:- Developer choice: use the local container or the Azure Dev DB — both are valid; the connection string in user secrets determines which
  ````

### RP-022 Layer-dependency rule says Infrastructure depends only on Domain; it depends on Application
- Severity: convention drift
- Layer: repo
- Location: `README.md:186`; `README.md:190`; `.sir/disciplines/dotnet.md:12`
- Rule: discipline .sir/disciplines/dotnet.md § Solution and layers; external Common web application architectures: Clean Architecture https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture
- Moves: discipline
- What: Restate the dependency rule in `.sir/disciplines/dotnet.md:12` and README line 186 as `Infrastructure → Application, Domain`, and correct README line 190: `TokenService` and `BcryptPasswordHasher` are implemented in Infrastructure (`src/DMAuth.Infrastructure/Services/TokenService.cs`, `src/DMAuth.Infrastructure/Security/BcryptPasswordHasher.cs`) against interfaces in `Application/Common/Interfaces/`.
- Why: `src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:16` references `DMAuth.Application`, which is required because Infrastructure implements `ITokenService` and `IPasswordHasher` declared in Application, the standard Clean Architecture direction and the discipline's own `Common/Interfaces/` placement; the code is evidence, so the rule moves.
- Verify: `grep -n 'Infrastructure → ' .sir/disciplines/dotnet.md README.md` shows `Application` among Infrastructure's dependencies, and `grep -n 'TokenService, PasswordHasher' README.md` no longer attributes them to Application.
- Evidence:
  ````
  README.md
  186:Infrastructure → Domain
  190:The Domain project has zero external dependencies. Application contains business logic and service implementations (TokenService, PasswordHasher). Infrastructure handles data persistence and external service integrations (EF Core, Key Vault). Web is the composition root that wires everything together via DI.
  .sir/disciplines/dotnet.md
  12:- Dependencies point only this way: Application → Domain; Infrastructure → Domain; Web → Application, Infrastructure. Domain has zero external dependencies. Web is the composition root.
  ````

### RP-023 README and ADR-001 document Commands/, Queries/ and Dto folders the discipline forbids and the code lacks
- Severity: convention drift
- Layer: repo
- Location: `README.md:200`; `README.md:202`; `README.md:204`; `README.md:207`; `docs/architecture-decisions.md:18`
- Rule: discipline .sir/disciplines/dotnet.md § Solution and layers
- Moves: code
- What: Replace the README's feature-layout block with the layout the discipline and code use, `Features/{Feature}/{Operation}/` holding `{Operation}Command|Query`, `{Operation}Handler`, `{Operation}Validator`, `{Operation}Response` (e.g. `Clients/GetById/`), and reword ADR-001's consequence line to match.
- Why: the docs teach `Commands/`, `Queries/`, `…CommandHandler` and `…Dto`, which `dotnet.md` explicitly forbids and which no folder under `src/DMAuth.Application/Features` uses, so a contributor following the README produces code that fails review.
- Verify: `grep -n -E 'Commands/|Queries/|CommandHandler|Dto\.cs' README.md docs/architecture-decisions.md` prints nothing.
- Census: 0 via `find src/DMAuth.Application/Features -type d \( -name Commands -o -name Queries \) | wc -l`
- Evidence:
  ````
  README.md
  200:  Commands/{CommandName}/
  202:    {CommandName}CommandHandler.cs
  204:  Queries/{QueryName}/
  207:    {QueryName}Dto.cs
  docs/architecture-decisions.md
  18:- Feature-organized folder structure (Features/{Feature}/Commands/ and Queries/) makes the codebase navigable and enforces single-responsibility at the handler level
  ````

### RP-024 README links to a nonexistent ADR file
- Severity: hygiene
- Layer: repo
- Location: `README.md:210`
- Rule: external Write the Docs: Docs as Code https://www.writethedocs.org/guide/docs-as-code/
- Moves: n/a
- What: Point the link at `docs/architecture-decisions.md#adr-001-cqrs-lite-for-project-structure`, the only ADR file present.
- Why: `docs/` holds a single `architecture-decisions.md`; the linked `docs/architecture-decisions/001-cqrs-lite.md` 404s.
- Verify: every relative link target in `README.md` exists (`ls docs/architecture-decisions.md`), and `grep -n '001-cqrs-lite.md' README.md` prints nothing.
- Evidence:
  ````
  210:See [docs/architecture-decisions/001-cqrs-lite.md](docs/architecture-decisions/001-cqrs-lite.md) for the full rationale.
  $ ls docs/architecture-decisions/001-cqrs-lite.md
  ls: cannot access 'docs/architecture-decisions/001-cqrs-lite.md': No such file or directory
  ````

### RP-025 README Testing section's counts are stale
- Severity: hygiene
- Layer: repo
- Location: `README.md:356`; `README.md:428`; `README.md:429`
- Rule: external Write the Docs: Docs as Code https://www.writethedocs.org/guide/docs-as-code/
- Moves: n/a
- What: Say "four test tiers" (the table under it has four rows), and replace the literal Vitest summary with a shape-only example (or drop the numbers), since the client now has more test files than the 9 shown.
- Why: hard-coded counts go stale silently; the sample says `9 passed (9)` test files while `src/DMAuth.Client/src` holds more, so a reader cannot tell a correct run from a missing suite.
- Verify: `grep -n -E 'three test tiers|9 passed \(9\)' README.md` prints nothing.
- Census: 16 via `find src/DMAuth.Client/src -type f \( -name '*.test.ts' -o -name '*.test.tsx' \) | wc -l`
- Evidence:
  ````
  356:The project has three test tiers:
  428:Test Files  9 passed (9)
  429:     Tests  43 passed (43)
  ````

### RP-026 README documents the vault setting as KeyVault:VaultUri; the code reads ConnectionStrings:KeyVault
- Severity: hygiene
- Layer: repo
- Location: `README.md:330`; `README.md:510`
- Rule: external Configuration in ASP.NET Core: environment variables https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#non-prefixed-environment-variables
- Moves: n/a
- What: Rename the documented setting to `ConnectionStrings:KeyVault` / App Service setting `ConnectionStrings__KeyVault` (what `src/DMAuth.Web/Program.cs:22` reads via `GetConnectionString("KeyVault")` and `appsettings*.json` declare), and fix the sample `appsettings.json` block and the "only setting" sentence above a four-row table.
- Why: an operator who sets `KeyVault__VaultUri` as documented gets `InvalidOperationException: Key Vault connection string cannot be empty.` at startup.
- Verify: `grep -n 'VaultUri' README.md` prints nothing.
- Evidence:
  ````
  330:The only non-secret Key Vault setting is `KeyVault:VaultUri`, which tells the app where to find its vault. Set this in `appsettings.Development.json` per-environment and override it in production via an App Service Application Setting (`KeyVault__VaultUri`).
  510:| `KeyVault__VaultUri` | Production Key Vault URI (e.g. `https://dmauth-prod.vault.azure.net/`) |
  ````

### RP-027 README and ADR-007 claim a SameSite=Strict session cookie; the code sets SameSite=None
- Severity: hygiene
- Layer: repo
- Location: `README.md:529`; `docs/architecture-decisions.md:168`; `docs/architecture-decisions.md:176`
- Rule: external Work with SameSite cookies in ASP.NET Core https://learn.microsoft.com/en-us/aspnet/core/security/samesite
- Moves: n/a
- What: Document the actual `SameSiteMode.None` (`src/DMAuth.Web/Program.cs:59`, needed because the SPA and API are on different sites), amend ADR-007 with that decision, and state which control now provides CSRF protection instead of attributing it to SameSite.
- Why: the security section and ADR assert a CSRF defence the cookie does not have, so a reviewer relying on the docs misjudges the dashboard's CSRF exposure.
- Verify: `grep -n 'SameSite=Strict' README.md docs/architecture-decisions.md` prints nothing, or the code sets `SameSiteMode.Strict`.
- Evidence:
  ````
  README.md
  529:- **Session cookies** — HttpOnly, Secure, SameSite=Strict for the dashboard
  docs/architecture-decisions.md
  168:- **Cookie authentication** (default scheme) — the dashboard authenticates via username/password to `/api/users/login` and receives an `HttpOnly`, `SameSite=Strict`, `Secure` session cookie valid for 24 hours with sliding expiration
  176:- Cookie security properties (HttpOnly, SameSite=Strict, Secure) provide CSRF protection and prevent JavaScript access to the session token
  ````

### RP-028 ADR-005 says reuse detection follows the ReplacedByToken chain; the code revokes by FamilyId
- Severity: hygiene
- Layer: repo
- Location: `docs/architecture-decisions.md:108`
- Rule: external OAuth 2.0 Security Best Current Practice (RFC 9700) §4.14 refresh token protection https://www.rfc-editor.org/rfc/rfc9700#section-4.14
- Moves: n/a
- What: Reword the decision to say the family is every token sharing the `FamilyId` assigned at the first exchange, which `RevokeByTokenFamilyAsync` revokes (`src/DMAuth.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs:28`).
- Why: the ADR describes a chain walk the code does not perform, misleading anyone auditing or changing reuse detection.
- Verify: `grep -n 'ReplacedByToken. chain' docs/architecture-decisions.md` prints nothing and the ADR names `FamilyId`.
- Evidence:
  ````
  108:Implement refresh token rotation: every time a refresh token is used, a new refresh token is issued and the old one is revoked. Additionally, implement reuse detection: if a revoked refresh token is presented, revoke the entire token family (all tokens linked via the `ReplacedByToken` chain).
  ````

### RP-029 ADR security rationale cites the wrong standards and mischaracterises BCrypt
- Severity: hygiene
- Layer: repo
- Location: `docs/architecture-decisions.md:66`; `docs/architecture-decisions.md:88`; `docs/architecture-decisions.md:177`
- Rule: external OAuth 2.0 Security Best Current Practice (RFC 9700) https://www.rfc-editor.org/rfc/rfc9700; external OAuth 2.0 Pushed Authorization Requests (RFC 9126) https://www.rfc-editor.org/rfc/rfc9126; external OpenID Connect Core 1.0 §5.3 UserInfo Endpoint https://openid.net/specs/openid-connect-core-1_0.html#UserInfo; external OWASP Password Storage Cheat Sheet https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
- Moves: n/a
- What: Cite RFC 9700 (not RFC 9126, which defines Pushed Authorization Requests) as the current best practice in ADR-003; cite OpenID Connect Core 1.0 §5.3 (not RFC 5749) for UserInfo in ADR-007; and drop "memory-hard" from ADR-004 (bcrypt's GPU resistance comes from its tunable cost and its small fixed state; memory-hard functions are scrypt and Argon2).
- Why: the ADRs are the stated rationale for the server's security posture; wrong citations send readers to unrelated specifications.
- Verify: `grep -n -E 'RFC 9126\)|RFC 5749|memory-hard' docs/architecture-decisions.md` prints nothing.
- Evidence:
  ````
  66:- Aligns with OAuth 2.1 draft and current best practice (RFC 9126)
  88:- BCrypt is a well-established, widely audited algorithm with proven resistance to GPU-based attacks due to its memory-hard properties
  177:- The `/connect/userinfo` endpoint correctly requires a Bearer token, conforming to the OIDC UserInfo spec (RFC 5749)
  ````

### RP-030 README commit format omits the colon the repository's history uses
- Severity: hygiene
- Layer: repo
- Location: `README.md:538`
- Rule: external Conventional Commits 1.0.0 https://www.conventionalcommits.org/en/v1.0.0/
- Moves: n/a
- What: Change the documented format to `type(scope): Summary` and add the colon to the examples below it.
- Why: the session's starting git context shows recent subjects as `chore(sir): name branches <type>/<slug> per sir-sdlc 1.0.50` and `chore(sir): tend lessons`, the Conventional Commits form, so the README teaches a format the project does not follow.
- Verify: `git log --no-merges --format=%s -20` and the README format agree on `type(scope): `.
- Evidence:
  ````
  538:type(scope) Summary
  ````
