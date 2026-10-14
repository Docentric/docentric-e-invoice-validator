// PreToolUse guard for Claude Code. Reads the hook payload from stdin and exits with code 2
// (which blocks the tool call and shows stderr to the agent) when a protected action is attempted.
// Rules mirror AGENTS.md: no edits to generated files or secrets, no commits or pushes on main,
// no force pushes. Requires only Node.js, which the project already needs for the web UI.
import { execFileSync } from "node:child_process";

const PROTECTED_FILES = [
  { pattern: /(^|\/)src\/Server\/wwwroot\/assets\//i, reason: "wwwroot/assets is generated build output. Change the sources in src/WebUI and rebuild." },
  { pattern: /\.g\.cs$/i, reason: "*.g.cs files are generated. Change the generator input instead." },
  { pattern: /(^|\/)Mustang-CLI-[^/]*\.jar$/i, reason: "The Mustang CLI JAR is replaced only in a dedicated dependency upgrade (use the upgrade-dependency skill)." },
  { pattern: /(^|\/)\.env(\.[^/]*)?$/i, reason: "Environment files may contain secrets and must not be edited by the agent." },
];

const MAIN_BRANCH = /(?<![\w/-])main(?![\w-])/;

function block(message) {
  process.stderr.write(`Blocked by .claude/hooks/guard.mjs: ${message}\n`);
  process.exit(2);
}

function currentBranch(cwd) {
  try {
    return execFileSync("git", ["rev-parse", "--abbrev-ref", "HEAD"], { cwd, encoding: "utf8" }).trim();
  } catch {
    return "";
  }
}

function checkFileEdit(toolInput) {
  const rawPath = toolInput?.file_path ?? toolInput?.notebook_path ?? "";
  const path = String(rawPath).replaceAll("\\", "/");
  const match = PROTECTED_FILES.find((rule) => rule.pattern.test(path));
  if (match) {
    block(`${path}: ${match.reason}`);
  }
}

function checkShellCommand(command, cwd) {
  const isCommit = /\bgit\s+commit\b/.test(command);
  const pushArguments = command.match(/\bgit\s+push\b[^;&|]*/)?.[0];
  const isPush = pushArguments !== undefined;

  if (isPush && /(\s--force\b|\s--force-with-lease\b|\s-f\b)/.test(pushArguments)) {
    block("force pushes are not allowed.");
  }
  if (isPush && MAIN_BRANCH.test(pushArguments)) {
    block("pushing to main is not allowed. Push a feature branch and open a pull request.");
  }

  if (isPush || isCommit) {
    const branch = currentBranch(cwd);
    if (branch === "main") {
      block(`the current branch is main. Create a branch first (see Branching Strategy in AGENTS.md).`);
    }
  }
}

let raw = "";
for await (const chunk of process.stdin) {
  raw += chunk;
}

let payload;
try {
  payload = JSON.parse(raw);
} catch {
  process.exit(0);
}

const toolName = payload.tool_name ?? "";
const toolInput = payload.tool_input ?? {};

if (["Edit", "Write", "MultiEdit", "NotebookEdit"].includes(toolName)) {
  checkFileEdit(toolInput);
} else if (toolName === "Bash") {
  checkShellCommand(String(toolInput.command ?? ""), payload.cwd);
}

process.exit(0);
