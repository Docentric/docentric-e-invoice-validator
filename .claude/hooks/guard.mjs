// PreToolUse guard for Claude Code. Reads the hook payload from stdin and exits with code 2
// (which blocks the tool call and shows stderr to the agent) when a protected action is attempted.
// Rules mirror AGENTS.md: no edits to generated files or secrets, no commits or pushes on main,
// no force pushes. Requires only Node.js, which the project already needs for the web UI.
import { execFileSync } from "node:child_process";
import { resolve } from "node:path";

const PROTECTED_FILES = [
  { pattern: /(^|\/)src\/Server\/wwwroot\/assets\//i, reason: "wwwroot/assets is generated build output. Change the sources in src/WebUI and rebuild." },
  { pattern: /\.g\.cs$/i, reason: "*.g.cs files are generated. Change the generator input instead." },
  { pattern: /(^|\/)Mustang-CLI-[^/]*\.jar$/i, reason: "The Mustang CLI JAR is replaced only in a dedicated dependency upgrade (use the upgrade-dependency skill)." },
  { pattern: /(^|\/)\.env(\.[^/]*)?$/i, reason: "Environment files may contain secrets and must not be edited by the agent." },
];

const MAIN_BRANCH = /(?<![\w/-])(?:refs\/heads\/)?main(?![\w-])/;

// Git accepts global options between the executable name and the subcommand.
// They are parsed here so that they cannot be used to hide a commit or push from the checks.
const OPTION_ARGUMENT = String.raw`"[^"]*"|'[^']*'|[^\s"';&|]+`;
const GIT_GLOBAL_OPTIONS = String.raw`(?:\s+(?:-C|-c|--git-dir|--work-tree|--namespace|--exec-path|--super-prefix|--config-env)(?:=|\s+)(?:${OPTION_ARGUMENT})|\s+--[\w-]+|\s+-[pPhv])*`;
const GIT_COMMIT_OR_PUSH = new RegExp(String.raw`\bgit(${GIT_GLOBAL_OPTIONS})\s+(commit|push)\b([^;&|]*)`, "g");
const GIT_CHANGE_DIRECTORY = new RegExp(String.raw`(?:^|\s)-C\s+(${OPTION_ARGUMENT})`, "g");

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

function unquote(value) {
  return /^(".*"|'.*')$/.test(value) ? value.slice(1, -1) : value;
}

function effectiveDirectory(globalOptions, cwd) {
  let directory = cwd || process.cwd();
  for (const match of globalOptions.matchAll(GIT_CHANGE_DIRECTORY)) {
    directory = resolve(directory, unquote(match[1]));
  }
  return directory;
}

function checkShellCommand(command, cwd) {
  for (const [, globalOptions, subcommand, argumentsText] of command.matchAll(GIT_COMMIT_OR_PUSH)) {
    if (subcommand === "push") {
      if (/(\s--force\b|\s--force-with-lease\b|\s-f\b)/.test(argumentsText)) {
        block("force pushes are not allowed.");
      }
      if (MAIN_BRANCH.test(argumentsText)) {
        block("pushing to main is not allowed. Push a feature branch and open a pull request.");
      }
    }

    if (currentBranch(effectiveDirectory(globalOptions, cwd)) === "main") {
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
