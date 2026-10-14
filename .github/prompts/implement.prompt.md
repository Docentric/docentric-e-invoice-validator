---
description: Implement the GitHub issue on the current branch following repository conventions
---
Implement GitHub issue #${input:issue:issue number} on the current branch.

1. Use the GitHub MCP tools to read the issue.
2. Implement the change following `.github/copilot-instructions.md` (code style, XML docs, tests, docs updates).
3. Build and run tests; fix failures.
4. Summarize the changes and ask the user whether the implementation is done as expected.

Do not commit, push or open a PR. Never work on `main`.
