---
description: Commit, push and open a pull request for the current branch following repository conventions
---
Open a pull request for the current branch (issue #${input:issue:issue number}).

1. Confirm with the user that the implementation is done as expected and has been reviewed; stop if not.
2. Run the build and tests; fix failures.
3. Commit as `#<issue> <Imperative summary>` and push the branch (never `main`).
4. Open a PR to `main` titled like the commit, with sections: Summary, Related issue (`Closes #<n>`), Changes, Testing, Breaking changes / notes.
5. Check the PR check runs and fix failures.
