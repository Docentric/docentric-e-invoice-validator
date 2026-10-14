---
name: security-check
description: Check NuGet, .NET and npm dependencies for vulnerabilities and fix them.
---
Follow "Dependency and Security Checking" in `AGENTS.md`.

1. Run `dotnet list src/Docentric.EInvoice.Validator.slnx package --vulnerable --include-transitive`, `--deprecated` and `--outdated`.
2. In `src/WebUI` run `npm audit` and `npm audit --omit=dev`.
3. Read open Dependabot, code scanning and secret scanning alerts (GitHub MCP tools).
4. Fix at the root cause: lowest patched version, direct reference for vulnerable transitive packages, `npm audit fix` before `--force`. Do not suppress warnings.
5. Rebuild, run tests and `npm run build`.
6. Summarize findings, fixes and anything left unresolved (with advisory IDs).
