---
description: Read a GitHub issue and create a branch following repository conventions
---
Prepare a branch for GitHub issue #${input:issue:issue number}.

1. Use the GitHub MCP tools to read the issue and check for existing branches/PRs for it; stop and report if one already exists.
2. Create the branch from the latest `main` using `<type>/<issue>_<kebab-description>` (types: `feature`, `bugfix`, `hotfix`, `chore`, `docs`).
3. Check out the branch locally.

Do not implement anything. Never push to `main`.
