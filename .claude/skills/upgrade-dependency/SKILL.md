---
name: upgrade-dependency
description: Upgrade a NuGet/npm package or the Mustang CLI as a dedicated change.
argument-hint: "[package or Mustang CLI] [target version]"
disable-model-invocation: true
---
Upgrade the package given as the first argument to the version given as the second (ask if missing).

- Read the release notes/changelog for breaking changes and security fixes; never invent CLI options or APIs.
- Change only what the upgrade requires; for Mustang CLI replace the JAR and update every reference (project file, Dockerfile, README, code).
- For npm, commit `package-lock.json`; for NuGet, keep versions consistent across projects.
- Build with warnings as errors, run all tests, and verify the UI builds when relevant.
- Commit as `#<issue> Upgrade <package> to <version>` and note advisories or breaking changes in the PR.
