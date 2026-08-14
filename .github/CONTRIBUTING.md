# Contributing

Open an issue first for non-trivial changes so we can align on direction before code.

## Build and test

```pwsh
dotnet restore Veadotube.Client.slnx
dotnet build Veadotube.Client.slnx
dotnet test tests/Veadotube.Client.Tests/Veadotube.Client.Tests.csproj
```

## Releasing

Tag `vX.Y.Z` on `main`. Publishing a GitHub release triggers the workflow's NuGet push via OIDC trusted publishing. No API keys are stored in the repo.

## House rules

- **Warnings are errors.** `TreatWarningsAsErrors` is on. Fix the diagnostic rather than suppressing
  it; a `NoWarn` or `#pragma` needs a comment saying why the rule genuinely does not apply.
- **Nullable reference types are enabled** everywhere. No `!` without a reason.
- **All I/O is async**, with a `CancellationToken` accepted and propagated. No `.Result`,
  `.GetAwaiter().GetResult()`, or `Thread.Sleep`.
- **Public API carries XML documentation.**
- **The package is trim- and AOT-clean.** `IsAotCompatible` is set, so the trim and AOT analyzers run
  on every build. Serialization goes through a source-generated `JsonSerializerContext`, never the
  reflection-based `JsonSerializer` overloads.

## Tests

- Name tests `{Method}_{Scenario}_{ExpectedResult}`.
- Prefer the purpose-built MSTest assertions (`Assert.HasCount`, `Assert.Contains`,
  `Assert.AreSequenceEqual`) over hand-rolled equality checks — the analyzers will point you at them.
- No `Thread.Sleep`. Use `TaskCompletionSource`, channels, or a fake clock.
- New behaviour needs a test. Bug fixes need a test that fails before the fix.

## Commits and pull requests

Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/):

```
fix(webhooks): reject a signature computed over the decoded body
```

Keep the subject under 50 characters and in the imperative mood. Add a body only when the reason for
the change would not be obvious to the next reader — explain *why*, not *what*.

One logical change per commit. Rebase rather than merge when updating a branch.

## Code of conduct

This project follows the [Contributor Covenant](CODE_OF_CONDUCT.md). By participating you are
expected to uphold it.

## Reporting security issues

Please do not open a public issue. See [SECURITY.md](SECURITY.md).
