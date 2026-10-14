---
name: review-implementation
description: Review the local implementation against the Code Review checklist before opening a pull request.
---
Review the uncommitted and unpushed changes on the current branch (diff against `main`).

1. Apply the Code Review checklist in `.github/copilot-instructions.md`.
2. Run the build and tests; report failures.
3. Report only actionable findings ordered by severity (security/correctness > contracts > tests > style), with a concrete fix for each.
4. Fix findings the user approves.

Do not commit, push or open a PR.
