# Retired lessons

<!-- Lessons that stopped being true, kept for a while in case a session meets the trap
     again. One line per retired lesson: the original `lessons.md` line verbatim, then
     ` — retired <YYYY-MM-DD> after <session id>: <reason>; evidence: <command or file:line>`.
     `<session id>` is the last wrapped session at the time of retirement, or `none`
     when `.sir/log.md` has no session line; a line retired `after none` never purges.

     Purge rule: a line is deleted once `upkeep.retiredRetentionSessions` wrapped
     sessions have run after `<session id>`; tend lines do not count.

     Restore rule: if a session meets a trap a retired lesson describes, wrap moves
     the line back to `lessons.md` with ` [restored <YYYY-MM-DD> <session id>]`
     instead of writing a new lesson. Check by grepping this file for the lesson's
     anchor before appending.

     Never create this file empty: it is made on the first retirement, and only then. -->

<!-- Example: 2026-10-26 20261026-1238-oauth-login [project] The token cache lives in `src/auth/cache.ts`. — retired 2026-11-18 after 20261117-0900-cache-move: the cache moved to `src/session/cache.ts`; evidence: git log --diff-filter=D -- src/auth/cache.ts -->

2026-11-20 20261120-0315-sir-bootstrap [framework] Fixing advisories at Gate 3 is a full revision round per task: three parallel fixes cost three implementers, three re-reviews and three commits, and the re-reviews raised six new advisories that needed a second disposition ask before approval. — retired 2026-11-20 after 20261120-0315-sir-bootstrap: redundant-plugin: the plugin now states that a Gate 3 fix on Standard/Large is a full revision round whose re-review can raise findings of its own; evidence: plugin:skills/sir-phase-review/SKILL.md:90
2026-11-20 20261120-0315-sir-bootstrap [harness] A `sed -i` substitution writing `\|` into a markdown table cell had its backslash halved by the tool layer, producing an unescaped pipe; the masked field-count check on coverage.md caught it, and the Edit tool wrote the escape correctly. — retired 2026-11-20 after 20261120-0315-sir-bootstrap: redundant-plugin: sir-shell rule 7 states that the tool layer halves a backslash and cites the same coverage.md pipe-masking case; evidence: plugin:skills/sir-shell/SKILL.md:73
