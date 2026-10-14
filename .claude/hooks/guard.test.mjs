// Regression tests for guard.mjs. Run with: node --test .claude/hooks/guard.test.mjs
import assert from "node:assert/strict";
import { execFileSync, spawnSync } from "node:child_process";
import { mkdtempSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { after, before, describe, it } from "node:test";
import { fileURLToPath } from "node:url";

const guardPath = fileURLToPath(new URL("./guard.mjs", import.meta.url));

function runGuard(toolName, toolInput, cwd) {
  const result = spawnSync("node", [guardPath], {
    input: JSON.stringify({ tool_name: toolName, tool_input: toolInput, cwd }),
    encoding: "utf8",
  });
  return result.status;
}

function runBash(command, cwd) {
  return runGuard("Bash", { command }, cwd);
}

describe("guard.mjs", () => {
  let mainRepo;
  let featureRepo;

  before(() => {
    mainRepo = mkdtempSync(join(tmpdir(), "guard-main-"));
    featureRepo = mkdtempSync(join(tmpdir(), "guard-feature-"));
    for (const [repo, branch] of [[mainRepo, "main"], [featureRepo, "feature/1_test"]]) {
      execFileSync("git", ["init", `--initial-branch=${branch}`], { cwd: repo });
      // A branch name is only reported once the repository has a first commit.
      execFileSync("git", ["-c", "user.name=Test", "-c", "user.email=test@example.com", "commit", "--allow-empty", "-m", "init"], { cwd: repo });
    }
  });

  after(() => {
    rmSync(mainRepo, { recursive: true, force: true });
    rmSync(featureRepo, { recursive: true, force: true });
  });

  const blockedFromFeatureBranch = [
    "git push --force origin feature/1_test",
    "git -C /some/repo push --force origin feature/1_test",
    "git -c core.editor=true push -f origin feature/1_test",
    "git --no-pager push origin main",
    "git -C /some/repo push origin main",
    "git push origin HEAD:refs/heads/main",
    "git --git-dir=.git push --force-with-lease origin feature/1_test",
  ];

  for (const command of blockedFromFeatureBranch) {
    it(`blocks "${command}"`, () => {
      assert.equal(runBash(command, featureRepo), 2);
    });
  }

  it("allows a normal push of a feature branch", () => {
    assert.equal(runBash("git push origin feature/1_test", featureRepo), 0);
  });

  it("blocks a commit on main", () => {
    assert.equal(runBash("git commit -m test", mainRepo), 2);
  });

  it("blocks a commit on main when git -C points to the main repository", () => {
    assert.equal(runBash(`git -C "${mainRepo}" commit -m test`, featureRepo), 2);
  });

  it("allows a commit on a feature branch when git -C points to it", () => {
    assert.equal(runBash(`git -C ${featureRepo} commit -m test`, mainRepo), 0);
  });

  it("blocks edits to generated files", () => {
    assert.equal(runGuard("Edit", { file_path: "src/Server/wwwroot/assets/index.js" }, featureRepo), 2);
  });
});
