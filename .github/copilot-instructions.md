# Copilot Instructions

All general project rules (layout, code style, branching, commits, pull requests, tests, frontend, build, pipelines, GitHub access, dependency security) are in [`AGENTS.md`](../AGENTS.md) at the repository root. Follow them. Do not duplicate them here.

Reusable workflows are skills in `.claude/skills/` (invoke with `/<name>`).

## Code Review

Applies to human and Copilot reviews. Review the diff against `AGENTS.md` and this checklist; comment only on actionable findings, ordered by severity (security/correctness > contracts > tests > style).

Checklist:
- **Scope:** one issue per PR, no unrelated refactoring, branch/commit/PR title follow the conventions in `AGENTS.md`.
- **Correctness:** null/empty input, cancellation (`CancellationToken` passed through), error paths return a defined `ErrorCode`, no sync-over-async.
- **Contracts:** no breaking change to REST contracts or `ErrorCode` values unless requested; Swagger metadata, `ErrorCode.Examples.md` and README updated when behavior changes.
- **Security:** uploads validated (type, size, path traversal), temp files always deleted, no shell string concatenation for Java/Mustang, no secrets or file contents in logs, no stack traces or internal paths in responses.
- **Style:** `.editorconfig`, naming, least-exposure access modifiers, XML docs on every public member, TSDoc on exported frontend symbols.
- **Tests:** new/changed behavior has deterministic tests including edge cases and regressions; no loops/conditionals in tests.
- **Dependencies:** new packages justified; `package-lock.json` committed with `package.json`; no suppressed audit warnings.
- **Pipeline:** workflow changes keep SHA-pinned actions, least-privilege permissions and `env:`-passed untrusted values.
- **Generated files:** no manual edits to `wwwroot/assets/*`, `*.g.cs` or the Mustang JAR (outside a dedicated upgrade).

Review etiquette:
- Prefix non-blocking remarks with `nit:`; explain the *why* and suggest a concrete fix.
- Do not approve while CI is failing or blocking comments are unresolved.
- Authors address feedback with follow-up commits and resolve threads once fixed.
