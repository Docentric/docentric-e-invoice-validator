---
name: create-branch-implement-and-pr
description: Read a GitHub issue, create a branch, implement the change, review it and open a pull request after user confirmation.
argument-hint: "[issue number]"
disable-model-invocation: true
---
Work on the GitHub issue given as the argument (ask for the number if missing).

1. Read the issue (GitHub MCP tools) and check for existing branches/PRs for it.
2. Create the branch from the latest `main` using `<type>/<issue>_<kebab-description>`.
3. Implement the change following `AGENTS.md` (code style, XML docs, tests).
4. Build and run tests; fix failures.
5. Review the implementation against the Code Review checklist in `.github/copilot-instructions.md`; fix actionable findings.
6. Summarize the implementation and review result, then ask the user whether the implementation is done as expected. Do not continue until the user confirms; apply requested changes and repeat steps 4-6 if needed.
7. Commit as `#<issue> <Imperative summary>`.
8. Open a PR to `main` titled like the commit, with sections: Summary, Related issue (`Closes #<n>`), Changes, Testing, Breaking changes / notes.
9. Check the PR check runs and fix failures.

Never push to `main`.
