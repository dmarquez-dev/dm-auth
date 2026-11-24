# DM Auth

## Git and SIR sessions

Every change reaches `main` through a pull request. Nothing is committed to `main` locally and `main` is never pushed. SIR sessions run in PR mode (`git.integration: "pr"` in `.sir/config.json`): the plugin names the branch, commits the session's records on it, pushes it, opens the PR, waits for it to merge and cleans up, all under the approval asked at Gate 3. A tend runs the same way on its own branch. Merge PRs with a merge commit, never a squash or rebase, because the `.sir/` records cite commit hashes.

## Conventions

Project code conventions live in `.sir/disciplines/`, wired to each domain by `.sir/config.json`.
