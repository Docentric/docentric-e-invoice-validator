---
name: commit-message
description: Write a commit message in the repository format for the current staged changes (or working tree). Use when the user asks for a commit message. Only returns text and does not commit.
---
Write a commit message for the current staged changes (or the working tree if nothing is staged).

- Format: `#<issue> <Imperative summary>`; infer the issue number from the branch name, otherwise use `#noissue`.
- Capitalized imperative summary, no trailing period, about 72 characters or less.
- Add a body only if the motivation, trade-offs or breaking changes are not obvious.
- If the changes mix unrelated concerns, say so and suggest how to split them.

Return only the message.
