---
name: review-pull-request
description: Review a GitHub pull request using the repository code review checklist.
argument-hint: "[PR number]"
---
Review the pull request given as the argument (ask for the number if missing) using the GitHub MCP tools (details, diff, files, check runs, existing review comments).

Apply the "Code Review" checklist in `.github/copilot-instructions.md`.

Output:
1. Short summary of the change.
2. Findings grouped as Blocking / Suggestions / Nits, each with file, line and a concrete fix.
3. CI status and missing tests/docs.
4. Recommendation: approve, comment or request changes.

Do not submit the review unless asked.
