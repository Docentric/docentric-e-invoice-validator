---
name: create-branch-implement-and-pr
description: Run the full issue workflow end to end. Creates the branch, implements, reviews, asks the user to confirm, then opens the pull request. Use only when the user wants the whole flow. For a single step use create-branch, implement, review-implementation or create-pull-request.
argument-hint: "[issue number]"
disable-model-invocation: true
---
Work on the GitHub issue given as the argument (ask for the number if missing). Run these skills in order and follow their instructions exactly. Do not repeat their steps here.

1. `create-branch`: stop and report if a branch or PR already exists for the issue.
2. `implement`: implement, build and test. Skip its closing question, because the confirmation happens in step 4.
3. `review-implementation`: fix actionable findings, then rebuild and retest.
4. Summarize the implementation and the review result, then ask the user whether the implementation is done as expected. Do not continue until the user confirms. If changes are requested, apply them and repeat steps 3 and 4.
5. `create-pull-request`: the user has confirmed in step 4, so do not ask again.

Never push to `main`.
