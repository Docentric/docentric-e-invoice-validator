---
name: create-pull-request
description: Commit, push and open a pull request for the current branch after the user has confirmed the implementation. Use when the work is done and reviewed.
argument-hint: "[issue number]"
disable-model-invocation: true
---
Open a pull request for the current branch (issue number given as the argument; infer from the branch name if missing).

1. Confirm with the user that the implementation is done as expected and has been reviewed; stop if not. Skip the question if the user already confirmed earlier in this session.
2. Run the build and tests; fix failures.
3. Commit as `#<issue> <Imperative summary>` and push the branch (never `main`).
4. Open a PR to `main` titled like the commit. Use `.github/pull_request_template.md` as the body and fill in every section.
5. Check the PR check runs and fix failures.
