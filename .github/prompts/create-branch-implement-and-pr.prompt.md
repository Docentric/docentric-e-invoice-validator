---
description: Read a GitHub issue, create a branch, implement the change, review it and open a pull request after user confirmation
---
Work on GitHub issue #${input:issue:issue number}.

1. Use the GitHub MCP tools to read the issue and check for existing branches/PRs for it.
2. Create the branch from the latest `main` using `<type>/<issue>_<kebab-description>`.
3. Implement the change following `.github/copilot-instructions.md` (code style, XML docs, tests).
4. Build and run tests; fix failures.
5. Review the implementation against the Code Review checklist in `.github/copilot-instructions.md`; fix actionable findings.
6. Summarize the implementation and review result, then ask the user whether the implementation is done as expected. Do not continue until the user confirms; apply requested changes and repeat steps 4-6 if needed.
7. Commit as `#<issue> <Imperative summary>`.
8. Open a PR to `main` titled like the commit, with sections: Summary, Related issue (`Closes #<n>`), Changes, Testing, Breaking changes / notes.
9. Check the PR check runs and fix failures.

Never push to `main`.
