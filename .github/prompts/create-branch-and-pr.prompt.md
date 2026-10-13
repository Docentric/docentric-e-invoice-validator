---
description: Read a GitHub issue, create a branch and open a pull request following repository conventions
---
Work on GitHub issue #${input:issue:issue number}.

1. Use the GitHub MCP tools to read the issue and check for existing branches/PRs for it.
2. Create the branch from the latest `main` using `<type>/<issue>_<kebab-description>`.
3. Implement the change following `.github/copilot-instructions.md` (code style, XML docs, tests).
4. Build and run tests; fix failures.
5. Commit as `#<issue> <Imperative summary>`.
6. Open a PR to `main` titled like the commit, with sections: Summary, Related issue (`Closes #<n>`), Changes, Testing, Breaking changes / notes.
7. Check the PR check runs and fix failures.

Never push to `main`.
