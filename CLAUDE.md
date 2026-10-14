@AGENTS.md

## Claude Code Notes

- Code review checklist: `.github/copilot-instructions.md` (section "Code Review").
- Skills are in `.claude/skills/`; MCP servers are configured in `.mcp.json` (requires the `GITHUB_PERSONAL_ACCESS_TOKEN` environment variable).
- `.claude/settings.json` defines shared permissions and a `PreToolUse` hook (`.claude/hooks/guard.mjs`, needs Node.js). The hook blocks edits to generated files, the Mustang JAR and `.env` files, and blocks force pushes and commits or pushes on `main`. Personal overrides go in `.claude/settings.local.json`, which is not committed.
