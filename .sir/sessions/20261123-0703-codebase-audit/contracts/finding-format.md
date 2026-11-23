<!-- frozen: 20261123-0703-codebase-audit, Gate 2 -->

# Contract: audit finding record, layer audit file, and backlog row

Markdown fallback: no language-native form exists for a document format. This file holds
declarations only.

Producers: T1-T7 (one layer audit file each). Consumer: T8 (consolidates into `.sir/backlog.md`
and `audit/README.md`).

All paths below are repository-root-relative with forward slashes. `AUDIT/` abbreviates
`.sir/sessions/20261123-0703-codebase-audit/audit/` in the worktree.

## 1. Layers, ID prefixes, files and scope

Every scoped path belongs to exactly one layer. "Scope command" is the operative definition of
a layer's file population: run it with `<R>` set to the worktree root and strip the `<R>/`
prefix from its output. At base `5b7d9a6` no product, test or config file is modified by any
task, so the worktree's content of every scoped file is its content at `5b7d9a6`.

| Task | Prefix | Layer value | Audit file | Wired fragments (`.sir/disciplines/`) |
|---|---|---|---|---|
| T1 | `DA` | `backend` | `AUDIT/backend-domain-application.md` | `csharp-style.md`, `dotnet.md`, `efcore.md` |
| T2 | `IW` | `backend` | `AUDIT/backend-infrastructure-web.md` | `csharp-style.md`, `dotnet.md`, `efcore.md` |
| T3 | `DB` | `database` | `AUDIT/database.md` | `csharp-style.md`, `efcore.md` |
| T4 | `FE` | `frontend` | `AUDIT/frontend.md` | `typescript-style.md`, `react.md` |
| T5 | `NT` | `test` | `AUDIT/tests-dotnet.md` | `csharp-style.md`, `dotnet-tests.md` |
| T6 | `CT` | `test` | `AUDIT/tests-client.md` | `typescript-style.md`, `client-tests.md` |
| T7 | `RP` | `repo` | `AUDIT/repo.md` | none (best practice only; may cite any fragment where a repo file restates or contradicts it) |

Scope commands:

- **T1** `find <R>/src/DMAuth.Domain <R>/src/DMAuth.Application -type f -not -path '*/bin/*' -not -path '*/obj/*'`
- **T2** `find <R>/src/DMAuth.Infrastructure <R>/src/DMAuth.Web <R>/src/DM-Auth.sln -type f -not -path '*/bin/*' -not -path '*/obj/*' -not -path '<R>/src/DMAuth.Infrastructure/Persistence/*' -not -path '<R>/src/DMAuth.Infrastructure/Migrations/*'`
- **T3** `find <R>/src/DMAuth.Infrastructure/Persistence <R>/src/DMAuth.Infrastructure/Migrations -type f`
- **T4** `find <R>/src/DMAuth.Client -type f -not -path '*/node_modules/*' -not -path '<R>/src/DMAuth.Client/coverage/*' -not -path '<R>/src/DMAuth.Client/tests/*' -not -path '<R>/src/DMAuth.Client/src/test/*' -not -name '*.test.ts' -not -name '*.test.tsx' -not -path '<R>/src/DMAuth.Client/playwright.config.ts' -not -path '<R>/src/DMAuth.Client/vitest.config.ts' -not -path '<R>/src/DMAuth.Client/tsconfig.test.json' -not -path '<R>/src/DMAuth.Client/.gitignore'`
- **T5** `find <R>/tests -type f -not -path '*/bin/*' -not -path '*/obj/*'`
- **T6** `find <R>/src/DMAuth.Client/tests <R>/src/DMAuth.Client/src <R>/src/DMAuth.Client/playwright.config.ts <R>/src/DMAuth.Client/vitest.config.ts <R>/src/DMAuth.Client/tsconfig.test.json -type f ( -path '<R>/src/DMAuth.Client/tests/*' -o -path '<R>/src/DMAuth.Client/src/test/*' -o -name '*.test.ts' -o -name '*.test.tsx' -o -name playwright.config.ts -o -name vitest.config.ts -o -name tsconfig.test.json )` (parentheses escaped for the shell)
- **T7** fixed list, no command: `Directory.Build.props`, `.editorconfig`, `.gitignore`,
  `src/DMAuth.Client/.gitignore`, `src/DMAuth.Client/coverage/` (one entry for the whole
  directory), `.github/workflows/main_app-dmauth-api.yml`,
  `.github/workflows/azure-static-web-apps-purple-island-0b65ca11e.yml`,
  `eng/dev/docker-compose.yml`, `eng/dev/seed-data.sql`, `eng/dev/setup.ps1`, `README.md`,
  `docs/architecture-decisions.md`, `CLAUDE.md`.

