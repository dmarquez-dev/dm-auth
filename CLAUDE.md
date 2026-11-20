# DM Auth

## Branching policy for SIR sessions

These rules are binding for every SIR session in this repository.

1. Start from an up-to-date `main`: pull the latest `main` first, then create a fresh feature
   branch from it and do all of the session's work on that feature branch.
2. Commit and push the session's work on that feature branch. Never commit or push directly on
   `main`.
3. Open a pull request from that feature branch into `main`.
4. Do not end the session until that pull request is merged (completed). Wait for the merge, so
   `main` is current and the next SIR session starts from the latest changes.

## Conventions

Project code conventions live in `.sir/disciplines/`, wired to each domain by `.sir/config.json`.
