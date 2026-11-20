<!-- frozen: 20261120-0315-sir-bootstrap, Gate 2 -->

# Contract: discipline fragments, their domain wiring, and the port ledger

Declarations only. Consumers: T4 (writes `.sir/config.json`, repoints `README.md`, deletes the
source docs), the orchestrator (persists each porting task's ledger), and every reviewer of T1–T3.

## 1. Fragment set

Every fragment is a file at `.sir/disciplines/<name>.md` in the repository. No other file is
created under `.sir/disciplines/`.

| Name | Path | Owning task | Line budget (`wc -l`, max) | Listed under domains |
|---|---|---|---|---|
| `csharp-style` | `.sir/disciplines/csharp-style.md` | T1 | 110 | backend, database, test |
| `dotnet` | `.sir/disciplines/dotnet.md` | T1 | 220 | backend |
| `efcore` | `.sir/disciplines/efcore.md` | T1 | 40 | backend, database |
| `dotnet-tests` | `.sir/disciplines/dotnet-tests.md` | T2 | 60 | test |
| `client-tests` | `.sir/disciplines/client-tests.md` | T2 | 190 | test |
| `typescript-style` | `.sir/disciplines/typescript-style.md` | T3 | 80 | frontend, test |
| `react` | `.sir/disciplines/react.md` | T3 | 330 | frontend |

Topic of each fragment (fragments are topically disjoint; a rule has exactly one home):

- `csharp-style` — rules binding every C# file, production and test alike: naming, file
  organization, parameter/inheritance/lambda/chaining layout, XML documentation comments.
- `dotnet` — server architecture and ASP.NET Core practice: project/layer structure, CQRS shapes,
  entities, value objects, policies, async/cancellation, DI, error handling, HttpContext safety,
  memory, HTTP clients, exceptions, request/response handling, performance, configuration and
  secrets.
- `efcore` — data access through EF Core.
- `dotnet-tests` — C# test conventions (xUnit/NSubstitute/assertions/structure/isolation).
- `client-tests` — Vitest and Playwright conventions for `DMAuth.Client`.
- `typescript-style` — rules binding every TypeScript/TSX file, source and test alike: file
  naming, component style, exports, props, styling, import organization.
- `react` — React/SPA architecture and performance: API client module structure, TanStack Query
  integration, form validation, project structure, state management, and the React performance
  rules.

## 2. `.sir/config.json` `disciplines` value

Exact value, keys in this order, arrays in this order (order is composition order). A
`"$comment"` key may additionally be present.

```json
{
  "backend": ["csharp-style", "dotnet", "efcore"],
  "frontend": ["typescript-style", "react"],
  "database": ["csharp-style", "efcore"],
  "test": ["csharp-style", "typescript-style", "dotnet-tests", "client-tests"]
}
```

Composed-size bound per domain (conservative; over-counts the real composition, which drops
frontmatter): `wc -l` of the bundled base + Σ over listed fragments of (`wc -l` + 1) ≤ 500.
With every fragment at its budget: backend 418, database 190, frontend 449, test 484.

## 3. Port unit

A **unit** is one line of a source doc, read at `2e0f9e2` (identical to the worktree copy until T4
deletes it), that matches `^#{1,3} ` and lies outside a ``` fence. The `#` (H1) unit stands for the
doc's preamble — the text between the title and the next heading. A unit's body is the text from
its line to the next unit line.

## 4. Unit assignment

| Source doc | Units | Lines | Task |
|---|---|---|---|
| `docs/backend-conventions.md` | 17 | 1–209 | T1 |
| `docs/backend-conventions.md` | 6 | 210–end (§9 Testing Conventions) | T2 |
| `docs/dotnet-format.md` | 20 | 1–509 | T1 |
| `docs/dotnet-format.md` | 4 | 510–end (§10 Testing in C#) | T2 |
| `docs/dotnet-guidelines.md` | 37 | all | T1 |
| `docs/frontend-conventions.md` | 6 | 1–94 | T3 |
| `docs/frontend-conventions.md` | 3 | 95–end (§5 Testing Conventions) | T2 |
| `docs/playwright-guidelines.md` | 17 | all | T2 |
| `docs/react-format.md` | 11 | 1–218 | T3 |
| `docs/react-format.md` | 3 | 219–end (§9 Testing Patterns) | T2 |
| `docs/react-guidelines.md` | 68 | all | T3 |
| `docs/vitest-guidelines.md` | 19 | all | T2 |

Totals: T1 74, T2 52, T3 85; 211 units.

A task carries a unit's rules only into its own fragments (§1). A rule whose topic belongs to a
peer's fragment is a contract-change escalation, never a write into the peer's file and never a
silent drop.

## 5. Memory-only conventions (no source doc; placed by T1)

| # | Convention | Fragment |
|---|---|---|
| M1 | XML doc description text indented two tabs after `///`; a long sentence wraps onto a new `///` + two-tab line instead of running past line length | `csharp-style` |
| M2 | Lambda parameters have descriptive names (`user =>`, `client =>`, `token =>`), never a single letter | `csharp-style` |
| M3 | Connection string named `DmAuth`, read as `GetConnectionString("DmAuth")`; Key Vault secret `ConnectionStrings--DmAuth` | `dotnet` |
| M4 | Private static fields are `_camelCase`, like every private field | `csharp-style` |
| M5 | Sensitive settings (credentials, private keys) are `null` in `appsettings.json` and every `appsettings.{Environment}.json`, supplied via user secrets or Key Vault; non-secret settings keep safe defaults (empty string or number) | `dotnet` |

Precedence: where a source doc rule or example contradicts M1–M5, the M rule is carried and the
ledger row for that unit says so.

Precedence (amendment A1, E2): where a source doc rule contradicts the repository's own practice,
the repository's practice is carried — that counts as `carried` — and the ledger row for that unit
says so, naming the repository evidence. M1–M5 still win over both.

## 6. Port ledger (one per porting task)

Produced by T1, T2, T3 inline in the task report; persisted by the orchestrator, before review
dispatch, to `<session dir>/port-ledger/T<n>.md`. Never written into the repository tree.

Shape: one markdown table, header exactly

```
| Unit | Heading | Disposition | Where or why |
```

one row per unit assigned to the task (§4), no other rows.

- `Unit` — `docs/<file>:<line>`, the line number at `2e0f9e2`.
- `Heading` — the heading text after the `#` marks, verbatim.
- `Disposition` — exactly one of `carried`, `dropped`, `container`.
  - `carried` — the unit body's rules are stated in one of the task's fragments.
  - `dropped` — the unit body holds rules none of which is carried.
  - `container` — the unit body holds no rule (heading, table of contents, or lead-in only); its
    rules, if any, are carried by its child units.
- `Where or why` — `carried`: `<fragment>.md` and the fragment heading it went under; add
  `overridden by M<n>` where §5 precedence applied. `dropped`: the reason (e.g. Next.js/RSC/SSR
  only, duplicated by another unit — name it, enforced by `.editorconfig` — name the key,
  illustration of a rule carried elsewhere — name it). `container`: `-` or a short note.

Escaping: a literal `|` inside a cell is written `\|`. The sequence `\|` meaning a literal
backslash followed by a pipe cannot occur: no unit heading contains `|` or `\` (measured over all
211 units), and `Where or why` text is authored and must not contain a backslash.

## 7. Error cases

| Condition | How it surfaces |
|---|---|
| A name listed in `disciplines` has no `.sir/disciplines/<name>.md` | Composition fails and every later session stops; T4's missing/orphan check reports `MISSING n>0` |
| A `.sir/disciplines/*.md` not listed in `disciplines` | Never composed; T4's check reports `ORPHAN n>0` |
| A fragment exceeds its budget | Owning task fails its budget criterion; budgets are not borrowed between tasks without a contract amendment |
| A domain's composed bound exceeds 500 | T4's size check prints `OVER`; reported, never a stop (sir-disciplines size cap) |
| A unit's rule fits no fragment the task owns | Contract-change escalation (§4) |
