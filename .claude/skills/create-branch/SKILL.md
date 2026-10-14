---
name: create-branch
description: Read a GitHub issue and create a branch following repository conventions.
argument-hint: "[issue number]"
disable-model-invocation: true
---
Prepare a branch for the GitHub issue given as the argument (ask for the number if missing).

1. Read the issue (GitHub MCP tools) and check for existing branches/PRs for it; stop and report if one already exists.
2. Create the branch from the latest `main` using `<type>/<issue>_<kebab-description>` (types: `feature`, `bugfix`, `hotfix`, `chore`, `docs`).
3. Check out the branch locally.

Do not implement anything. Never push to `main`.
