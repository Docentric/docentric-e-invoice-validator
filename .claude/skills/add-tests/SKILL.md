---
name: add-tests
description: Write xUnit v3 tests for the selected code in src/Server.Tests. Use when asked to add or extend unit tests.
---
Write tests for the selected code in `src/Server.Tests`, following the "Tests" section of `AGENTS.md`.

- Mirror the source structure and naming (`<Type>Tests`).
- xUnit v3 with `[Fact]` / `[Theory]`, built-in `Assert`, Arrange-Act-Assert.
- Cover normal, edge (empty/invalid input), cancellation and failure cases.
- Test via public APIs only; mock only external dependencies (Java process, file system).
- No loops or conditionals; deterministic and parallel-safe.

Run `dotnet test src/Docentric.EInvoice.Validator.slnx` and fix failures.
