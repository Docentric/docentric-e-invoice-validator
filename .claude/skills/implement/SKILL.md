---
name: implement
description: Implement a GitHub issue on the current branch following repository conventions.
argument-hint: "[issue number]"
disable-model-invocation: true
---
Implement the GitHub issue given as the argument (ask for the number if missing) on the current branch.

1. Read the issue (GitHub MCP tools).
2. Implement the change following `AGENTS.md` (code style, XML docs, tests, docs updates).
3. Build and run tests; fix failures.
4. Summarize the changes and ask the user whether the implementation is done as expected.

Do not commit, push or open a PR. Never work on `main`.