Out of scope for every layer: everything under `.sir/` (the disciplines are the reference, not
the subject; a finding may still say a discipline should move, see `Moves`).

## 2. Finding ID

`<PREFIX>-<NNN>`: the layer's two-letter prefix, a hyphen, a three-digit zero-padded ordinal
starting at `001`, unique within its audit file, assigned in the order findings appear. Regex:
`^(DA|IW|DB|FE|NT|CT|RP)-[0-9]{3}$`. IDs are never reused or renumbered after the file is
committed.

## 3. Finding record

One record per finding, in the audit file's `## Findings` section. A record starts with a level-3
heading and is followed by field lines, each starting at column 0 with `- <Label>: `. Fields
appear once each, in this order. Every value except `Evidence` is a single line.

```text
### <ID> <short title>
- Severity: <severity>
- Layer: <layer value>
- Location: <location>[; <location>]...
- Rule: <rule source>[; <rule source>]...
- Moves: <moves>
- What: <the change to make, imperative>
- Why: <why it matters>
- Verify: <how a later session checks it is done>
- Census: <n> via `<command>`          (optional; see below)
- Evidence:
  <fenced block, indented two spaces>
```

Field types:

- **severity** — exactly one of the three strings `correctness/security risk`,
  `convention drift`, `hygiene`. No other value, no qualifier.
  - `correctness/security risk`: wrong behaviour, a broken build/restore, data loss, or a
    security weakness.
  - `convention drift`: the code departs from a `.sir/disciplines/` rule or an established
    in-repo convention, with no behavioural defect.
  - `hygiene`: neither of the above (dead code, committed generated output, stale docs, naming
    noise not covered by a rule).
- **layer value** — the value from the table in section 1 for the producing task.
- **location** — a backticked `` `path:line` `` where `line` is the 1-based line number
  `grep -n` reports at `5b7d9a6`, or a backticked `` `path` `` with no line when the finding is
  about a whole file or the absence of something (the `What` then says which). `path` is one
  the layer's scope command prints (T7: an entry of its fixed list, or a file under its
  `src/DMAuth.Client/coverage/` entry), or for `Moves: discipline` additionally the
  `.sir/disciplines/<fragment>.md:<line>` of the rule that should be restated. At least one
  location. Where more than ten sites share a finding, list up to ten and give `Census`.
- **rule source** — one or more of:
  - `discipline .sir/disciplines/<fragment>.md § <heading>` where `<heading>` is a `## `
    heading of that fragment copied verbatim without the `## `;
  - `base <domain> § <heading>` for a section of the sir-sdlc bundled base discipline composed
    into the session's `<domain>` discipline (`backend`, `frontend`, `database`, `test`);
  - `external <title> <https URL>` for a best-practice source. A best-practice finding with no
    URL is not a valid record.
- **moves** — exactly one of `code`, `discipline`, `n/a`. A record whose `Rule` contains a
  `discipline` or `base` source carries `code` or `discipline` (project lesson: where a
  discipline rule and the code disagree, the code is evidence, and the record says which side
  moves). `n/a` only when every rule source is `external`.
- **Verify** — a command or observable check a later session can run (a build or test command
  is allowed here: it is for the session that fixes the finding, not for this one).
- **Census** — optional; `<n>` is the integer the named command printed in this session. The
  command must be scoped so it cannot read `.sir/` (search `src/`, `tests/`, or named files),
  so the audit file carrying the count is outside the population it measures.
- **Evidence** — a fenced code block, indented two spaces, holding verbatim `grep -n` output
  lines (`<line>:<text>`) for each `path:line` location in this record (for a pathless location,
  the command and output that shows the absence). The fence is a run of backticks longer than
  the longest backtick run inside the quoted lines (CommonMark fenced-code rule), so quoted text
  needs no escaping.

Error cases (an invalid record): a missing or repeated field; a severity outside the three
strings; a location whose path the layer's scope command does not print (other than the
`Moves: discipline` fragment line); a `path:line` whose `sed -n '<line>p' <path>` output differs
from the text after `<line>:` in Evidence; a `discipline` heading that is not a `## ` heading of
the named fragment; an `external` source without an `https://` URL; `Moves: n/a` beside a
`discipline` or `base` source. The consumer (T8) does not repair an invalid record: it records
the ID as `withdrawn` with the failing check named (section 6).

## 4. Layer audit file

Sections, in order, each a level-2 heading:

1. Title line `# Audit: <layer value> (<task id>)`, then a line `Ref: 5b7d9a6`.
2. `## Scope` — the scope command (or T7's fixed list) verbatim.
3. `## Files` — one line per path the scope command prints (T7: per list entry), each
   `` - `<path>` — <finding IDs comma-separated, or `no finding`> ``.
4. `## Rules checked` — one line per `## ` heading of every wired fragment for the task (section
   1), each `` - `<fragment>.md` § <heading> — <finding IDs, or `conforms`, or `n/a: <reason>`> ``.
   T7 lists instead the best-practice areas it checked, each with its `external` source.
5. `## Findings` — the records (section 3). An empty section reads `No findings.`

## 5. Backlog row (produced by T8, appended to `.sir/backlog.md`)

One line, LF-terminated, appended after the file's existing content, which stays byte-identical:

```text
2026-11-23 20261123-0703-codebase-audit [<severity>] <What> Where (at 5b7d9a6): <locations> Why: <Why> Verify: <Verify> (audit: <ID>[, <ID>]...)
```

- Date is fixed at `2026-11-23` (the session's start date, `session.json` `started`).
- `<severity>` per section 3; a row merging several findings takes the highest, ordered
  `correctness/security risk` > `convention drift` > `hygiene`.
- `<locations>` is the union of the merged findings' locations, `; `-separated; above ten sites,
  the first ten followed by `(+<n> more, see audit <ID>)`.
- The audit list names every finding ID the row covers, ascending, `, `-separated. The literal
  `(audit: ` cannot occur elsewhere in a row; it occurs exactly once, at the end.
- A row contains no newline. Rows are ordered by severity (order above), then by their first ID.
- Regex every appended row matches:
  `^2026-11-23 20261123-0703-codebase-audit \[(correctness/security risk|convention drift|hygiene)\] .+ Where \(at 5b7d9a6\): .+ Why: .+ Verify: .+ \(audit: (DA|IW|DB|FE|NT|CT|RP)-[0-9]{3}(, (DA|IW|DB|FE|NT|CT|RP)-[0-9]{3})*\)$`

## 6. Disposition index (produced by T8, `AUDIT/README.md`)

- `## Audit files` — one line per layer audit file in section 1.
- `## Dispositions` — exactly one line per finding ID found in the seven audit files:
  - `` - <ID> — row `` : the ID is in exactly one appended backlog row's audit list;
  - `` - <ID> — existing: <first five words of the existing backlog row> `` : the finding
    duplicates one of the two backlog rows that predate this session (`Bring
    src/DMAuth.Application/Features/Auth/GetUserInfo/ ...` or `Restate the one-time-initialization
    rule ...`), so no new row is written for it;
  - `` - <ID> — withdrawn: <the section 3 check that failed> ``.
- `## Merges` — one line per appended row covering more than one ID, naming the IDs and why they
  are the same change.

Guarantee consumers may rely on: an ID with disposition `existing` or `withdrawn` appears in no
backlog row, and an ID with disposition `row` appears in exactly one.
